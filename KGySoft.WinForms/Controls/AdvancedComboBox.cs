#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedComboBox.cs
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Advanced version of <see cref="ComboBox"/> control that supports read-only mode and customized coloring even in disabled state.
    /// Furthermore, supports numerous data-bound combo initializations and fixes an auto complete bug: in original combo box auto complete
    /// does not work in <see cref="ComboBoxStyle.Simple"/> mode.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBox))]
    [Description(@"A combo box with the following additional features:
- Disabled colors
- ReadOnly property and ReadOnlyChanged event
- TextChangedOnLeave
- LoadFrom methods
- Auto complete works in Simple mode")]
    public class AdvancedComboBox : ComboBox, ISupportsDisabledColor, IReadOnlyCapable
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
                        base.WndProc(ref m);
                        var bounds = User32.GetClientRect(Handle, out var rect) ? rect.ToRectangle() : Rectangle.Empty;
                        if (!bounds.IsEmpty)
                        {
                            using var g = Graphics.FromHwnd(Handle);
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

        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledOrReadOnlyBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color defaultReadOnlyForeColor = SystemColors.ControlText;

        #endregion

        #region Instance Fields

        // NOTE: Unlike in ButtonBase descendants, we always set the base back and fore colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        private FlatStyle lastFlatStyle = FlatStyle.Standard; // would not be needed if there was an overridable OnFlatStyleChanged method
        private bool systemDrawDropDownListMode = true;
        private bool readOnly;
        private InnerEditWindow? nativeEditorChild;
        private InnerListBoxWindow? nativeListBoxChild;
        private string? textOnFocus;
        private bool textAndFontChanging;
        private AutoCompleteSource origCompleteSource = AutoCompleteSource.None;
        private AutoCompleteMode origCompleteMode = AutoCompleteMode.None;
        private bool clearingText;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when <see cref="ReadOnly"/> property has been changed.
        /// </summary>
        [Description("Occurs when ReadOnly property has been changed.")]
        [Category("AdvancedComboBox")]
        public event EventHandler? ReadOnlyChanged;

        /// <summary>
        /// Occurs on leaving the control when content is different from the original one when the control was focused.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused.")]
        public event EventHandler? TextChangedOnLeave;

        #endregion

        #region Properties

        #region Static Properties

        private static Color ThemedDisabledDropDownListColor => VisualStyleHelper.GetTextColor(VisualStyleHelper.ComboBoxTheme, (int)COMBOBOXPARTS.CP_READONLY, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, defaultDisabledForeColor);

        #endregion

        #region Instance Properties

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
                : VisualStyleHelper.RenderWithVisualStyles && SystemDrawDropDownListMode
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
        /// Do not set this property. DrawMode is automatically managed in <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode
        {
            get => base.DrawMode;
            set => throw new NotSupportedException("DrawMode cannot be set in AdvancedComboBox");
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner combo box.
        /// </summary>
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
        /// Gets or sets whether the combo box should have the default system appearance <see cref="ComboBoxStyle.DropDownList"/> mode.
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

                OnReadOnlyChanged(EventArgs.Empty);
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedComboBox"/>
        /// </summary>
        public AdvancedComboBox()
        {
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
                textAndFontChanging = true;
                try
                {
                    // without this text might remain selected even if not focused
                    if (!Focused && style != ComboBoxStyle.DropDownList)
                        SelectionLength = 0;
                }
                finally
                {
                    textAndFontChanging = false;
                }

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
            InitHooks();
        }

        /// <inheritdoc />
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseHooks();
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

        /// <inheritdoc />
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            ResetColors(); // can be relevant when switching between high contrast and normal mode
        }

        /// <summary>
        /// Raises the <see cref="ReadOnlyChanged"/> event.
        /// </summary>
        protected virtual void OnReadOnlyChanged(EventArgs e) => ReadOnlyChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        protected virtual void OnTextChangedOnLeave(EventArgs e) => TextChangedOnLeave?.Invoke(this, e);

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            // suppressing event if changing is a workaround
            if (textAndFontChanging)
                return;
            base.OnTextChanged(e);
        }

        /// <inheritdoc />
        protected override void OnFontChanged(EventArgs e)
        {
            // suppressing event if changing is a workaround
            if (textAndFontChanging)
                return;
            base.OnFontChanged(e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // suppressing deleting and navigation (selecting item from list) because these cannot be suppressed in KeyPress
            if (readOnly && (e.KeyCode is Keys.Delete or Keys.Back or Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown
                || DropDownStyle == ComboBoxStyle.DropDownList && e.KeyCode is Keys.Space or Keys.Right or Keys.Left or Keys.Home or Keys.End))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (readOnly)
            {
                // allowing only Ctrl+C (Copy) - Ctrl+Insert is not captured here
                e.Handled = e.KeyChar != (char)3; //!e.KeyChar.In((char)3, (char)13, (char)27);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            ReleaseHooks();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws an item in the dropdown area and also in the control area in dropdownlist mode.
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

                e.Graphics.FillRectangle(backColor.GetBrush(), e.Bounds);
                TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, foreColor, backColor, this.GetFormatFlags());
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

                case Constants.WM_PAINT when !Enabled:
                    // As there is no overridable OnFlatStyleChanged we detect FlatStyle change here.
                    // This is required because DisabledForeColor depends on FlatStyle.
                    var flatStyle = FlatStyle;
                    if (lastFlatStyle != flatStyle)
                    {
                        lastFlatStyle = flatStyle;
                        if (ResetColors())
                            return; // invalidation occurred, so there will be a new paint message
                    }

                    base.WndProc(ref m);

                    if (systemDrawDropDownListMode && (DropDownStyle == ComboBoxStyle.DropDownList || nativeEditorChild == null))
                    {
                        var bounds = User32.GetClientRect(Handle, out var rect) ? rect.ToRectangle() : Rectangle.Empty;
                        if (!bounds.IsEmpty)
                        {
                            using var g = Graphics.FromHwnd(Handle);
                            DrawDisabledTextBox(g, bounds);
                        }
                    }

                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        #endregion

        #region Private Methods

        private void InitHooks()
        {
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

            // This is required to raise the Click event when the mouse button is released
            this.SetMouseEvents();
            Capture = true;
        }

        private void AdjustDrawMode()
        {
            bool customDraw = DropDownStyle == ComboBoxStyle.Simple || !systemDrawDropDownListMode;
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
            if (style == ComboBoxStyle.DropDownList)
            {
                bounds.X += visualStyles || !rtl ? 2 : 4;
                bounds.Y += 2;
                bounds.Width -= visualStyles || !rtl ? 5 : 6;
                bounds.Height -= 4;
                bounds.Width -= 17; // assuming that dropdown button is 17 px wide
                if (rtl)
                    bounds.X += 17;
            }
            else
            {
                bounds.X -= 3;
                bounds.Width += 7;
            }

            // System DropDownList mode: not clearing with background color but drawing the disabled background by visual styles
            if (style == ComboBoxStyle.DropDownList && systemDrawDropDownListMode && VisualStyleHelper.RenderWithVisualStyles
                && FlatStyle is FlatStyle.System or FlatStyle.Standard)
            {
                VisualStyleHelper.Render(VisualStyleHelper.ComboBoxTheme, this, g, (int)COMBOBOXPARTS.CP_READONLY, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, clientRect);

                var part = rtl ? COMBOBOXPARTS.CP_DROPDOWNBUTTONLEFT : COMBOBOXPARTS.CP_DROPDOWNBUTTONRIGHT;
                var buttonSize = new Size(17, 21); // TODO: scale
                var dropDownButtonBounds = new Rectangle(Point.Empty, buttonSize);
                if (!rtl)
                    dropDownButtonBounds.X = clientRect.Right - buttonSize.Width;
                VisualStyleHelper.Render(VisualStyleHelper.ComboBoxTheme, this, g, (int)part, (int)COMBOBOXSTYLESTATES.CBXS_DISABLED, dropDownButtonBounds);
            }
            else
                g.FillRectangle(BackColor.GetBrush(), bounds);

            if (style == ComboBoxStyle.DropDownList)
            {
                bounds.X -= visualStyles
                    ? !rtl ? 1 : 0
                    : !rtl ? 1 : 2;
                bounds.Y += 2;
                bounds.Width += 5;
            }

            TextRenderer.DrawText(g, base.Text, Font, bounds, ForeColor, this.GetFormatFlags());
        }

        #endregion

        #endregion

        #region IListControl Members

        /// <summary>
        /// Gets whether the there is no selected item in the combo box (<see cref="ComboBox.SelectedValue"/> is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlExtensions.NotSelectedValue"/>)
        /// </summary>
        [Obsolete("This property reflects the special value represented by the obsoleted SelectionPlusItems and should not be used")]
        public bool IsEmpty => this.IsEmpty();

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
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
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
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
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        [Obsolete("LoadFrom methods are obsolete. SelectionPlusItems enumeration is also obsolete. Provide a data source by a view model class instead.")]
        public void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
            => ListControlExtensions.LoadFrom(this, enumType, valueMemberType, plusItems);

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
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
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
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
