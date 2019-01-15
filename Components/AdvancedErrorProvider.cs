#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KGySoft.Controls.Reflection;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Provides a <see cref="SetMessage"/> event, which is triggered if the <see cref="ErrorProvider.DataSource"/> property is set and the message
    /// of a bound property is about to be retrieved.
    /// </summary>
    /// <seealso cref="ErrorProvider" />
    [ToolboxBitmap(typeof(ErrorProvider))]
    public class AdvancedErrorProvider : ErrorProvider
    {
        #region Fields

        private BindingManagerBase lastManager;
        private EventHandler<SetMessageEventArgs> setMessageHandler;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ErrorProvider.DataSource"/> property is set and the message of a bound property is about to be retrieved.
        /// </summary>
        [Category(nameof(AdvancedErrorProvider))]
        [Description("Occurs when the DataSource property is set and the message of a bound property is about to be retrieved.")]
        public event EventHandler<SetMessageEventArgs> SetMessage
        {
            add => setMessageHandler += value;
            remove => setMessageHandler -= value;
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether binding errors should be shown by this <see cref="AdvancedErrorProvider"/>.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        [Category(nameof(AdvancedErrorProvider))]
        [Description("Gets or sets whether binding errors should be shown by this AdvancedErrorProvider.")]
        public bool ShowBindingErrors { get; set; } = true;

        #endregion

        #region Private Properties

        private BindingManagerBase BindingManager => (BindingManagerBase)Accessors.ErrorProvider_errorManager.Get(this);

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class with the default settings.
        /// </summary>
        public AdvancedErrorProvider() => Initilize();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class attached to a container.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        public AdvancedErrorProvider(ContainerControl parentControl) : base(parentControl) => Initilize();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class attached to an <see cref="IContainer"/> implementation.
        /// </summary>
        /// <param name="container">The container.</param>
        public AdvancedErrorProvider(IContainer container) : base(container) => Initilize();

        private void RewireEvents(BindingManagerBase bindingManager)
        {
            if (lastManager != null)
                UnwireRedirectedEvents(lastManager);

            lastManager = bindingManager;

            if (bindingManager == null)
                return;

            // removing the originally set event handlers
            this.UnwireEvents(bindingManager);

            // wiring the fixed event handlers
            bindingManager.CurrentChanged += BindingManager_CurrentChanged;
            bindingManager.BindingComplete += BindingManager_BindingComplete;
            if (bindingManager is CurrencyManager currencyManager)
            {
                currencyManager.ItemChanged += CurrencyManager_ItemChanged;
                currencyManager.Bindings.CollectionChanged += CurrencyManager_BindingsCollectionChanged;
            }

            // as we are coming from a newly triggered CurrentChanged we let the rewired handler to go
            ApplyMessagesFromBinding();
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Provides a method to update the bindings of the <see cref="ErrorProvider.DataSource" />, <see cref="ErrorProvider.DataMember" />, and the error text.
        /// </summary>
        public new void UpdateBinding()
        {
            // Unfortunately this method is not virtual so if someone calls the base it may behave differently.
            ApplyMessagesFromBinding();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Releases this <see cref="AdvancedErrorProvider" /> instance.
        /// </summary>
        /// <param name="disposing"><see langword="true"/>&#160;to release both managed and unmanaged resources; <see langword="false"/>&#160;to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            setMessageHandler = null;
            if (lastManager != null && disposing)
            {
                UnwireRedirectedEvents(lastManager);
                lastManager = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Raises the <see cref="SetMessage" /> event.
        /// </summary>
        /// <param name="e">The <see cref="SetMessageEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSetMessage(SetMessageEventArgs e) => setMessageHandler?.Invoke(this, e);

        #endregion

        #region Private Methods

        private void Initilize()
        {
            // Replacing the method of the base.currentChanged delegate, which will help us to rewire the other events whenever the data source is changed.
            // This can be auto detected only if the data source notifies about changes, like a BindingSource. Otherwise, we can only hope that consumers
            // call the DataSource and DataMember of this class.
            // ReSharper disable once ConvertToLocalFunction - it will be converted to delegate anyway
            EventHandler injectedCurrentChanged = InjectedCurrentChanged;
            Accessors.ErrorProvider_currentChanged.Set(this, injectedCurrentChanged);
        }

        private void UnwireRedirectedEvents(BindingManagerBase manager)
        {
            manager.CurrentChanged -= BindingManager_CurrentChanged;
            manager.BindingComplete -= BindingManager_BindingComplete;
            if (manager is CurrencyManager currencyManager)
            {
                currencyManager.ItemChanged -= CurrencyManager_ItemChanged;
                currencyManager.Bindings.CollectionChanged -= CurrencyManager_BindingsCollectionChanged;
            }
        }

        private void ApplyMessagesFromBinding()
        {
            BindingManagerBase bindingManager = BindingManager;
            if (bindingManager == null || bindingManager.Count == 0)
                return;

            BindingsCollection bindings = bindingManager.Bindings;
            object currentItem = bindingManager.Current;

            // Collecting the messages for the controls
            Dictionary<Control, StringBuilder> controlMessages = new Dictionary<Control, StringBuilder>(bindings.Count);
            foreach (Binding binding in bindings)
            {
                // Ignore everything but bindings to Controls
                var control = binding.Control;
                if (control == null)
                    continue;

                string propertyName = binding.BindingMemberInfo.BindingField;
                var args = new SetMessageEventArgs(currentItem, propertyName, currentItem is IDataErrorInfo info ? info[propertyName] : null);
                OnSetMessage(args);

                if (!controlMessages.TryGetValue(control, out StringBuilder message))
                    controlMessages[control] = new StringBuilder(args.Message ?? String.Empty);
                else if (!String.IsNullOrEmpty(args.Message))
                {
                    message.AppendLine();
                    message.Append(args.Message);
                }
            }

            foreach (var entry in controlMessages)
                SetError(entry.Key, entry.Value.ToString());
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// This is the new target of the base.currentChanged delegate field. If this is invoked, we can sure that the base manager is not fixed yet.
        /// </summary>
        private void InjectedCurrentChanged(object sender, EventArgs eventArgs)
        {
            RewireEvents((BindingManagerBase)sender);
        }

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_CurrentChanged method
        /// </summary>
        private void BindingManager_CurrentChanged(object sender, EventArgs e) => ApplyMessagesFromBinding();

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_BindingsChanged method
        /// </summary>
        private void CurrencyManager_BindingsCollectionChanged(object sender, CollectionChangeEventArgs e) => ApplyMessagesFromBinding();

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_BindingComplete method
        /// </summary>
        private void BindingManager_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            Binding binding = e.Binding;
            if (!ShowBindingErrors || binding?.Control == null)
                return;
            var args = new SetMessageEventArgs(null, binding.PropertyName, e.ErrorText);
            OnSetMessage(args);
            SetError(binding.Control, args.Message);
        }

        private void CurrencyManager_ItemChanged(object sender, ItemChangedEventArgs e)
        {
            // This is the fixed version of the base.ErrorManager_ItemChanged method.
            BindingManagerBase manager = BindingManager;

            // The original handler is overridden only due to this part.
            if (e.Index != -1 || manager.Count != 0)
            {
                ApplyMessagesFromBinding();
                return;
            }

            // If the list became empty then reset the errors
            foreach (Binding binding in manager.Bindings)
            {
                if (binding.Control != null)
                    SetError(binding.Control, null);
            }
        }

        #endregion

        #endregion
    }
}
