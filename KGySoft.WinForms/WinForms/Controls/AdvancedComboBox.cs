#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedComboBox.cs
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Advanced version of the <see cref="ComboBox"/> control that provides some advanced features and fixes for the original <see cref="ComboBox"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedTextBox"/> control offers the following features in addition to <see cref="TextBox"/>:
    /// <list type="bullet">
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item><see cref="TextChangedOnLeave"/> event: occurs when leaving the control and <see cref="ComboBox.Text"/> is different from the value when the control received focus.</item>
    /// <item>Auto complete works even in <see cref="ComboBoxStyle.Simple"/> mode.</item>
    /// <item>Consistent font scaling on all platforms when per-monitor DPI awareness is enabled (see <see cref="AutoScaleFont"/> property).
    /// Note that it affects font scaling only, so auto-sizing behavior still depends on the current platform.</item>
    /// </list>
    /// </remarks>
    [ToolboxBitmap(typeof(ComboBox))]
    [Description(@"A combo box with the following additional features:
- Disabled colors
- ReadOnly property and ReadOnlyChanged event
- TextChangedOnLeave event
- Auto complete works in Simple mode
- Auto scaling Font on all platform targets")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ShouldSerialize... methods must be instance methods for designer serialization.")]
    public class AdvancedComboBox : ComboBox, ISupportsDisabledColor, IReadOnlyCapable, IPerMonitorDpiAware
    {
        #region Nested classes

        #region InnerEditWindow class

        /// <summary>
        /// Hooks WndProc of the inner editor window to deny WM_PASTE in ReadOnly mode and implement custom WM_PAINT.
        /// </summary>
        private sealed class InnerEditWindow : NativeWindow
        {
            #region Fields

            private readonly AdvancedComboBox parent;

            #endregion

            #region Constructors

            internal InnerEditWindow(AdvancedComboBox parent) => this.parent = parent;

            #endregion

            #region Methods

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    // workaround: AutoComplete clears text in Simple mode
                    // Note: WM_SETTEXT is visible also in ComboBox.WndProc but solves only Append/SuggestAppend mode. Here Suggest mode is solved, too
                    case Constants.WM_SETTEXT when parent is { readOnly: false, clearingText: false, DropDownStyle: ComboBoxStyle.Simple, AutoCompleteMode: not AutoCompleteMode.None }:
                        string origText = parent.Text;
                        int selectionStart = parent.SelectionStart;
                        int selectionLength = parent.SelectionLength;
                        base.WndProc(ref m);
                        if (origText.Length > 0 && parent.Text.Length == 0)
                        {
                            parent.Text = origText;
                            parent.SelectionStart = selectionStart;
                            parent.SelectionLength = selectionLength;
                        }
                        return;

                    // Suppressing cut, paste, clear and undo in ReadOnly mode
                    case Constants.WM_CUT or Constants.WM_CLEAR or Constants.WM_PASTE or Constants.WM_UNDO when parent.readOnly:
                        return;

                    // Special handling for disabled painting
                    case Constants.WM_PAINT when !parent.Enabled:
                        User32.ValidateRect(m.HWnd, IntPtr.Zero);
                        Rectangle bounds = User32.GetClientRect(m.HWnd, out var rect) ? rect.ToRectangle() : Rectangle.Empty;
                        if (!bounds.IsEmpty())
                        {
                            using var g = Graphics.FromHwnd(m.HWnd);
                            parent.DrawDisabledTextBox(g, bounds);
                        }

                        return;

                    default:
                        base.WndProc(ref m);
                        return;
                }
            }

            #endregion
        }

        #endregion

        #region InnerListBoxWindow class

        /// <summary>
        /// Hooks WndProc of the inner editor window to prevent selection change for mouse clicks.
        /// </summary>
        private sealed class InnerListBoxWindow : NativeWindow
        {
            #region Fields

            private readonly AdvancedComboBox parent;

            #endregion

            #region Constructors

            internal InnerListBoxWindow(AdvancedComboBox parent) => this.parent = parent;

            #endregion

            #region Methods

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case Constants.WM_LBUTTONDOWN or Constants.WM_LBUTTONDBLCLK when parent.readOnly:
                        parent.ProcessReadOnlyMouseDown(ref m);
                        return;

                    default:
                        base.WndProc(ref m);
                        return;
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const int referenceDropDownWidth = 17;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledOrReadOnlyBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color defaultReadOnlyForeColor = SystemColors.ControlText;

        #endregion

        #region Instance Fields

        private readonly bool isPerMonitorDpiAwarenessV1 = ScaleHelper.PerMonitorDpiAwarenessVersion == 1; // it's alright to cache it for the control because an instance is tied to the same thread

        // NOTE: Unlike in ButtonBase descendants, we always set the base back and fore colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        private FlatStyle lastFlatStyle = FlatStyle.Standard; // would not be needed if there was an overridable OnFlatStyleChanged method
        private bool systemDrawDropDownListMode = true;
        private bool readOnly;
        private string? textOnFocus;
        private InnerEditWindow? nativeEditorChild;
        private InnerListBoxWindow? nativeListBoxChild;
        private AutoCompleteSource origCompleteSource = AutoCompleteSource.None;
        private AutoCompleteMode origCompleteMode = AutoCompleteMode.None;
        private bool clearingText;

        private bool suppressFontChanged;
        private bool autoScaleFont = true;
        private ScalingFont? font; // The explicitly set font.
        private ScalingFont? defaultFont; // The font when Font is not set. Used only when AutoScaleFont is set; otherwise, actual Parent.Font is used.
        private PointF lastScale;
        private int dpiChangingCount;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when <see cref="ReadOnly"/> property has been changed.
        /// </summary>
        [Description("Occurs when ReadOnly property has been changed.")]
        [Category("AdvancedComboBox")]
        public event EventHandler? ReadOnlyChanged
        {
            add => Events.AddHandler(nameof(ReadOnlyChanged), value);
            remove => Events.RemoveHandler(nameof(ReadOnlyChanged), value);
        }

        /// <summary>
        /// Occurs on leaving the control when content is different from the original one when the control was focused.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused.")]
        public event EventHandler? TextChangedOnLeave
        {
            add => Events.AddHandler(nameof(TextChangedOnLeave), value);
            remove => Events.RemoveHandler(nameof(TextChangedOnLeave), value);
        }

        #endregion

        #region Properties

        #region Static Properties

        private static Color ThemedDisabledDropDownListColor => VisualStyleHelper.GetTextColor(VisualStyleHelper.ComboBoxTheme, (int)COMBOBOXPARTS.CP_READONLY, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, defaultDisabledForeColor);

        #endregion

        #region Instance Properties
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> and <see cref="ReadOnly"/> state.
        /// </summary>
        [Description("The background color in the current Enabled/ReadOnly state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (!ReadOnly && Enabled)
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current state.
        /// </summary>
        [Description("The text color in the current Enabled state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (Enabled)
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/> and not <see cref="ReadOnly"/>.
        /// </summary>
        /// <remarks>
        /// <para>If visual styles are enabled, <see cref="ComboBox.DropDownStyle"/> is <see cref="ComboBoxStyle.DropDownList"/>,
        /// <see cref="ComboBox.FlatStyle"/> is <see cref="FlatStyle.System"/> or <see cref="FlatStyle.Standard"/>,
        /// and <see cref="SystemDrawDropDownListMode"/> is <see langword="true"/>, then this property is ignored.</para>
        /// </remarks>
        [Category("AdvancedComboBox")]
        [Description("Determines the background color when the control is Enabled and not ReadOnly. "
            + "Has no effect with visual styles enabled when DropDownStyle is DropDownList, FlatStyle is System or Standard, and SystemDrawDropDownListMode is true.")]
        public Color EnabledBackColor
        {
            get => !enabledBackColor.IsEmpty ? enabledBackColor : defaultEnabledBackColor;
            set
            {
                if (enabledBackColor == value)
                    return;
                enabledBackColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is <see cref="Control.Enabled"/>.
        /// </summary>
        /// <remarks>
        /// <para>If visual styles are enabled, <see cref="ComboBox.DropDownStyle"/> is <see cref="ComboBoxStyle.DropDownList"/>,
        /// <see cref="ComboBox.FlatStyle"/> is <see cref="FlatStyle.System"/> or <see cref="FlatStyle.Standard"/>,
        /// and <see cref="SystemDrawDropDownListMode"/> is <see langword="true"/>, then this property is ignored.</para>
        /// </remarks>
        [Category("AdvancedComboBox")]
        [Description("Determines the text color when the control is Enabled. "
            + "Has no effect with visual styles enabled when DropDownStyle is DropDownList, FlatStyle is System or Standard, and SystemDrawDropDownListMode is true.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor
                : ReadOnly ? defaultReadOnlyForeColor
                : defaultEnabledForeColor;
            set
            {
                if (enabledForeColor == value)
                    return;
                enabledForeColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/> or is <see cref="ReadOnly"/>.
        /// </summary>
        /// <remarks>
        /// <para>If visual styles are enabled, <see cref="ComboBox.DropDownStyle"/> is <see cref="ComboBoxStyle.DropDownList"/>,
        /// <see cref="ComboBox.FlatStyle"/> is <see cref="FlatStyle.System"/> or <see cref="FlatStyle.Standard"/>,
        /// and <see cref="SystemDrawDropDownListMode"/> is <see langword="true"/>, then this property is ignored.</para>
        /// </remarks>
        [Category("AdvancedComboBox")]
        [Description("Determines the background when the control is not Enabled or is ReadOnly. "
            + "Has no effect with visual styles enabled when DropDownStyle is DropDownList, FlatStyle is System or Standard, and SystemDrawDropDownListMode is true.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : defaultDisabledOrReadOnlyBackColor;
            set
            {
                if (disabledBackColor == value)
                    return;
                disabledBackColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets the text color when the control is not <see cref="Control.Enabled"/>.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Determines the text color when the control is not Enabled.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor
                : VisualStyleHelper.RenderWithVisualStyles && SystemDrawDropDownListMode && !OSHelper.IsFrameworkMono
                    && DropDownStyle is ComboBoxStyle.DropDownList && FlatStyle is FlatStyle.Standard or FlatStyle.System ? ThemedDisabledDropDownListColor
                : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
                ResetColors();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Font"/> should be automatically scaled when DPI changes and the current thread has per-monitor DPI awareness.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>When <see langword="true"/>, the <see cref="Font"/> is automatically scaled to the current DPI of the corresponding display on every executing platform.
        /// It also ensures that without an explicitly set font it is inherited from <see cref="Control.Parent"/>, which would be the normal behavior, but is broken in .NET 6+ and above.</para>
        /// <para>When <see langword="false"/>, the <see cref="Font"/> may or may not be scaled, and the font of the parent control may or may not be applied correctly, depending on the default behavior of the executing platform.</para>
        /// <note>Please note that this property affects the font only. Scaling the size and location always depends on the executing platform behavior.</note>
        /// </remarks>
        [Category("AdvancedComboBox")]
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
                font?.ResetFrom(font.Font, value ? this.GetScale() : ScaleHelper.SystemScale);
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
                if (dpiChangingCount > 0 && AutoScaleFont)
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
                PointF scale = AutoScaleFont ? this.GetScale() : ScaleHelper.SystemScale;
                if (font == null)
                    font = new ScalingFont(ScaleHelper.GetFontOrDefault(value), scale);
                else
                    font.ResetFrom(ScaleHelper.GetFontOrDefault(value), scale);
                SetFont(font);
            }
        }

        /// <summary>
        /// Do not set this property. DrawMode is automatically managed in <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("ReSharper", "ValueParameterNotUsed", Justification = "Intended")]
        public new DrawMode DrawMode
        {
            get => base.DrawMode;
            set { }
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner combo box.
        /// </summary>
        [DefaultValue(AutoCompleteMode.None)]
        public new AutoCompleteMode AutoCompleteMode
        {
            get => readOnly ? origCompleteMode : base.AutoCompleteMode;
            set
            {
                // When handle is created, we hook the inner text box, which accidentally stops auto complete from working in Simple mode.
                // Re-setting auto complete mode after handle creation does not work from code: it throws a NullReferenceException from the ComboBox.SetAutoComplete method.
                // So another workaround if we make sure that the handle is created (and the hook is already set) before setting the AutoCompleteMode property.
                if (!DesignMode && !IsHandleCreated && DropDownStyle == ComboBoxStyle.Simple)
                    CreateHandle();

                if (readOnly)
                    origCompleteMode = value;
                else
                    base.AutoCompleteMode = value;
            }
        }

        ///<summary>
        /// Gets or sets a value specifying the source of complete strings used for automatic completion.
        ///</summary>
        [DefaultValue(AutoCompleteSource.None)]
        public new AutoCompleteSource AutoCompleteSource
        {
            get => readOnly ? origCompleteSource : base.AutoCompleteSource;
            set
            {
                if (readOnly)
                    origCompleteSource = value;
                else
                    base.AutoCompleteSource = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the combo box should have the default system appearance in <see cref="ComboBoxStyle.DropDownList"/> mode.
        /// If this property is <see langword="false"/>, then drop-down list appearance will look similar to the <see cref="ComboBoxStyle.DropDown"/> mode
        /// even on Windows Vista and newer platforms.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Determines whether the combo box should have the default system appearance in DropDownList mode. " +
                    "If this property is false, then drop-down list appearance will look similar to the DropDown mode " +
                    "even on Windows Vista and newer platforms.")]
        [DefaultValue(true)]
        public bool SystemDrawDropDownListMode
        {
            get => systemDrawDropDownListMode;
            set
            {
                systemDrawDropDownListMode = value;
                AdjustDrawMode();
                ResetColors(); // because DisabledForeColor depends on this property
            }
        }

        /// <summary>
        /// Gets or sets read-only state of the control.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Gets or sets read-only state of the control.")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                if (readOnly == value)
                    return;

                var style = DropDownStyle;
                if (value)
                {
                    origCompleteSource = base.AutoCompleteSource;
                    origCompleteMode = base.AutoCompleteMode;
                    if (style != ComboBoxStyle.DropDownList)
                    {
                        base.AutoCompleteMode = AutoCompleteMode.None;
                        base.AutoCompleteSource = AutoCompleteSource.None;
                    }
                }
                else if (style != ComboBoxStyle.DropDownList)
                {
                    if (DropDownStyle != ComboBoxStyle.DropDownList)
                    {
                        base.AutoCompleteMode = origCompleteMode;
                        base.AutoCompleteSource = origCompleteSource;
                    }
                }

                readOnly = value;
                if (Enabled)
                    ResetColors();

                // Handling read-only changes on Mono. Otherwise, it's handled in the native controls directly.
                if (OSHelper.IsFrameworkMono)
                    AdjustReadOnlyOnFrameworkMono();
                OnReadOnlyChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region Private Properties

        private bool DrawByVisualStylesWhenDisabled => systemDrawDropDownListMode && VisualStyleHelper.RenderWithVisualStyles && !OSHelper.IsFrameworkMono
            && OSHelper.IsWindowsVistaOrLater && DropDownStyle == ComboBoxStyle.DropDownList && FlatStyle is FlatStyle.System or FlatStyle.Standard;

        #endregion

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedComboBox"/>
        /// </summary>
        public AdvancedComboBox()
        {
            defaultFont = new ScalingFont(ScaleHelper.DefaultFont, ScaleHelper.SystemScale);
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
            this.RegisterPerMonitorAwarenessNotifications();
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Clears <see cref="ComboBox.Text"/> property. If <see cref="AutoCompleteMode"/> property is set on a simple mode combo box, then
        /// use this method to clear text instead of setting empty string to Text property.
        /// </summary>
        public void Clear()
        {
            clearingText = true;
            try
            {
                Text = String.Empty;
            }
            finally
            {
                clearingText = false;
            }
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            var style = DropDownStyle;

            // enabling
            if (Enabled)
            {
                // without this Text may remain selected even if not focused
                if (!Focused && style != ComboBoxStyle.DropDownList)
                    SelectionLength = 0;

                // if readonly was changed in disabled style original auto complete should be restored here
                if (!readOnly && origCompleteSource != AutoCompleteSource.None && DropDownStyle != ComboBoxStyle.DropDownList
                    && (base.AutoCompleteSource != origCompleteSource || base.AutoCompleteMode != origCompleteMode))
                {
                    base.AutoCompleteMode = origCompleteMode;
                    base.AutoCompleteSource = origCompleteSource;
                }
            }
            // disabling
            else
            {
                // saving current auto complete
                if (!readOnly)
                {
                    origCompleteMode = base.AutoCompleteMode;
                    origCompleteSource = base.AutoCompleteSource;
                }
            }

            ResetColors();
        }

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            // Hooking inner text box to capture WM_PASTE and others.
            // The base.OnHandleCreated creates the inner native window for Simple and DropDown modes only.
            base.OnHandleCreated(e);

            // Hooking the native inner controls on .NET [Framework] only. On Framework Mono, it's in OnDropDownStyleChanged.
            if (!OSHelper.IsFrameworkMono)
                InitHooks();

            // BUG workaround: If DropDownStyle is Simple or DropDown, setting the font recreates the handle again, which will end up in a Win32Exception.
            // In this case waiting with the DPI resizing. In worst case we can still detect the DPI change in WM_PAINT.
            if (DropDownStyle == ComboBoxStyle.DropDownList)
                CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseHooks();
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            if (suppressFontChanged)
                return;
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            textOnFocus = Text;
        }

        /// <inheritdoc />
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (textOnFocus != Text)
                OnTextChangedOnLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="ReadOnlyChanged"/> event.
        /// </summary>
        protected virtual void OnReadOnlyChanged(EventArgs e) => Events.GetHandler<EventHandler>(nameof(ReadOnlyChanged))?.Invoke(this, e);


        /// <summary>
        /// Raises the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        protected virtual void OnTextChangedOnLeave(EventArgs e) => Events.GetHandler<EventHandler>(nameof(TextChangedOnLeave))?.Invoke(this, e);

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // suppressing deleting and navigation (selecting item from list) because these cannot be suppressed in KeyPress
            if (readOnly && (e.KeyCode is Keys.Delete or Keys.Back or Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown
                || DropDownStyle == ComboBoxStyle.DropDownList && e.KeyCode is Keys.Space or Keys.Right or Keys.Left or Keys.Home or Keys.End))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (!OSHelper.IsFrameworkMono || !e.SuppressKeyPress)
                base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (readOnly)
            {
                // allowing only Ctrl+C (Copy) - Ctrl+Insert is not captured here
                e.Handled = e.KeyChar != (char)3; //!e.KeyChar.In((char)3, (char)13, (char)27);
            }

            if (!OSHelper.IsFrameworkMono || !e.Handled)
                base.OnKeyPress(e);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!OSHelper.IsFrameworkMono || !readOnly || DropDownStyle == ComboBoxStyle.Simple)
            {
                base.OnMouseDown(e);
                return;
            }

            // The Mono implementation subscribes the MouseDown event to handle the drop-down button, 
            // so we must not call the base.OnMouseDown in read-only mode on Mono.
            Rectangle buttonArea = DropDownStyle == ComboBoxStyle.DropDownList ? ClientRectangle : this.GetButtonArea() ?? ClientRectangle;
            if (!buttonArea.Contains(e.Location))
                base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;
            ReleaseHooks();
            if (disposing)
            {
                font?.Dispose();
                defaultFont?.Dispose();
                font = null;
                defaultFont = null;
            }

            base.Dispose(disposing);
            if (disposing)
                Events.Dispose();
        }

        /// <summary>
        /// Draws an item in the dropdown area and also in the control area in DropDownList mode.
        /// Works only if DrawMode is OwnerDrawFixed.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // drawing an item in the dropdown area
            if (e.Index >= 0)
            {
                string? text = GetItemText(Items[e.Index]);

                Color foreColor;
                Color backColor;

                // Selected list item
                if ((int)(e.State & DrawItemState.Selected) != 0)
                {
                    foreColor = SystemColors.HighlightText;
                    backColor = SystemColors.Highlight;
                }
                // Non-selected list item (with correct Enabled/Disabled/ReadOnly colors)
                else
                {
                    foreColor = ForeColor;
                    backColor = BackColor;
                }

                Rectangle bounds = e.Bounds;
                e.Graphics.FillRectangle(backColor.GetBrush(), bounds);
                bounds.Inflate(-1, -1);
                TextRenderer.DrawText(e.Graphics, text, e.Font, bounds, foreColor, backColor, this.GetFormatFlags());
                e.DrawFocusRectangle();
            }
            else
                e.DrawBackground();

            // Invoking the DrawItem event
            base.OnDrawItem(e);
        }

        /// <inheritdoc />
        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            base.OnDropDownStyleChanged(e);
            AdjustDrawMode();
            ResetColors(); // because DisabledForeColor depends on this property

            // Handling read-only for new style on Framework Mono. Otherwise, it's handled in OnHandleCreated.
            if (OSHelper.IsFrameworkMono)
                AdjustReadOnlyOnFrameworkMono();
        }

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // NOTE: ComboBox.WndProc does not see WM_PASTE and other messages so they are captured in InnerEditorWindow
            switch (m.Msg)
            {
                // Suppressing dropping list down
                case Constants.WM_LBUTTONDOWN or Constants.WM_LBUTTONDBLCLK when ReadOnly:
                    ProcessReadOnlyMouseDown(ref m);
                    return;

                case Constants.WM_PAINT:
                    if (Enabled)
                    {
                        // BUG workaround: In .NET 7+ the control resets the Font in WM_DPICHANGED_BEFOREPARENT, which causes a handle recreation and an immediate repaint.
                        // If we also set the font here, it will cause a Win32Exception (Error creating window handle)
                        if (dpiChangingCount == 0)
                            CheckDpiChange();
                        base.WndProc(ref m);
                        return;
                    }

                    // As there is no overridable OnFlatStyleChanged we detect FlatStyle change here.
                    // This is required because DisabledForeColor depends on FlatStyle.
                    var flatStyle = FlatStyle;
                    if (lastFlatStyle != flatStyle)
                    {
                        lastFlatStyle = flatStyle;
                        if (ResetColors())
                            return; // invalidation occurred, so there will be a new paint message
                    }

                    // BUG workaround: see above
                    if (dpiChangingCount == 0)
                        CheckDpiChange();

                    // In System DrawDropDownList mode we completely redraw the control by visual styles renderer, so just validating the control to prevent repeated WM_PAINT messages.
                    if (DrawByVisualStylesWhenDisabled)
                        User32.ValidateRect(m.HWnd, IntPtr.Zero);
                    // otherwise, we let the system paint the control first for the borders
                    else
                        base.WndProc(ref m);

                    if (systemDrawDropDownListMode && DropDownStyle == ComboBoxStyle.DropDownList && !OSHelper.IsFrameworkMono)
                    {
                        var bounds = OSHelper.IsWindows
                            ? User32.GetClientRect(m.HWnd, out RECT rect) ? rect.ToRectangle() : Rectangle.Empty
                            : ClientRectangle;
                        if (!bounds.IsEmpty())
                        {
                            using var g = Graphics.FromHwnd(m.HWnd);
                            DrawDisabledTextBox(g, bounds);
                        }
                    }

                    return;

                case Constants.WM_DPICHANGED_BEFOREPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }

                    CheckDpiChange();
                    return;

                case Constants.WM_DPICHANGED_AFTERPARENT:
                    dpiChangingCount += 1;
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        dpiChangingCount -= 1;
                    }
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Control? parent = Parent;
            if (parent == null)
                return;

            // Setting default font from new parent font without scaling
            if (font == null)
            {
                PointF scale = this.GetScaleForParentChanged();
                defaultFont?.ResetFrom(ScaleHelper.GetFontOrDefault(parent.Font), scale);
                if (this.GetScale() != scale)
                    lastScale = PointF.Empty;
            }

            CheckDpiChange();
        }

        /// <inheritdoc />
        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            // without this Text may get selected even if not focused
            if (!Focused && DropDownStyle != ComboBoxStyle.DropDownList)
                SelectionLength = 0;

            // if the parent control is rescaling its font due to DPI change, then ignoring the event (we do our scaling in CheckDpiChange)
            if (dpiChangingCount > 0 || !AutoScaleFont)
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
            if (this.GetScale() != scale)
                lastScale = PointF.Empty;
        }

        #endregion

        #region Private Methods

        private void InitHooks()
        {
            Debug.Assert(IsHandleCreated && !OSHelper.IsFrameworkMono);
            if (DropDownStyle == ComboBoxStyle.Simple)
            {
                // Hooking inner list box the same way as the base class does. In Simple mode the first child is the list box.
                IntPtr hwnd = User32.GetWindow(Handle, Constants.GW_CHILD);
                if (hwnd != IntPtr.Zero)
                {
                    nativeListBoxChild = new InnerListBoxWindow(this);
                    nativeListBoxChild.AssignHandle(hwnd);
                }
            }

            if (DropDownStyle != ComboBoxStyle.DropDownList)
            {
                // hooking inner text box to capture WM_PASTE and others
                IntPtr lhWnd = User32.FindWindowEx(Handle, IntPtr.Zero, "EDIT", null);
                if (lhWnd != IntPtr.Zero)
                {
                    nativeEditorChild = new InnerEditWindow(this);
                    nativeEditorChild.AssignHandle(lhWnd);
                }
            }
        }

        private void ReleaseHooks()
        {
            nativeListBoxChild?.ReleaseHandle();
            nativeListBoxChild = null;
            nativeEditorChild?.ReleaseHandle();
            nativeEditorChild = null;
        }

        private void ProcessReadOnlyMouseDown(ref Message m)
        {
            if (!Focused)
                Focus();

            OnMouseDown(new MouseEventArgs(MouseButtons.Left, m.Msg is Constants.WM_LBUTTONDOWN ? 1 : 2, m.LParam.SignedLOWORD(), m.LParam.SignedHIWORD(), 0));

            if (OSHelper.IsFrameworkMono)
                return;

            // This is required to raise the Click event when the mouse button is released
            this.SetMouseEvents();
            Capture = true;
        }

        private void AdjustDrawMode()
        {
            bool customDraw = DropDownStyle == ComboBoxStyle.Simple || !systemDrawDropDownListMode || OSHelper.IsFrameworkMono;
            DrawMode drawMode = customDraw ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
            if (base.DrawMode != drawMode)
                base.DrawMode = drawMode;
        }

        private bool ResetColors()
        {
            bool enabled = Enabled;
            Color baseBackColor = base.BackColor;
            Color baseForeColor = base.ForeColor;
            bool changed = false;

            if (enabled && !readOnly && EnabledBackColor is Color enabledBgColor && enabledBgColor != baseBackColor)
            {
                base.BackColor = enabledBgColor;
                changed = true;
            }
            else if ((readOnly || !enabled) && DisabledBackColor is Color disabledBgColor && disabledBgColor != baseBackColor)
            {
                base.BackColor = disabledBgColor;
                changed = true;
            }

            if (enabled && EnabledForeColor is Color enabledFgColor && enabledFgColor != baseForeColor)
            {
                base.ForeColor = enabledFgColor;
                changed = true;
            }
            else if (!enabled && DisabledForeColor is Color disabledFgColor && disabledFgColor != baseForeColor)
            {
                base.ForeColor = disabledFgColor;
                changed = true;
            }

            return changed;
        }

        private bool ShouldSerializeFont() => font != null;
        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        private void DrawDisabledTextBox(Graphics g, Rectangle bounds)
        {
            var style = DropDownStyle;
            var clientRect = bounds;
            bool rtl = RightToLeft == RightToLeft.Yes;
            bool visualStyles = VisualStyleHelper.RenderWithVisualStyles;
            int dropDownButtonWidth = 0;
            if (style == ComboBoxStyle.DropDownList)
            {
                bounds.X += visualStyles || !rtl ? 2 : 4;
                bounds.Y += 2;
                bounds.Width -= visualStyles || !rtl ? 5 : 6;
                bounds.Height -= 4;

                // We could use SystemInformation.GetHorizontalScrollBarArrowWidthForDpi on .NET Framework 4.7.2 and above,
                // but that works only for V2 per-monitor DPI awareness, and only on Windows 10 and above.
                // This may cause that the disabled rendering will look better than the enabled one on some platforms.
                dropDownButtonWidth = this.ScaleWidth(referenceDropDownWidth);
                bounds.Width -= dropDownButtonWidth;
                if (rtl)
                    bounds.X += dropDownButtonWidth;
            }
            else
            {
                bounds.X -= 3;
                bounds.Width += 7;
            }

            // System DropDownList mode: not clearing with background color but drawing the disabled background by visual styles
            if (DrawByVisualStylesWhenDisabled)
            {
                Debug.Assert(OSHelper.IsWindowsVistaOrLater);
                VisualStyleHelper.Render(VisualStyleHelper.ComboBoxTheme, this, g, (int)COMBOBOXPARTS.CP_READONLY, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, clientRect);

                var part = rtl ? COMBOBOXPARTS.CP_DROPDOWNBUTTONLEFT : COMBOBOXPARTS.CP_DROPDOWNBUTTONRIGHT;
                var buttonSize = new Size(dropDownButtonWidth, clientRect.Height);
                var dropDownButtonBounds = new Rectangle(Point.Empty, buttonSize);
                if (!rtl)
                    dropDownButtonBounds.X = clientRect.Right - buttonSize.Width;
                
                VisualStyleHelper.Render(VisualStyleHelper.ComboBoxTheme, this, g, (int)part, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, dropDownButtonBounds);
            }
            else
                g.FillRectangle(BackColor.GetBrush(), bounds);

            Rectangle textRect = clientRect;
            if (style == ComboBoxStyle.DropDownList)
            {
                textRect.Inflate(-4, -4);
                if (rtl)
                    textRect.X += 2;
            }

            TextRenderer.DrawText(g, base.Text, Font, textRect, ForeColor, this.GetFormatFlags());
        }

        private void CheckDpiChange()
        {
            // BUG workaround: If Font is changed while the control is not created (even when IsHandleCreated is already true), the control may not appear. Occurs in Simple mode.
            if (!Created)
                return;

            PointF scale = this.GetScale();

            // The Font check is needed for .NET 6, where WinForms' (bad) auto font scaling may occur without notification
            if ((scale == lastScale && (!AutoScaleFont || (font ?? defaultFont)?.Font.Equals(Font) == true)) || Disposing || IsDisposed)
                return;

            lastScale = scale;
            if (!AutoScaleFont)
                return;

            if (font is ScalingFont explicitFont)
                explicitFont.Scale(scale);
            else
                defaultFont!.Scale(scale);
            SetFont(font ?? defaultFont);
        }

        private void SetFont(ScalingFont? value)
        {
            if (value == null)
            {
                base.Font = null!;
                return;
            }

            Font oldFont = base.Font;
            Font newFont = value.Font;

            // If base.Font equals to newFont.Font, then setting the new one does nothing. This matters if the old font is already
            // disposed or when the control is in a broken state so it displays some default font. In such cases we must set null first.
            // No optimization with reference equality for the AdvancedComboBox, because it can happen that the displayed font size is different
            // from the one that the base.Font property returns. Occurs typically in .NET 6+ when handles are created early,
            // and the system scale wad changed after starting the application.
            if (Equals(oldFont, newFont))
            {
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

            // without this Text may get selected even if not focused
            if (!Focused && DropDownStyle != ComboBoxStyle.DropDownList)
                SelectionLength = 0;
        }

        private void AdjustReadOnlyOnFrameworkMono()
        {
            Debug.Assert(OSHelper.IsFrameworkMono);
            var style = DropDownStyle;
            if (style == ComboBoxStyle.Simple)
                this.InnerListBox()?.Enabled = !readOnly;
            if (style != ComboBoxStyle.DropDownList)
                this.InnerTextBox()?.ReadOnly = readOnly;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanging()
        {
            dpiChangingCount += 1;
            if (isPerMonitorDpiAwarenessV1)
                CheckDpiChange();
        }

        void IPerMonitorDpiAware.ParentFormDpiChanged()
        {
            Debug.Assert(dpiChangingCount > 0);
            dpiChangingCount -= 1;
        }

        #endregion

        #region Event Handlers

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e) => ResetColors(); // because DisabledForeColor may depend on visual styles

        #endregion

        #endregion

        #region Former IListControl Obsolete Members

        /// <summary>
        /// Gets whether the there is no selected item in the combo box (<see cref="ListControl.SelectedValue"/> is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlExtensions.NotSelectedValue"/>)
        /// </summary>
        [Obsolete("This property reflects the special value represented by the obsoleted SelectionPlusItems and should not be used")]
        [Browsable(false)]
        public bool IsEmpty => this.IsEmpty();

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        [Obsolete("LoadFrom methods are obsolete. Names are not auto-translated anymore and SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, dataTable, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        [Obsolete("LoadFrom methods are obsolete. SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, dataTable, valueMember, displayMember, plusItems);

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        [Obsolete("LoadFrom methods are obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember)
            => ListControlExtensions.LoadFrom(this, dataTable, valueMember, displayMember);

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        [Obsolete("LoadFrom methods are obsolete. Names are not auto-translated anymore and SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, enumType, valueMemberType, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        [Obsolete("LoadFrom methods are obsolete. SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, enumType, valueMemberType, plusItems);

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        [Obsolete("LoadFrom methods are obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(Type enumType, Type valueMemberType)
            => ListControlExtensions.LoadFrom(this, enumType, valueMemberType);

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        [Obsolete("LoadFrom methods are obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(Type enumType) => ListControlExtensions.LoadFrom(this, enumType);

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        [Obsolete("LoadFrom methods are obsolete. Names are not auto-translated anymore and SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, collection, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        [Obsolete("LoadFrom methods are obsolete. SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, collection, valueMember, displayMember, plusItems);

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        [Obsolete("LoadFrom methods are obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember)
            => ListControlExtensions.LoadFrom(this, collection, valueMember, displayMember);

        #endregion
    }
}
