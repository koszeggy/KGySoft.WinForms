#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedErrorProvider.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms 
#pragma warning disable CS8602 // Dereference of a possibly null reference - inconsistent nullability annotations on different platforms 
#endif

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// An <see cref="ErrorProvider"/> with a <see cref="SetMessage"/> event, which is triggered if the <see cref="ErrorProvider.DataSource"/> property is set and the message
    /// of a bound property is about to be retrieved.
    /// </summary>
    /// <remarks>
    /// <para>If the original <see cref="ErrorProvider"/> is used with WinForms data binding (by setting the <see cref="ErrorProvider.DataSource"/> property), the bound items
    /// must implement the <see cref="IDataErrorInfo"/> interface to make the error messages appear on the controls.
    /// The <see cref="AdvancedErrorProvider"/> class allows customizing this behavior by providing a <see cref="SetMessage"/> event, which can be handled
    /// to allow the messages to be retrieved from any custom source.</para>
    /// <para>If the bound objects implement the <see cref="IDataErrorInfo"/> interface, the error messages are preinitialized in
    /// the <see cref="SetMessageEventArgs.Message">SetMessageEventArgs.Message</see> property when the <see cref="SetMessage"/> event is raised.</para>
    /// <note type="tip">To provide error/message/info messages for objects, create three instances of this class. Set their icon accordingly (you can use the
    /// properties of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Icons.htm">Icons</a> class from <c>KGySoft.Drawing</c>),
    /// and handle the <see cref="SetMessage"/> event. You can derive the bound objects from the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_ComponentModel_ValidatingObjectBase.htm">ValidatingObjectBase</a> class
    /// (or implement the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_ComponentModel_IValidatingObject.htm">IValidatingObject</a> interface)
    /// to provide error/warning/info messages for the bound properties.</note>
    /// </remarks>
    /// <seealso cref="ErrorProvider" />
    [ToolboxBitmap(typeof(ErrorProvider))]
    public class AdvancedErrorProvider : ErrorProvider
    {
        #region Fields

        private BindingManagerBase? lastManager;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ErrorProvider.DataSource"/> property is set and the message of a bound property is about to be retrieved.
        /// </summary>
        [Category(nameof(AdvancedErrorProvider))]
        [Description("Occurs when the DataSource property is set and the message of a bound property is about to be retrieved.")]
        public event EventHandler<SetMessageEventArgs>? SetMessage
        {
            add => Events.AddHandler(nameof(SetMessage), value);
            remove => Events.RemoveHandler(nameof(SetMessage), value);
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

        private BindingManagerBase? BindingManager => this.GetErrorManager();

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class with default settings.
        /// </summary>
        public AdvancedErrorProvider() => Initialize();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class attached to a container.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        public AdvancedErrorProvider(ContainerControl parentControl) : base(parentControl) => Initialize();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedErrorProvider" /> class attached to an <see cref="IContainer"/> implementation.
        /// </summary>
        /// <param name="container">The container.</param>
        public AdvancedErrorProvider(IContainer container) : base(container) => Initialize();

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
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (lastManager != null && disposing)
            {
                UnwireRedirectedEvents(lastManager);
                lastManager = null;
            }

            base.Dispose(disposing);
            Events.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="SetMessage" /> event.
        /// </summary>
        /// <param name="e">The <see cref="SetMessageEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSetMessage(SetMessageEventArgs e) => Events.GetHandler<EventHandler<SetMessageEventArgs>>(nameof(SetMessage))?.Invoke(this, e);

        #endregion

        #region Private Methods

        private void Initialize()
        {
            // Replacing the method of the base.currentChanged delegate, which will help us to rewire the other events whenever the data source is changed.
            // This can be auto-detected only if the data source notifies about changes, like a BindingSource. Otherwise, we can only hope that consumers
            // call the DataSource and DataMember of this class.
            // ReSharper disable once ConvertToLocalFunction - it will be converted to delegate anyway
            EventHandler injectedCurrentChanged = InjectedCurrentChanged;
            this.SetCurrentChanged(injectedCurrentChanged);
        }

        private void RewireEvents(BindingManagerBase? bindingManager)
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
            BindingManagerBase? bindingManager = BindingManager;
            if (bindingManager == null || bindingManager.Count == 0)
                return;

            BindingsCollection bindings = bindingManager.Bindings;
            object? currentItem = bindingManager.Current;

            // Collecting the messages for the controls
            var controlMessages = new Dictionary<Control, StringBuilder>(bindings.Count);
            foreach (Binding binding in bindings)
            {
                // Ignore everything but bindings to Controls
                var control = binding.Control;
                if (control == null)
                    continue;

                string propertyName = binding.BindingMemberInfo.BindingField;
                var args = new SetMessageEventArgs(currentItem, propertyName, currentItem is IDataErrorInfo info ? info[propertyName] : null);
                OnSetMessage(args);
                if (args.Cancel)
                    continue;

                if (!controlMessages.TryGetValue(control, out StringBuilder? message))
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
        /// This is the new target of the base.currentChanged delegate field. If this is invoked, we can be sure that the base manager is not fixed yet.
        /// </summary>
        private void InjectedCurrentChanged(object? sender, EventArgs eventArgs) => RewireEvents(sender as BindingManagerBase);

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_CurrentChanged method
        /// </summary>
        private void BindingManager_CurrentChanged(object? sender, EventArgs e) => ApplyMessagesFromBinding();

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_BindingsChanged method
        /// </summary>
        private void CurrencyManager_BindingsCollectionChanged(object? sender, CollectionChangeEventArgs e) => ApplyMessagesFromBinding();

        /// <summary>
        /// This is the fixed version of the base.ErrorManager_BindingComplete method
        /// </summary>
        private void BindingManager_BindingComplete(object? sender, BindingCompleteEventArgs e)
        {
            Binding? binding = e.Binding;
            if (!ShowBindingErrors || binding?.Control == null)
                return;
            var args = new SetMessageEventArgs(null, binding.PropertyName, e.ErrorText);
            OnSetMessage(args);
            if (!args.Cancel)
                SetError(binding.Control, args.Message);
        }

        private void CurrencyManager_ItemChanged(object? sender, ItemChangedEventArgs e)
        {
            // This is the fixed version of the base.ErrorManager_ItemChanged method.
            BindingManagerBase? manager = BindingManager;
            if (manager == null)
                return;

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
