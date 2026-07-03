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
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Controls;
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
    public class AdvancedErrorProvider : ErrorProvider, IPerMonitorDpiAware
    {
        #region Fields

        private BindingManagerBase? lastManager;
        private IDisposable? dpiChangeNotifier;
        private Icon? customIcon; // assigned by the caller, never should be disposed
        private Icon? adjustedIcon; // always generated, should be always disposed
        private IconSizeMode iconSizeMode = IconSizeMode.AutoScale;
        private Size currentScaledSize; // not necessarily the actual size, but the actual calculated one

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

        /// <inheritdoc cref="ErrorProvider.ContainerControl"/>
        public new ContainerControl? ContainerControl
        {
            get => base.ContainerControl;
            set
            {
                base.ContainerControl = value;
                dpiChangeNotifier?.Dispose();
                dpiChangeNotifier = null;
                if (customIcon != null && iconSizeMode != IconSizeMode.SystemDefault)
                    dpiChangeNotifier = value?.RegisterPerMonitorAwarenessNotifications(this);
                ResetIcon(false);
            }
        }

        /// <summary>
        /// Gets or sets the icon for this <see cref="AdvancedErrorProvider"/>.
        /// Explicitly set icons can be scaled automatically, depending on the <see cref="IconSizeMode"/> property.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IconSizeMode"/> property for details.
        /// </summary>
        [Category(nameof(AdvancedErrorProvider))]
        [Description("Gets or sets the icon for this AdvancedErrorProvider. Explicitly set icons can be scaled automatically, depending on the IconSizeMode property.")]
        public new Icon Icon
        {
            get => customIcon ?? base.Icon;
            set
            {
                customIcon = value ?? throw new ArgumentNullException(nameof(value), PublicResources.ArgumentNull);
                if (iconSizeMode != IconSizeMode.SystemDefault)
                    dpiChangeNotifier ??= ContainerControl?.RegisterPerMonitorAwarenessNotifications(this);

                currentScaledSize = default;
                ResetIcon(true);
            }
        }

        /// <summary>
        /// Gets or sets the icon sizing behavior for explicitly set icons.
        /// <br/>Default value: <see cref="IconSizeMode.AutoScale"/>.
        /// </summary>
        /// <remarks>
        /// <para>If the value of this property is <see cref="IconSizeMode.SystemDefault"/>, the behavior is framework dependent.
        /// Older frameworks may simply pick the smallest image from the assigned icon, which still may be too large if the icon contains only
        /// a large image. Starting with .NET 8.0 the <see cref="ErrorProvider"/> provider class supports multi-resolution icons,
        /// though only the existing resolutions are used, similarly to the <see cref="IconSizeMode.GetNearestSize"/> mode.</para>
        /// <para>If the property is set to <see cref="IconSizeMode.AutoScale"/> or <see cref="IconSizeMode.GetNearestSize"/>,
        /// the reference size on 100% DPI is 16 x 16 pixels.</para>
        /// <para>To make auto-scaling work for per-monitor DPI awareness, the <see cref="ContainerControl"/> property must not be <see langword="null"/>.
        /// The Windows Forms designer sets the <see cref="ContainerControl"/> automatically if you add the <see cref="AdvancedErrorProvider"/>
        /// to the form or user control from the Toolbox.</para>
        /// <para>To autoscale the default icon as well, just get and set it explicitly: <c>myErrorProvider.Icon = myErrorProvider.Icon;</c></para>
        /// <note type="tip">It is recommended to use multi-resolution icons in a DPI aware application. You can use the predefined icons
        /// from the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Icons.htm">Icons</a> class.</note>
        /// </remarks>
        [Category(nameof(AdvancedErrorProvider))]
        [Description("Gets or sets the icon sizing behavior for explicitly set icons.")]
        [DefaultValue(IconSizeMode.AutoScale)]
        public IconSizeMode IconSizeMode
        {
            get => iconSizeMode;
            set
            {
                if (iconSizeMode == value)
                    return;
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                iconSizeMode = value;
                if (customIcon == null)
                    return;

                currentScaledSize = default;
                if (value == IconSizeMode.SystemDefault)
                {
                    dpiChangeNotifier?.Dispose();
                    dpiChangeNotifier = null;
                }
                else
                    dpiChangeNotifier ??= ContainerControl?.RegisterPerMonitorAwarenessNotifications(this);

                ResetIcon(true);
            }
        }

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

            if (disposing)
            {
                Events.Dispose();
                dpiChangeNotifier?.Dispose();
                adjustedIcon?.Dispose();
            }

            customIcon = null;
            adjustedIcon = null;
            base.Dispose(disposing);
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

        private void ResetIcon(bool iconChanged)
        {
            if (customIcon == null)
                return;

            switch (iconSizeMode)
            {
                case IconSizeMode.SystemDefault:
                    if (iconChanged)
                        SetIcon(customIcon);
                    return;

                case IconSizeMode.AutoScale:
                case IconSizeMode.GetNearestSize:
                    var scale = ContainerControl?.GetScale() ?? ScaleHelper.SystemScale;
                    var size = IconsHelper.SmallIconReferenceSize.Scale(scale);
                    if (!iconChanged && size == currentScaledSize)
                        return;

                    adjustedIcon?.Dispose();
                    SetIcon(iconSizeMode == IconSizeMode.AutoScale
                        ? customIcon.Resize(size)
                        : customIcon.ExtractNearestIcon(size, PixelFormat.Format32bppArgb));

                    currentScaledSize = size;
                    return;

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected size mode: {iconSizeMode}"));

            }
        }

        private void SetIcon(Icon icon)
        {
#if NETFRAMEWORK && !NET47_OR_GREATER
            // On .NET Framework [3.5..4.7) the icon image gets corrupted if its size is not divisible by 16.
            // It's because the internally generated Region size must be divisible by 16, which is ensured on NET47+ only.
            if (OSHelper.IsWindows && !OSHelper.IsMono && icon.GetImagesCount() == 1
#if !NET35
                && !OSHelper.IsNet47OrLater // if the actually installed framework is .NET Framework 4.7+, then we don't need the fix
#endif
                )
            {
                int size = icon.Width;
                int mod = size & 0xF;
                if (mod != 0)
                {
                    using Bitmap iconImage = icon.ExtractBitmap(0)!;
                    if (icon != customIcon)
                        icon.Dispose();

                    // creating a larger icon without scaling so apparently it will have the same size as the original one
                    icon = iconImage.ToIcon(size + (16 - mod), ScalingMode.NoScaling);
                }
            }
#endif

            if (icon != customIcon)
                adjustedIcon = icon;
            base.Icon = icon;
        }

        private bool ShouldSerializeIcon() => customIcon != null;

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanging() { }
        void IPerMonitorDpiAware.ParentFormDpiChanged() => ResetIcon(false);

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
