#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseForm.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
#if !NET5_0_OR_GREATER
using System.Collections.Specialized;
#endif
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Libraries.Language;
using KGySoft.WinForms.Controls;
#if !NET5_0_OR_GREATER
using KGySoft.Reflection;
using KGySoft.WinForms.Reflection;
#endif
using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if NETFRAMEWORK && !NET47_OR_GREATER
#pragma warning disable CS1574 // the documentation contains types that are not available in every target
#endif

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// A base form with additional features and bug fixes.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseForm"/> class provides the following features and changes:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the form is disposed. To do that for the events of derived forms as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item>Advanced per-monitor high DPI support on all target platforms. See the <see cref="DeviceScale"/> property
    /// and the <see cref="DeviceScaleChanged"/>, <see cref="DeviceScaleChanging"/> and <see cref="DeviceScaleAutoResized"/> events.</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only (which may indirectly affect also size and content scaling if <see cref="ContainerControl.AutoScaleMode"/> is <see cref="AutoScaleMode.Font"/>),
    /// but basically auto-sizing behavior still depends on the current platform.</item>
    /// <item>The <see cref="DynamicStringLocalization"/> property allows creating dynamically generated localizations for any language.</item>
    /// <item><see cref="CommandBindings"/> property. See the <a href="https://kgysoft.net/corelibraries#command-binding" target="_blank">online documentation</a> for details.</item>
    /// <item>Advanced MDI application support, see the <see cref="ShowMdiChild"/> method and <see cref="OwnedMdiChildClosed"/> and <see cref="PaintMdiClientArea"/> events.</item>
    /// <item>Fixes a <a href="https://github.com/dotnet/winforms/issues/1504" target="_blank">resizing bug</a> that exists in .NET Framework and .NET Core 3.x that can occur with multiple displays.</item>
    /// <item><see cref="ToolTip"/> property to create tool tips for the controls on the form.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// </list>
    /// </remarks>
    public class BaseForm : Form, IPerMonitorDpiAware, IObservableParent
    {
        #region Nested Classes

        #region ControlCollection class

        /// <summary>
        /// Represents a collection of controls contained within a <see cref="BaseForm"/>.
        /// </summary>
        protected new class ControlCollection : Form.ControlCollection
        {
            #region Fields

            private readonly BaseForm owner;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlCollection"/> class with the specified owner.
            /// </summary>
            /// <param name="owner">The <see cref="BaseForm"/> that owns this collection.</param>
            public ControlCollection(BaseForm owner)
                : base(owner ?? throw new ArgumentNullException(nameof(owner), PublicResources.ArgumentNull))
            {
                this.owner = owner;
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public override void Add(Control value)
            {
                owner.isAddingControl = true;
                try
                {
                    base.Add(value);
                }
                finally
                {
                    owner.isAddingControl = false;
                }
            }

            #endregion
        }

        #endregion

        #region ControlCollection class

        /// <summary>
        /// Needed for Mono compatibility, because Form.ControlCollection.Add casts every Control to Form on Mono.
        /// </summary>
        private sealed class ControlCollectionMono : Control.ControlCollection
        {
            #region Fields

            private readonly BaseForm owner;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ControlCollection"/> class with the specified owner.
            /// </summary>
            /// <param name="owner">The <see cref="BaseForm"/> that owns this collection.</param>
            public ControlCollectionMono(BaseForm owner)
                : base(owner ?? throw new ArgumentNullException(nameof(owner), PublicResources.ArgumentNull))
            {
                this.owner = owner;
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public override void Add(Control value)
            {
                owner.isAddingControl = true;
                try
                {
                    base.Add(value);
                }
                finally
                {
                    owner.isAddingControl = false;
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields

#if !NET5_0_OR_GREATER
        private static readonly BitVector32.Section formStateRenderSizeGrip;
#endif

        #endregion

        #region Instance Fields

        #region Protected Fields
        
        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.ToolTip"/> of the <see cref="BaseForm"/>.
        /// Kept for compatibility, if a derived form uses it from the designer.
        /// From code, prefer using the <see cref="ToolTip"/> property instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected readonly ToolTip BaseToolTip;

        #endregion

        #region Private Fields
        
        private readonly CommandBindingsCollection commandBindings = new WinFormsCommandBindingsCollection();
        private readonly InvokeMarshaller invoker;
        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the form because an instance is tied to the same thread

        private bool translateControls;
        private bool isLoaded;
        private bool autoScaleFont = true;
        private Form? suspendingMdiChild;
        private HashSet<Form>? ownedMdiChildren;
        private MdiClient? mdiClient;
        private DynamicStringLocalization localizationMode;

        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent/default font is used.

        private PointF deviceScale = ScaleHelper.SystemScale;
        private PointF previousScale;
        private Icon? smallIcon;
        private int dpiChangeRecursionCount; // Reentrancy count for WM_DPICHANGED[_BEFOREPARENT] messages processing.
        private Rectangle dpiChangedSuggestedBounds; // must be a field to handle reentrancy and for the triggering conditions of the DeviceScaleAutoResized event
        private PointF lastScaleAsChild; // Plays a role when this form is not a top-level form, i.e. when it is an MDI child. Otherwise, DeviceScale is used.
        private int dpiChangingAsChildCount; // Plays a role when this form is an MDI child. Otherwise, DPI changes are processed in WndProc.
        private bool isOnAutoResizedPending;
        private bool suppressFontChanged;
        private bool isAddingControl;
        private bool isChangingFont;

        #endregion

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an MDI child shown by a <see cref="ShowMdiChild">ShowMdiChild</see> method call is closed.
        /// </summary>
        /// <remarks>
        /// <note type="warning">This event is now obsolete. Its sender is not this form but the one that was closed.
        /// It is cleaner to use the new <see cref="OwnedMdiChildClosed"/> event instead.</note>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs when an MDI child shown by a ShowMdiChild call is closed.")]
        [Obsolete("Use the OwnedMdiChildClosed event instead.")]
        [Browsable(false)]
        public event FormClosedEventHandler? CalledMdiChildClosed
        {
            add => Events.AddHandler(nameof(CalledMdiChildClosed), value);
            remove => Events.RemoveHandler(nameof(CalledMdiChildClosed), value);
        }

        /// <summary>
        /// Occurs when an MDI child shown by the <see cref="ShowMdiChild">ShowMdiChild</see> method is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI child shown by a ShowMdiChild call is closed.")]
        public event EventHandler<OwnedMdiChildClosedEventArgs>? OwnedMdiChildClosed
        {
            add => Events.AddHandler(nameof(OwnedMdiChildClosed), value);
            remove => Events.RemoveHandler(nameof(OwnedMdiChildClosed), value);
        }

        /// <summary>
        /// Occurs when MDI area of the form has to be repainted. <see cref="Form.IsMdiContainer"/> must be true to access this event.
        /// </summary>
        /// <remarks>
        /// <note type="warning">This event is now obsolete. Its sender is not this form but the <see cref="System.Windows.Forms.MdiClient"/> instance,
        /// whose <see cref="Control.Paint"/> event is subscribed when you subscribe this event. It is cleaner to use the new <see cref="MdiClient"/> property instead.</note>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs when MDI area of the form has to be repainted. IsMdiContainer must be true to access this event.")]
        [Obsolete("Use the Paint event of the MdiClient property instead.")]
        [Browsable(false)]
        public event PaintEventHandler? PaintMdiClientArea
        {
            add
            {
                MdiClient client = MdiClient ?? throw new InvalidOperationException(Res.BaseFormNotMdiContainer);
                client.Paint += value;
            }
            remove
            {
                MdiClient client = MdiClient ?? throw new InvalidOperationException(Res.BaseFormNotMdiContainer);
                client.Paint -= value;
            }
        }

        /// <summary>
        /// Occurs when an MDI Child window called by <see cref="ShowMdiChild"/> suspends the caller instance.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when an MDI Child window called by ShowMdiChild suspends the caller instance.")]
        public event EventHandler? Suspended
        {
            add => Events.AddHandler(nameof(Suspended), value);
            remove => Events.RemoveHandler(nameof(Suspended), value);
        }

        /// <summary>
        /// Occurs when the MDI Child window called by <see cref="ShowMdiChild"/> that suspended the caller instance is closed.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when the MDI Child window called by ShowMdiChild that suspended the caller instance is closed.")]
        public event EventHandler? Resumed
        {
            add => Events.AddHandler(nameof(Resumed), value);
            remove => Events.RemoveHandler(nameof(Resumed), value);
        }

        /// <summary>
        /// Occurs with per-monitor DPI awareness, when the scale of the form's display device changes, before performing the default processing of the corresponding Windows message.
        /// On platform targets where the <see cref="Form.DpiChanged"/> event exists, this event is raised before <see cref="Form.DpiChanged"/>.
        /// </summary>
        /// <remarks>
        /// <para>This event is raised only on Windows 8.1 or later, when the application has per-monitor DPI awareness.</para>
        /// <para>On platform targets where the <see cref="Form.DpiChanged"/> event is also available, this event is raised before <see cref="Form.DpiChanged"/>.</para>
        /// <note>See also the <strong>Remarks</strong> section of the <see cref="DeviceScaleChanged"/> event for more details</note>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs with per-monitor DPI awareness, when the scale of the form's display device changes, "
            + "before performing the default processing of the corresponding Windows message.")]
        public event EventHandler<DeviceScaleChangeEventArgs>? DeviceScaleChanging
        {
            add => Events.AddHandler(nameof(DeviceScaleChanging), value);
            remove => Events.RemoveHandler(nameof(DeviceScaleChanging), value);
        }

        /// <summary>
        /// Occurs with per-monitor DPI awareness, when the scale of the form's display device changes, after performing the default processing of the corresponding Windows message.
        /// On platform targets where the <see cref="Form.DpiChanged"/> event exists, this event is raised after <see cref="Form.DpiChanged"/>.
        /// </summary>
        /// <remarks>
        /// <para>This event is raised only on Windows 8.1 or later, when the application has per-monitor DPI awareness.</para>
        /// <para>On platform targets where the <see cref="Form.DpiChanged"/> event is also available, this event is raised after <see cref="Form.DpiChanged"/>.
        /// If you want to prevent auto-scaling by <see cref="Form.DpiChanged"/>, subscribe <see cref="Form.DpiChanged"/> as well (or override <see cref="Form.OnDpiChanged">OnDpiChanged</see>),
        /// and set <see cref="CancelEventArgs.Cancel"/> in the event arguments to <see langword="true"/>.
        /// In contrast, the arguments of the <see cref="DeviceScaleChanged"/> event cannot be canceled, but this event does not do anything automatically if not subscribed.</para>
        /// <para>Unlike in the <see cref="Form.OnGetDpiScaledSize"/> event arguments, the <see cref="DeviceScaleChangeEventArgs.SuggestedBounds">DeviceScaleChangedEventArgs.SuggestedBounds</see> property
        /// contains a scaled size even if <see cref="ContainerControl.AutoScaleMode"/> is <see cref="AutoScaleMode.None"/>.
        /// The suggested bounds still can be ignored by the subscriber of the event.</para>
        /// <note>You don't need to set the size of the form when handling this event. If you don't set the size, the suggested bounds will be applied automatically.
        /// However, if you set a different size, and per-monitor DPI awareness version is V1, Windows may forcibly reset the suggested size after setting the custom bounds.
        /// To apply a custom size with per-monitor DPI awareness V1, use the <see cref="DeviceScaleAutoResized"/> event.</note>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs with per-monitor DPI awareness, when the scale of the form's display device changes, "
            + "after performing the default processing of the corresponding Windows message.")]
        public event EventHandler<DeviceScaleChangeEventArgs>? DeviceScaleChanged
        {
            add => Events.AddHandler(nameof(DeviceScaleChanged), value);
            remove => Events.RemoveHandler(nameof(DeviceScaleChanged), value);
        }

        /// <summary>
        /// Occurs with per-monitor DPI awareness V2, before calculating the suggested bounds for the <see cref="DeviceScaleChanging "/> and <see cref="DeviceScaleChanged"/> events.
        /// Similar to the <see cref="Form.OnGetDpiScaledSize"/> method, but this is available also as an event for all .NET versions, and does not cache the result.
        /// </summary>
        /// <remarks>
        /// <para>This event is raised only on Windows 10 Build 1703 or later, when the application has per-monitor DPI awareness V2.</para>
        /// <para>By default, the <see cref="DeviceScaleGetNewSizeEventArgs.DesiredSize"/> property is initialized to the original size of the form.
        /// To apply a custom size, change the <see cref="DeviceScaleGetNewSizeEventArgs.DesiredSize"/> property, and set the <see cref="HandledEventArgs.Handled"/> property to <see langword="true"/>.
        /// If you just set the <see cref="HandledEventArgs.Handled"/> property to <see langword="true"/>, the original size will be applied as the desired size.</para>
        /// <para>On platform targets where the <see cref="Form.OnGetDpiScaledSize">OnGetDpiScaledSize</see> method is also available, this event is raised after calling
        /// the <see cref="Form.OnGetDpiScaledSize">OnGetDpiScaledSize</see> method. If a derived form returns <see langword="true"/> from an overridden <see cref="Form.OnGetDpiScaledSize">OnGetDpiScaledSize</see>
        /// method, the <see cref="DeviceScaleGetNewSizeEventArgs.DesiredSize"/> may already contain a custom size, which is indicated by the <see cref="HandledEventArgs.Handled"/> property being <see langword="true"/>.
        /// To revoke such custom resizing and apply the default scaling behavior instead, set the <see cref="HandledEventArgs.Handled"/> property to <see langword="false"/>.</para>
        /// <para>On more recent target platforms, the <see cref="HandledEventArgs.Handled"/> property may already be initialized to <see langword="true"/>, depending on the <see cref="ContainerControl.AutoScaleMode"/>
        /// property of the form. For example, if <see cref="ContainerControl.AutoScaleMode"/> is <see cref="AutoScaleMode.None"/>, this is how the original form size is preserved by default.
        /// By setting the <see cref="HandledEventArgs.Handled"/> property to <see langword="false"/>, you can fall back to the default auto-scaling behavior of the form.</para>
        /// <note>When using per-monitor DPI awareness V1, this event is not raised, and even if you set a custom size in the <see cref="DeviceScaleChanged"/> event,
        /// Windows may forcibly reset the suggested size after setting the custom bounds. To apply a custom size with per-monitor DPI awareness V1, use the <see cref="DeviceScaleAutoResized"/> event instead.</note>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs with per-monitor DPI awareness V2, before calculating the suggested bounds for the DeviceScaleChanged event. Similar to the OnGetDpiScaledSize method, "
            + "but this is available also as an event for all .NET versions, and does not cache the result.")]
        public event EventHandler<DeviceScaleGetNewSizeEventArgs>? DeviceScaleGetNewSize
        {
            add => Events.AddHandler(nameof(DeviceScaleGetNewSize), value);
            remove => Events.RemoveHandler(nameof(DeviceScaleGetNewSize), value);
        }

        /// <summary>
        /// Occurs with per-monitor DPI awareness, when the form is resized automatically after the <see cref="DeviceScaleChanged"/> event.
        /// </summary>
        /// <remarks>
        /// <para>This event is raised after Windows applied the suggested size indicated by the <see cref="DeviceScaleChangeEventArgs.SuggestedBounds"/> property
        /// in the event arguments of the <see cref="DeviceScaleChanged"/> event.</para>
        /// <para>When using per-monitor DPI awareness V2, this event is not raised if you set custom bounds in the <see cref="DeviceScaleChanging"/> or <see cref="DeviceScaleChanged"/> events.
        /// If a new custom size can be calculated in advance, it is recommended to handle the <see cref="DeviceScaleGetNewSize"/> event instead of setting custom bounds
        /// in the <see cref="DeviceScaleChanging"/> or <see cref="DeviceScaleChanged"/> events. This event still can be used to manually scale the contents of the form, for example.</para>
        /// <para>When using per-monitor DPI awareness V1, Windows always (re)applies the non-customizable "suggested" size after the <see cref="DeviceScaleChanged"/> event,
        /// even if you set custom bounds in the <see cref="DeviceScaleChanging"/> or <see cref="DeviceScaleChanged"/> events. This event is raised when it is already safe to apply a new custom size.
        /// When you drag the form to a different display, this event might be raised only after the user finished dragging the form.</para>
        /// </remarks>
        [Category("BaseForm")]
        [Description("Occurs with per-monitor DPI awareness, when the form is resized automatically after the DeviceScaleChanged event. "
            + "This event may not be raised if the application is executed with per-monitor DPI awareness V2, "
            + "and you manually set the bounds of the form in the DeviceScaleChanged event handler.")]
        public event EventHandler? DeviceScaleAutoResized
        {
            add => Events.AddHandler(nameof(DeviceScaleAutoResized), value);
            remove => Events.RemoveHandler(nameof(DeviceScaleAutoResized), value);
        }

        /// <summary>
        /// Occurs when the <see cref="DynamicStringLocalization"/> property changes.
        /// </summary>
        [Category("BaseForm")]
        [Description("Occurs when the DynamicStringLocalization property changes.")]
        public event EventHandler? DynamicStringLocalizationChanged
        {
            add => Events.AddHandler(nameof(DynamicStringLocalization), value);
            remove => Events.RemoveHandler(nameof(DynamicStringLocalization), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the form should translate its controls.
        /// <br/>This property is obsolete. Use the <see cref="DynamicStringLocalization"/> property
        /// or override the <see cref="ApplyResources">ApplyResources</see> and/or <see cref="ApplyStringResources">ApplyStringResources</see> methods.
        /// </summary>
        [Category("BaseForm")]
        [DefaultValue(false)]
        [Description("[OBSOLETE]Gets or sets whether the form should translate its controls.")]
        [Obsolete("Old auto-translation does not work anymore, it just removes the possible translation postfixes. Use the LocalizationOptions property instead.")]
        [Browsable(false)]
        public bool TranslateControls
        {
            get => translateControls;
            set => translateControls = value;
        }

        /// <summary>
        /// Gets or sets the dynamic string localization strategy of the form. It allows using potentially auto-generated string resources from .resx files.
        /// </summary>
        /// <remarks>
        /// <note>This property offers a different localization strategy to the <c>Localizable</c> property of the Windows Forms designer, and it is not recommended to use them both together.</note>
        /// <para>Unlike the <c>Localizable</c> property of the Windows Forms designer, this property affects the localization of the string properties only,
        /// and basically determines the behavior of the default <see cref="ApplyStringResources"/> implementation. You still can apply non-string resources
        /// without enabling <c>Localizable</c> by overriding the <see cref="ApplyResources"/> method, whose default implementation just calls <see cref="ApplyStringResources"/>.</para>
        /// <para>When this property is set to <see cref="DynamicStringLocalization.Disabled"/>, no automatic localization occurs. To localize string resources
        /// programmatically, you can override the <see cref="ApplyStringResources"/> method.</para>
        /// <para>The <see cref="ApplyResources"/> method is called automatically when the form is loaded for the first time, but you can explicitly call it whenever
        /// you need to re-apply the resources (or <see cref="ApplyStringResources"/> to re-apply the string resources only), for example when the display language changes.</para>
        /// <para>When the value of this property is not <see cref="DynamicStringLocalization.Disabled"/>, then the base <see cref="ApplyStringResources"/> implementation
        /// calls the <see cref="LocalizationHelper.ApplyStringResources">LocalizationHelper.ApplyStringResources</see> method, which traverses the controls of the form recursively,
        /// and invokes the <see cref="LocalizationHelper.LocalizationRequested"/> event for each localizable string property of the controls. If this property is
        /// set to <see cref="DynamicStringLocalization.Custom"/>, then you must handle the event to provide localization for the controls programmatically.
        /// When the <see cref="LocalizationHelper.LocalizationRequested"/> event does not handle a request, using <see cref="DynamicStringLocalization.LocalScope"/>
        /// or <see cref="DynamicStringLocalization.AssemblyScope"/> allow using .resx files placed in the <c>Resources</c> folder of the deployment directory
        /// that can be automatically generated for the first time when a localization request is made for a culture that has no resource file yet.</para>
        /// <para>Using <see cref="DynamicStringLocalization.LocalScope"/> or <see cref="DynamicStringLocalization.AssemblyScope"/> works only if the invariant resource set exists for the form or user control.
        /// Only the properties that have an entry in the invariant resource set will be localized automatically.
        /// <br/>See also the <see cref="KGySoft.WinForms.DynamicStringLocalization"/> enumeration for details.
        /// </para>
        /// </remarks>
        /// <example>
        /// TODO:
        /// - creating the invariant resource set
        ///   - how to use it as a compiled resource
        ///   - how to use it as a .resx file
        /// - Applying RTL
        /// - how to handle the generated localizations
        ///   - by a resource editor
        ///   - from within the application, applying the translations at runtime
        /// </example>
        [Category("BaseForm")]
        [DefaultValue(DynamicStringLocalization.Disabled)]
        [Description("Specifies the dynamic string localization strategy of the form. LocalScope and AssemblyScope allow using potentially auto-generated .resx files "
            + "and ensure that localization is automatically re-applied when LanguageSettings.DisplayLanguage is changed. They need an existing invariant resource set to work. "
            + "The Custom setting allows handling the LocalizationHelper.LocalizationRequested event to provide localization for the controls programmatically.")]
        public DynamicStringLocalization DynamicStringLocalization
        {
            get => localizationMode;
            set
            {
                if (localizationMode == value)
                    return;
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                localizationMode = value;
                OnDynamicStringLocalizationChanged(EventArgs.Empty);
                LanguageSettings.DisplayLanguageChanged -= LanguageSettings_DisplayLanguageChanged;
                if (IsDesignMode || value == DynamicStringLocalization.Disabled)
                    return;

                LanguageSettings.DisplayLanguageChanged += LanguageSettings_DisplayLanguageChanged;
                if (isLoaded)
                    ApplyStringResources();
            }
        }

        /// <summary>
        /// Gets whether the form is suspended by an owned MDI child.
        /// </summary>
        [Browsable(false)]
        public bool IsSuspended => suspendingMdiChild != null;

        /// <summary>
        /// Gets the command bindings of this form. The <see cref="O:KGySoft.ComponentModel.CommandBindingsCollection.Add">Add</see> methods also add
        /// the <see cref="PropertyCommandStateUpdater"/> to the created bindings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        /// <summary>
        /// Gets the current scale of the form's display device. Before loading the form, or when per-monitor DPI awareness is not enabled,
        /// this property returns the system scale of the primary display, which is the same as the <see cref="ScaleHelper.SystemScale">ScaleHelper.SystemScale</see> property.
        /// </summary>
        /// <remarks>
        /// <para>This property is similar to the <see cref="Control.DeviceDpi"/> property, but it returns the scale factor as a <see cref="PointF"/> value,
        /// and it is available on all .NET versions, even on .NET Framework 3.5.</para>
        /// <note>Even on platforms where the <see cref="Control.DeviceDpi"/> is available, the <see cref="Control.DeviceDpi"/> property
        /// may return an incorrect value (e.g. the .NET Framework requires the DPI awareness settings in the <c>app.config</c> file, even
        /// if the awareness is set in the application manifest). In contrast, this property always returns the correct scale
        /// if there is an application manifest file or the DPI awareness is set for the application manually.</note>
        /// </remarks>
        [Browsable(false)]
        public PointF DeviceScale => deviceScale;

        /// <inheritdoc cref="Form.Icon" />
        public new Icon? Icon
        {
            get => base.Icon;
            set
            {
                base.Icon = value;
                smallIcon?.Dispose();
                if (value == null)
                {
                    smallIcon = null;
                    return;
                }

                if (!OSHelper.IsWindows || !ScaleHelper.IsThreadPerMonitorAware)
                    return;

                // Fixing the small icon if the DPI of the form is different from the system DPI
                Debug.Assert(deviceScale == this.GetScale());
                smallIcon = value.Resize(IconsHelper.SmallIconReferenceSize.Scale(deviceScale));
                if (IsHandleCreated)
                    User32.SendMessage(Handle, Constants.WM_SETICON, Constants.ICON_SMALL, smallIcon.Handle);
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Font"/> should be automatically scaled when DPI changes and the current thread has per-monitor DPI awareness.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see langword="true"/>, the <see cref="Font"/> is automatically scaled to the current DPI of the corresponding display on every executing platform.
        /// If this is an MDI child form, it also ensures that without an explicitly set font it is inherited from <see cref="Control.Parent"/>, which would be the normal behavior, but is broken in .NET 6+ and above.</para>
        /// <para>When <see langword="false"/>, the <see cref="Font"/> may or may not be scaled, and the font of a possible parent MDI container
        /// may or may not be applied correctly, depending on the default behavior of the executing platform.</para>
        /// <note>Please note that this property directly affects autoscaling the <see cref="Font"/> property only. It still may indirectly affect scaling
        /// the whole form and its contents, if the <see cref="ContainerControl.AutoScaleMode"/> property is <see cref="AutoScaleMode.Font"/>.
        /// Scaling the size on DPI change can also be controlled by the <see cref="DeviceScaleGetNewSize"/> event,
        /// or can be set on the <see cref="DeviceScaleChanged"/> or <see cref="DeviceScaleAutoResized"/> events.</note>
        /// </remarks>
        [Category("BaseForm")]
        [DefaultValue(true)]
        [Description("True to auto scale Font when DPI changes and inherit the font when it's not explicitly set; False to rely on the default behavior of the current executing platform.")]
        public bool AutoScaleFont
        {
            get => autoScaleFont;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (autoScaleFont == value)
                    return;

                autoScaleFont = value;
                Debug.Assert(deviceScale == this.GetScale());
                font?.ResetFrom(font.Font, value ? deviceScale : ScaleHelper.SystemScale);
                if (value)
                {
                    Control? parent = Parent;
                    defaultFont = new ScalingFont(ScaleHelper.GetFontOrDefault(parent?.Font), parent?.GetScale() ?? ScaleHelper.SystemScale);

                    // theoretically this would not be needed, but in .NET 6+ the default font handling gets broken after the first DPI change
                    SetFont(font ?? defaultFont);
                    return;
                }

                defaultFont?.Dispose();
                defaultFont = null;
                if (font == null)
                    base.Font = null!;
            }
        }

        /// <inheritdoc />
        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set
            {
                Debug.Assert(AutoScaleFont ^ defaultFont == null);
                if (dpiChangingAsChildCount > 0 && AutoScaleFont)
                    return;

                // resetting the default font; or null, when AutoScaleFont is false
                if (value is null)
                {
                    font?.Dispose();
                    font = null;
                    Control? parent = Parent;
                    PointF parentScale = parent?.GetScale() ?? ScaleHelper.SystemScale;
                    defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent?.Font), parentScale);
                    SetFont(defaultFont);
                    return;
                }

                // setting a font explicitly - always setting base.Font, even if it is the same as value
                Debug.Assert(deviceScale == this.GetScale());
                PointF scale = AutoScaleFont ? deviceScale : ScaleHelper.SystemScale;
                if (font == null)
                    font = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                else
                    font.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                SetFont(font);
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets a <see cref="System.Windows.Forms.ToolTip"/> instance that can be used to show tooltips for controls of this form.
        /// </summary>
        protected ToolTip ToolTip => BaseToolTip;

        /// <summary>
        /// Gets whether the form is in design mode. Unlike the <see cref="Component.DesignMode"/> property,
        /// this property works even during initialization.
        /// </summary>
        [Browsable(false)]
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Gets whether the form has already been loaded. This property is <see langword="true"/> after the <see cref="Form.Load"/> event is raised for the first time,
        /// and remains <see langword="true"/> even if the form is shown as a dialog multiple times or the handle is recreated (e.g. because <see cref="Control.RightToLeft"/> changes).
        /// Can be useful if we overload the <see cref="Form.OnLoad"/> method and want to avoid executing some initialization more than once.
        /// </summary>
        [Browsable(false)]
        protected bool IsLoaded => isLoaded;

        /// <summary>
        /// Gets the corresponding MDI client of the form, or <see langword="null"/>, if this form is neither an MDI container nor an MDI child.
        /// </summary>
        [Browsable(false)]
        protected MdiClient? MdiClient
        {
            get
            {
                #region Local Methods

                static MdiClient? GetMdiClient(Form mdiParent)
                {
                    foreach (Control? child in mdiParent.Controls)
                    {
                        if (child is MdiClient mdiClient)
                            return mdiClient;
                    }

                    return null;
                }

                #endregion

                return mdiClient?.IsDisposed == false
                    ? mdiClient
                    : mdiClient = IsMdiContainer ? GetMdiClient(this) : MdiParent is Form mdiParent ? GetMdiClient(mdiParent) : null;
            }
        }

        /// <summary>
        /// Gets the MDI child that is suspending this form by a <see cref="ShowMdiChild">ShowMdiChild</see> call,
        /// or <see langword="null"/> if this form is not suspended. When this property is not <see langword="null"/>,
        /// the returned form is among the elements of the <see cref="OwnedMdiChildren"/> property.
        /// </summary>
        [Browsable(false)]
        protected Form? SuspendingMdiChild => suspendingMdiChild;

        /// <summary>
        /// Gets the forms that were shown as MDI children by the <see cref="ShowMdiChild">ShowMdiChild</see> method.
        /// </summary>
        /// <remarks>
        /// <note>Please note that this property is different from <see cref="Form.MdiChildren"/>, which returns all MDI children of an MDI container form.
        /// This property returns the forms that were shown by the <see cref="ShowMdiChild">ShowMdiChild</see> method. If this form is an MDI container,
        /// the result may contain fewer forms than <see cref="Form.MdiChildren"/>. And if this form is an MDI child, the <see cref="Form.MdiChildren"/> property is always empty,
        /// whereas this property still may contain items. If this form is currently suspended, the <see cref="SuspendingMdiChild"/> property
        /// returns the blocker form, which is an element of this property.</note>
        /// </remarks>
        protected Form[] OwnedMdiChildren => ownedMdiChildren?.ToArray() ?? [];

        #endregion

        #region Explicitly Implemented Interface Properties

        bool IObservableParent.IsAddingControl => isAddingControl;
        bool IObservableParent.IsChangingFont => isChangingFont;

        #endregion

        #endregion

        #region Constructors

        #region Static Constructors

#if !NET5_0_OR_GREATER
        static BaseForm()
        {
            if (!OSHelper.IsWindows || OSHelper.IsMono)
                return;

            // Not using Accessors because it's obtained only once.
            formStateRenderSizeGrip = Reflector.TryGetField(typeof(Form), "FormStateRenderSizeGrip", out object? value) && value is BitVector32.Section section ? section : default;
        }
#endif

        #endregion

        #region Instance Constructors

        /// <summary>
        /// Creates a new instance of <see cref="BaseForm"/>
        /// </summary>
        public BaseForm()
        {
            invoker = new InvokeMarshaller(this);
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            SetFont(defaultFont);
            this.RegisterPerMonitorAwarenessNotifications();
            BaseToolTip = new ToolTip
            {
                InitialDelay = 500,
                ReshowDelay = 100
            };

#if !NET35
            if (!OSHelper.IsWindows11OrLater)
#endif
            {
                BaseToolTip.AutoPopDelay = Int16.MaxValue;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Shows the form as an MDI child, owned by of this <see cref="BaseForm"/>.
        /// This <see cref="BaseForm"/> must be either an MDI container or another MDI child.
        /// </summary>
        /// <param name="child">The child to show as an MDI child.</param>
        /// <param name="suspendCaller">When <see langword="true"/>, and the current <see cref="BaseForm"/> is also an MDI child,
        /// the current form will be suspended until the child is closed as if this form was the owner of the new MDI child. This parameter is optional.
        /// Default value: <see langword="false"/>.</param>
        /// <remarks>
        /// <para>Normally MDI children cannot be opened as dialogs. By setting <paramref name="suspendCaller"/> to <see langword="true"/>,
        /// a dialog-like behavior can be achieved, as the caller will be suspended until the child is closed.
        /// The child will not be a modal form, so you still will be able to interact with the parent form and other non-suspended child forms.</para>
        /// <para>As the <paramref name="child"/> form is not opened as a real dialog, this call returns immediately after the child is shown.
        /// You can use the <see cref="OwnedMdiChildClosed"/> event or the <see cref="OnOwnedMdiChildClosed"/> method to get notified when the <paramref name="child"/> form is closed.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="child"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">This form is neither an MDI container nor an MDI child that can own the new child.
        /// <br/>-or-
        /// <br/><paramref name="suspendCaller"/> is <see langword="true"/> when this form is already suspended.</exception>
        public void ShowMdiChild(Form child, bool suspendCaller = false)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child), PublicResources.ArgumentNull);
            Form mdiParent = MdiParent ?? (IsMdiContainer ? this : throw new InvalidOperationException(Res.BaseFormMdiContainerNotFound));
            if (suspendCaller && mdiParent == this)
                throw new InvalidOperationException(Res.BaseFormCannotSuspendMdiParent);
            if (suspendCaller && IsSuspended)
                throw new InvalidOperationException(Res.BaseFormAlreadySuspended);

            child.FormClosed += MdiChild_FormClosed;
            ownedMdiChildren ??= new HashSet<Form>();
            try
            {
                ownedMdiChildren.Add(child);
                if (suspendCaller)
                    Suspend(child);
                {
                    if (mdiParent is BaseForm bf)
                        bf.isAddingControl = true;
                }
                try
                {
                    child.MdiParent = mdiParent;
                }
                finally
                {
                    if (mdiParent is BaseForm bf)
                        bf.isAddingControl = false;
                }
                child.Show();
            }
            catch (Exception)
            {
                child.FormClosed -= MdiChild_FormClosed;
                ownedMdiChildren?.Remove(child);
                if (IsSuspended)
                    Resume();
                throw;
            }
        }

        /// <summary>
        /// Invalidates the MDI client area. Has effect only if the <see cref="Form.IsMdiContainer"/> or <see cref="Form.IsMdiChild"/> is <see langword="true"/> for this form.
        /// </summary>
        public void InvalidateMdiClientArea() => MdiClient?.Invalidate();

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override Control.ControlCollection CreateControlsInstance()
            => OSHelper.IsMono ? new ControlCollectionMono(this) : new ControlCollection(this);

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            previousScale = deviceScale;
            deviceScale = this.GetScale();
            base.OnHandleCreated(e);
            if (!ScaleHelper.IsThreadPerMonitorAware || previousScale == deviceScale)
                return;

            ResetSmallIcon();

            var args = new DeviceScaleChangeEventArgs(default, deviceScale, previousScale);
            OnDeviceScaleChanging(args);
            if (IsMdiChild)
                CheckDpiChangeAsMdiChild();
            else
                CheckDpiChangeAsTopLevelForm();
            OnDeviceScaleChanged(args);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            bool loaded = isLoaded;
            base.OnLoad(e);
            if (loaded)
                return;

            isLoaded = true;
#if NETFRAMEWORK
            // Possible bug in .NET Framework: if the StartPosition is WindowsDefaultBounds or WindowsDefaultLocation,
            // the form may have an unmatching scale from its screen, which fixes itself when moving or resizing the form for the first time.
            if (IsHandleCreated && OSHelper.IsWindows81OrLater && StartPosition is FormStartPosition.WindowsDefaultBounds or FormStartPosition.WindowsDefaultLocation
                && DeviceScale != Screen.FromRectangle(Bounds).GetScale())
            {
                // Bounds = Bounds and SetBoundsCore are "too smart" and recognize that there is no change. SWP_DRAWFRAME is needed to force the change.
                User32.SetWindowPos(Handle, IntPtr.Zero, Left, Top, Width, Height, Constants.SWP_NOZORDER | Constants.SWP_DRAWFRAME);
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            PerformTranslate(this);
#pragma warning restore CS0618 // Type or member is obsolete
            ApplyResources();
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnParentChanged(EventArgs e)
        {
            mdiClient = null;
            base.OnParentChanged(e);
            Control? parent = Parent;
            if (parent == null || !IsMdiChild)
                return;

            // If we have a parent it means this is an MDI child form. Setting default font from new parent font without scaling.
            if (font == null)
            {
                PointF scale = this.GetScaleForParentChanged();
                defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent.Font), scale);
                Debug.Assert(deviceScale == this.GetScale());
                if (deviceScale != scale)
                    lastScaleAsChild = PointF.Empty;
            }

            CheckDpiChangeAsMdiChild();
        }

        /// <inheritdoc />
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // if the parent control is rescaling its font due to DPI change, then ignoring the event (we do our scaling in CheckDpiChange)
            if (dpiChangingAsChildCount > 0 || !AutoScaleFont)
                return;

#if NET47_OR_GREATER || NETCOREAPP
            // The parent is rescaling its font out of a WM_DPICHANGED event (occurs typically in .NET 7+ during form handle creation)
            if (this.IsParentScalingWhileCreated())
                return;
#endif

            // but if the parent font is changing not because of scaling, then we reset our default font as well
            PointF scale = this.GetScaleForParentFontChanged();
            defaultFont!.ResetFrom(ScaleHelper.GetFontOrDefault(Parent?.Font), scale);

            if (font != null)
                return;

            // setting default font from new parent font without scaling
            SetFont(defaultFont);

            // the parent has different scale: invalidating lastScale, so CheckDpiChange will adjust the scale if needed
            Debug.Assert(deviceScale == this.GetScale());
            if (deviceScale != scale)
                lastScaleAsChild = PointF.Empty;
        }

        /// <summary>
        /// Applies the resources of the form. The default implementation just calls the <see cref="ApplyStringResources">ApplyStringResources</see> method.
        /// Called when the form is loaded for the first time. In a derived form, this method can be overridden to apply additional (non-string) resources,
        /// and it can be called whenever the form's resources should be re-applied, e.g. when the display language changes.
        /// </summary>
        protected virtual void ApplyResources()
        {
            if (!IsDesignMode)
                ApplyStringResources();
        }

        /// <summary>
        /// Applies the string resources of the form. If the <see cref="DynamicStringLocalization"/> property is not set to <see cref="DynamicStringLocalization.Disabled"/>,
        /// the default implementation just calls the <see cref="LocalizationHelper.ApplyStringResources">LocalizationHelper.ApplyStringResources</see> method.
        /// In a derived form, this method can be overridden to apply a custom string localization, and it can be called whenever the form's string resources
        /// should be re-applied, e.g. when the display language changes.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DynamicStringLocalization"/> property for more details.
        /// </summary>
        protected virtual void ApplyStringResources()
        {
            if (localizationMode != DynamicStringLocalization.Disabled && !IsDesignMode)
                LocalizationHelper.ApplyStringResources(this);
        }

        /// <summary>
        /// Disposes the form and its resources.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            LanguageSettings.DisplayLanguageChanged -= LanguageSettings_DisplayLanguageChanged;
            if (disposing)
            {
                BaseToolTip.Dispose();
                commandBindings.Dispose();
                Events.Dispose();
                smallIcon?.Dispose();
                font?.Dispose();
                defaultFont?.Dispose();
                font = null;
                defaultFont = null;
                if (ownedMdiChildren != null)
                {
                    foreach (Form mdiChild in ownedMdiChildren)
                        mdiChild.FormClosed -= MdiChild_FormClosed;
                    ownedMdiChildren = null;
                }
            }

            autoScaleFont = false;
            mdiClient = null;
        }

        /// <summary>
        /// Translates controls and tooltips of given control.
        /// </summary>
        /// <param name="control">The control to translate</param>
        /// <remarks>
        /// <note type="warning">This method is obsolete. It does not perform any translation anymore, it just removes the possible postfixes from the control's text properties.
        /// Use the <see cref="DynamicStringLocalization"/> property and the <see cref="ApplyStringResources">ApplyStringResources</see> method instead.</note>
        /// </remarks>
        [Obsolete("Translation does not work anymore, it just removes the possible postfixes.")]
        protected void PerformTranslate(Control control)
        {
            if (translateControls)
            {
                bool finished;
                if (LanguageWinForms.TranslateControl(control, out finished))
                    TranslateToolTip(control);
                if (finished)
                    return;

                if (control.HasChildren)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    foreach (Control c in control.Controls)
                        PerformTranslate(c!);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
        }

        /// <summary>
        /// Raises the <see cref="CalledMdiChildClosed"/> event.
        /// <br/>This method is obsolete along with the <see cref="CalledMdiChildClosed"/> event.
        /// Use the <see cref="OwnedMdiChildClosed"/> event and the <see cref="OnOwnedMdiChildClosed"/> method instead.
        /// </summary>
        /// <param name="sender">The closed form, which is the sender of the provided arguments.</param>
        /// <param name="e">Arguments of the closed form.</param>
        [Obsolete("The CalledMdiChildClosed event is now obsolete. Use the OwnedMdiChildClosed event and the OnCalledMdiChildClosed method instead.")]
        protected virtual void OnCalledMdiChildClosed(object sender, FormClosedEventArgs e)
            => Events.GetHandler<FormClosedEventHandler>(nameof(CalledMdiChildClosed))?.Invoke(sender, e);

        /// <summary>
        /// Raises the <see cref="OwnedMdiChildClosed"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnOwnedMdiChildClosed(OwnedMdiChildClosedEventArgs e)
            => Events.GetHandler<EventHandler<OwnedMdiChildClosedEventArgs>>(nameof(OwnedMdiChildClosed))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="Suspended"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnSuspended(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Suspended))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="Resumed"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnResumed(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(Resumed))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceScaleChanging"/> event.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeviceScaleChanging"/> event for more details.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDeviceScaleChanging(DeviceScaleChangeEventArgs e)
            => Events.GetHandler<EventHandler<DeviceScaleChangeEventArgs>>(nameof(DeviceScaleChanging))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceScaleChanged"/> event.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeviceScaleChanged"/> event for more details.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDeviceScaleChanged(DeviceScaleChangeEventArgs e)
            => Events.GetHandler<EventHandler<DeviceScaleChangeEventArgs>>(nameof(DeviceScaleChanged))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceScaleGetNewSize"/> event.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeviceScaleGetNewSize"/> event for more details.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDeviceScaleGetNewSize(DeviceScaleGetNewSizeEventArgs e)
            => Events.GetHandler<EventHandler<DeviceScaleGetNewSizeEventArgs>>(nameof(DeviceScaleGetNewSize))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceScaleAutoResized"/> event.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeviceScaleAutoResized"/> event for more details.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDeviceScaleAutoResized(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(DeviceScaleAutoResized))?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DynamicStringLocalizationChanged"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDynamicStringLocalizationChanged(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(DynamicStringLocalization))?.Invoke(this, e);

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
#if !NET5_0_OR_GREATER
                case Constants.WM_NCHITTEST when OSHelper.IsWindows && !OSHelper.IsMono:
                    WmNCHitTest(ref m);
                    return;
#endif

                case Constants.WM_NCCREATE:
                    if (isPerMonitorDpiAwarenessV1 && OSHelper.IsWindows10Build1607OrLater)
                    {
                        Debug.Assert(IsHandleCreated);
                        User32.EnableNonClientDpiScaling(Handle);
                    }

                    base.WndProc(ref m);
                    break;

                case Constants.WM_PAINT:
                    if (IsMdiChild)
                        CheckDpiChangeAsMdiChild();
                    else
                        CheckDpiChangeAsTopLevelForm();
                    base.WndProc(ref m);
                    return;

                case Constants.WM_GETDPISCALEDSIZE:
                    base.WndProc(ref m);
                    unsafe
                    {
                        var scale = new PointF(m.WParam.LOWORD() / ScaleHelper.DefaultDpi, m.WParam.HIWORD() / ScaleHelper.DefaultDpi);
                        SIZE* suggestedSize = (SIZE*)m.LParam;
                        var args = new DeviceScaleGetNewSizeEventArgs(suggestedSize->ToSize(), scale, deviceScale, m.Result != IntPtr.Zero);
                        OnDeviceScaleGetNewSize(args);
                        m.Result = new IntPtr(args.Handled ? 1 : 0);
                        if (args.Handled)
                            *suggestedSize = new SIZE(args.DesiredSize);
                    }

                    return;

                case Constants.WM_DPICHANGED:
                    // Starting to process every recursive call immediately, so the pointers of m are valid, and base.WndProc is safe to call.
                    dpiChangeRecursionCount += 1;
                    try
                    {
                        previousScale = deviceScale;
                        deviceScale = new PointF(m.WParam.LOWORD() / ScaleHelper.DefaultDpi, m.WParam.HIWORD() / ScaleHelper.DefaultDpi);
                        unsafe { dpiChangedSuggestedBounds = ((RECT*)m.LParam)->ToRectangle(); }
                        var args = new DeviceScaleChangeEventArgs(dpiChangedSuggestedBounds, deviceScale, previousScale);
                        OnDeviceScaleChanging(args);
                        base.WndProc(ref m);
                        if (m.Result != IntPtr.Zero)
                        {
                            m.Result = IntPtr.Zero; // Framework 4.7+ sets it to 1 when targeting lower version than 4.7
                            DefWndProc(ref m);
                        }

                        // Detecting if base.WndProc caused reentrancy (or OnDeviceScaleChanging, but that would be a very unclean usage), triggering another DPI change.
                        // If so, we return from here, and going on from the outermost call, because possible scaling by Font does not work from recursion.
                        if (dpiChangeRecursionCount > 1)
                        {
                            OnDeviceScaleChanged(args); // to ensure the same amount of Changed and Changing calls
                            return;
                        }

                        // From this point m and the local variables above may be outdated, referring to a previous DPI change.
                        // We must use fields or update the local variables.
                        ResetSmallIcon();
                        Rectangle before;
                        do
                        {
                            // This can also cause reentrancy if AutoScaleFont is true, AutoScaleMode is Font, and the targeted platform did not scale
                            // in base.WndProc on older platforms. The reentrancy is handled above, but each time we exit from an inner call,
                            // we must check if the current font scaling is still valid.
                            before = Bounds;
                            CheckDpiChangeAsTopLevelForm();
                        } while (autoScaleFont && (font ?? defaultFont)?.CurrentScale != deviceScale);

                        OnDeviceScaleChanged(args.Reset(dpiChangedSuggestedBounds, deviceScale, previousScale));
                        Rectangle after = Bounds;

                        // If neither us, nor the DeviceScaleChanged event handlers changed the size, we can expect Windows to apply the suggested bounds
                        // in a later WM_WINDOWPOSCHANGED message (V1 awareness: it always happens).
                        if (isPerMonitorDpiAwarenessV1 || before == after)
                            isOnAutoResizedPending = true;
                    }
                    finally
                    {
                        dpiChangeRecursionCount -= 1;
                    }

                    return;

                case Constants.WM_WINDOWPOSCHANGED when isOnAutoResizedPending && dpiChangedSuggestedBounds.Size == Size:
                    base.WndProc(ref m);
                    dpiChangedSuggestedBounds = Rectangle.Empty;
                    if (!isPerMonitorDpiAwarenessV1 || ActiveForm != this)
                    {
                        isOnAutoResizedPending = false;
                        OnDeviceScaleAutoResized(EventArgs.Empty);
                    }

                    return;

                case Constants.WM_EXITSIZEMOVE:
                    base.WndProc(ref m);
                    bool raiseAutoResized = isPerMonitorDpiAwarenessV1 && isOnAutoResizedPending;
                    isOnAutoResizedPending = false;
                    if (raiseAutoResized)
                        OnDeviceScaleAutoResized(EventArgs.Empty);
                    return;

                case Constants.WM_WINDOWPOSCHANGING when IsSuspended:
                    // preventing bringing the disabled MDI child form to the front
                    unsafe { ((WINDOWPOS*)m.LParam)->flags |= Constants.SWP_NOZORDER; }
                    base.WndProc(ref m);
                    return;

                case Constants.WM_DPICHANGED_BEFOREPARENT:
                    Debug.Assert(IsMdiChild);
                    dpiChangeRecursionCount += 1;
                    dpiChangingAsChildCount += 1;
                    try
                    {
                        // Though CheckDpiChangeAsMdiChild also has OnDeviceScaleChanging and OnDeviceScaleChanged calls, we must call them from here
                        // to wrap the base.WndProc call. The inner Changing/Changed events will not be called, because we reset deviceScale here.
                        previousScale = deviceScale;
                        deviceScale = this.GetScale();
                        var args = new DeviceScaleChangeEventArgs(default, deviceScale, previousScale);
                        OnDeviceScaleChanging(args);
                        base.WndProc(ref m);

                        // Detecting if base.WndProc caused reentrancy (though it requires something like changing the top-level form's font), triggering another DPI change.
                        // If so, we return from here, and going on from the outermost call, because possible scaling by Font does not work from recursion.
                        if (dpiChangeRecursionCount > 1)
                        {
                            OnDeviceScaleChanged(args); // to ensure the same amount of Changed and Changing calls
                            return;
                        }

                        ResetSmallIcon();
                        do
                        {
                            // This can also cause reentrancy (though quite unlikely) if something nasty happens when the font changes (e.g. blowing up the container form).
                            // The reentrancy is handled above, but each time we exit from an inner call, we must check if the current font scaling is still valid.
                            CheckDpiChangeAsMdiChild();
                        } while (autoScaleFont && (font ?? defaultFont)?.CurrentScale != deviceScale);
                        
                        OnDeviceScaleChanged(args.Reset(default, deviceScale, previousScale));
                    }
                    finally
                    {
                        dpiChangeRecursionCount -= 1;
                        dpiChangingAsChildCount -= 1;
                    }
                    return;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    Debug.Assert(IsMdiChild);
                    dpiChangingAsChildCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingAsChildCount -= 1;
                    }
                    return;

                case Constants.WM_SETICON when m.WParam is Constants.ICON_SMALL && smallIcon != null && m.LParam != smallIcon.Handle:
                    m.LParam = smallIcon.Handle;
                    base.WndProc(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
            => dpiChangeRecursionCount > 0 && !dpiChangedSuggestedBounds.IsEmpty()
                ? dpiChangedSuggestedBounds
                : base.GetScaledBounds(bounds, factor, specified);

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> on the thread that the control was created on.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// <para>This method is similar as using <see cref="Control.InvokeRequired"/> and <see cref="Control.Invoke(Delegate)"/> together,
        /// but it works even when the handle is not created yet, in which case <see cref="Control.InvokeRequired"/> returns <see langword="false"/>.</para>
        /// <para>The callback is invoked only if <see cref="Control.Disposing"/> and <see cref="Control.IsDisposed"/> properties return <see langword="false"/>.</para>
        /// </remarks>
        protected void InvokeOnUIThread(Action callback) => invoker.Invoke(callback);

        #endregion

        #region Private Methods

        private void Suspend(Form suspendingChild)
        {
            Debug.Assert(!IsSuspended && !IsMdiContainer && IsMdiChild);

            // Sending this form back to be just above the topmost disabled MDI child (if any), preventing "click-through" issues with the disabled form.
            // Using MdiClient.Controls instead of MdiParent.MdiChildren because the latter does not reflect the Z-order of the MDI children.
            Control.ControlCollection children = MdiClient!.Controls;
            int? firstDisabledIndex = null;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Enabled)
                    continue;
                firstDisabledIndex = i;
                break;
            }

            if (firstDisabledIndex > 0)
                children.SetChildIndex(this, firstDisabledIndex.Value - 1);
            else
                SendToBack();
            suspendingMdiChild = suspendingChild;
            Enabled = false;
            OnSuspended(EventArgs.Empty);
        }

        private void Resume()
        {
            Debug.Assert(IsSuspended);
            suspendingMdiChild = null;
            Enabled = true;
            Activate();
            OnResumed(EventArgs.Empty);
        }

        [Obsolete]
        private void TranslateToolTip(Control control)
        {
            if (BaseToolTip.GetToolTip(control)?.Length > 0)
                BaseToolTip.SetToolTip(control, Language.Translate(BaseToolTip.GetToolTip(control)));
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Bugfix: When size grip is visible, and form is above and left of the primary monitor, form cannot be dragged anymore due to forced diagonal resizing.
        /// In .NET 5 I already fixed this in WinForms: https://github.com/dotnet/winforms/pull/2032
        /// </summary>
        private void WmNCHitTest(ref Message m)
        {
            if (this.FormState()[formStateRenderSizeGrip] != 0)
            {
                // Here is the bug in original code: LParam contains two shorts. Without the cast negative values are positive ints
                int x = m.LParam.SignedLOWORD();
                int y = m.LParam.SignedHIWORD();
                POINT pt = new POINT(x, y);
                User32.ScreenToClient(Handle, ref pt);
                Size clientSize = ClientSize;
                if (pt.x >= clientSize.Width - 16 && pt.y >= clientSize.Height - 16 && clientSize.Height >= 16)
                {
                    m.Result = IsMirrored ? (IntPtr)16 : (IntPtr)17;
                    return;
                }
            }

            DefWndProc(ref m);
            if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
            {
                nint result = m.Result;
                if (result >= 10 && result <= 17)
                    m.Result = (IntPtr)18;
            }
        }
#endif

        private void ResetSmallIcon()
        {
            if (smallIcon == null || !OSHelper.IsWindows || !ScaleHelper.IsThreadPerMonitorAware)
                return;

            smallIcon.Dispose();
            Debug.Assert(deviceScale == this.GetScale());
            smallIcon = base.Icon?.Resize(IconsHelper.SmallIconReferenceSize.Scale(deviceScale));
            if (smallIcon != null && IsHandleCreated)
                User32.SendMessage(Handle, Constants.WM_SETICON, Constants.ICON_SMALL, smallIcon.Handle);
        }

        private bool ShouldSerializeFont() => font != null;

        private void CheckDpiChangeAsMdiChild()
        {
            Debug.Assert(IsMdiChild);
            PointF scale = this.GetScale();

            var oldScale = deviceScale;
            deviceScale = scale;
            var args = oldScale == deviceScale ? null : new DeviceScaleChangeEventArgs(default, deviceScale, oldScale);
            if (args != null)
                OnDeviceScaleChanging(args);

            if ((scale == lastScaleAsChild && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            lastScaleAsChild = scale;
            if (AutoScaleFont)
            {
                if (font is ScalingFont explicitFont)
                    explicitFont.Scale(scale);
                else
                    defaultFont!.Scale(scale);
                SetFont(font ?? defaultFont);
            }

            if (args != null)
                OnDeviceScaleChanged(args);
        }

        private void CheckDpiChangeAsTopLevelForm()
        {
            // The Font check is needed because if reentrancy occurs in the WM_DPICHANGED message, the currently set Font may be wrong.
            // May occur before the form is loaded when initialization jumps between displays, depending on the StartPosition value,
            // or when user changes the bounds manually in the DeviceScaleChanged event.
            if (!AutoScaleFont || ((font ?? defaultFont) is ScalingFont f && f.CurrentScale == deviceScale && f.Font.Equals(Font)) || Disposing || IsDisposed)
                return;

            if (font is ScalingFont explicitFont)
                explicitFont.Scale(deviceScale);
            else
                defaultFont!.Scale(deviceScale);

            SetFont(font ?? defaultFont);
        }

        private void SetFont(ScalingFont? value)
        {
            isChangingFont = true;
            try
            {
                if (value == null)
                {
                    base.Font = null!;
                    return;
                }

                // explicitly set fonts must be forcibly set in base.Font for non-top-level forms
                bool force = IsMdiChild && ReferenceEquals(font, value);
                Font oldFont = base.Font;
                Font newFont = value.Font;

                // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
                // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
                if (Equals(oldFont, newFont))
                {
                    if (!force && (ReferenceEquals(oldFont, newFont) || !oldFont.IsDisposed()))
                        return;

                    suppressFontChanged = true;
                    try
                    {
                        base.Font = null!;

                        // setting base.Font caused reentrancy: not letting the outer call to set the font again
                        if (!suppressFontChanged)
                            return;
                    }
                    finally
                    {
                        suppressFontChanged = false;
                    }
                }

                base.Font = newFont;
            }
            finally
            {
                isChangingFont = false;
            }
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanging()
        {
            Debug.Assert(IsMdiChild);
            dpiChangingAsChildCount += 1;
            if (isPerMonitorDpiAwarenessV1 && IsMdiChild)
                CheckDpiChangeAsMdiChild();
        }

        void IPerMonitorDpiAware.ParentFormDpiChanged()
        {
            Debug.Assert(IsMdiChild);
            Debug.Assert(dpiChangingAsChildCount > 0);
            dpiChangingAsChildCount -= 1;
#if !NET6_0_OR_GREATER
            // On .NET 6- the layout may not be updated automatically after a DPI change if both the MDI parent and MDI child scales the font automatically.
            PerformLayout();
#endif
        }

        #endregion

        #region Event Handlers

        private void LanguageSettings_DisplayLanguageChanged(object? sender, EventArgs e) => ApplyStringResources();

        private void MdiChild_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not Form mdiChild)
                return;

            mdiChild.FormClosed -= MdiChild_FormClosed;
            try
            {
                ownedMdiChildren?.Remove(mdiChild);
#pragma warning disable CS0618 // Type or member is obsolete
                OnCalledMdiChildClosed(sender, e);
#pragma warning restore CS0618 // Type or member is obsolete
                OnOwnedMdiChildClosed(new OwnedMdiChildClosedEventArgs(mdiChild, e.CloseReason));
            }
            finally
            {
                if (mdiChild == suspendingMdiChild)
                    Resume();
            }
        }

        #endregion

        #endregion
    }
}