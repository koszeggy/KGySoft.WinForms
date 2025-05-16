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
        /// Hooks WndProc of the inner editor window to deny WM_PASTE in ReadOnly mode.
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
                if (!parent.readOnly)
                {
                    // workaround: AutoComplete clears text in Simple mode
                    // Note: WM_SETTEXT is visible also in ComboBox.WndProc but solves only Append/SuggestAppend mode. Here Suggest mode is solved, too
                    if (!parent.clearingText && m.Msg == Constants.WM_SETTEXT && parent.style == ComboBoxStyle.Simple && parent.AutoCompleteMode != AutoCompleteMode.None)
                    {
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
                    }

                    base.WndProc(ref m);
                    return;
                }

                // *** read-only mode processings below ***

                // suppressing editing in ReadOnly mode
                switch (m.Msg)
                {
                    case Constants.WM_CUT:
                    case Constants.WM_CLEAR:
                    case Constants.WM_PASTE:
                    case Constants.WM_UNDO:
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
        /// Hooks WndProc of the inner editor window to deny WM_PASTE in ReadOnly mode.
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

        private Color disabledBackColor = SystemColors.Control;
        private Color disabledForeColor = SystemColors.ControlDarkDark;
        private Color enabledBackColor = SystemColors.Window;
        private Color enabledForeColor = SystemColors.WindowText;
        private ComboBoxStyle style = ComboBoxStyle.DropDown;
        private bool styleChanging;
        private string textSaved = String.Empty;
        private int indexSaved = -1;
        private Bitmap? bmpSaved;
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

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        public override string Text
        {
            get => !Enabled ? textSaved : base.Text;
            set
            {
                base.Text = value;
                if (!Enabled)
                {
                    indexSaved = -1;
                    textSaved = value;
                    SetPaintMode();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the control in current state.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (ReadOnly || !Enabled)
                    DisabledBackColor = value;
                else
                    EnabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in current state.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (!Enabled)
                    DisabledForeColor = value;
                else
                    EnabledForeColor = value;
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
        /// Gets or sets a value specifying the style of the combo box.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Description("Gets or sets a value specifying the style of the combo box.")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        public new ComboBoxStyle DropDownStyle
        {
            get => style;
            set
            {
                if (style != value)
                {
                    if (base.DropDownStyle == ComboBoxStyle.DropDownList && value == ComboBoxStyle.DropDownList)
                    {
                        styleChanging = true;
                        base.DropDownStyle = ComboBoxStyle.DropDown;
                        styleChanging = false;
                    }

                    base.DropDownStyle = value;
                }
            }
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
                if (!DesignMode && !IsHandleCreated && base.DropDownStyle == ComboBoxStyle.Simple)
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
        /// ForeColor when control is Enabled.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("ForeColor when control is Enabled.")]
        [DefaultValue(typeof(Color), "WindowText")]
        public Color EnabledForeColor
        {
            get => enabledForeColor;
            set
            {
                enabledForeColor = value;
                ResetColor();
            }
        }

        /// <summary>
        /// BackColor when control is Enabled and not ReadOnly.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("BackColor when control is Enabled and not ReadOnly.")]
        [DefaultValue(typeof(Color), "Window")]
        public Color EnabledBackColor
        {
            get => enabledBackColor;
            set
            {
                enabledBackColor = value;
                ResetColor();
            }
        }

        /// <summary>
        /// BackColor when control is not Enabled or is ReadOnly.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("BackColor when control is not Enabled or is ReadOnly.")]
        [DefaultValue(typeof(Color), "Control")]
        public Color DisabledBackColor
        {
            get => disabledBackColor;
            set
            {
                disabledBackColor = value;
                ResetColor();
            }
        }

        /// <summary>
        /// ForeColor when control is not Enabled.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("ForeColor when control is not Enabled.")]
        [DefaultValue(typeof(Color), "ControlDarkDark")]
        public Color DisabledForeColor
        {
            get => disabledForeColor;
            set
            {
                disabledForeColor = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets whether the enabled combo box should be drawn by the system in <see cref="ComboBoxStyle.DropDownList"/> mode.
        /// If this property is <see langword="false"/>, then drop-down list appearance will be the same as in case of <see cref="ComboBoxStyle.DropDown"/> mode
        /// even with Windows Vista/Windows 7 themes.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Gets or sets whether the enabled combo box should be drawn by the system in DropDownList mode. " +
                    "If this property is false, then drop-down list appearance will be the same as in case of DropDown mode " +
                    "even with Windows Vista/Windows 7 themes.")]
        [DefaultValue(true)]
        public bool SystemDrawDropDownListMode
        {
            get => systemDrawDropDownListMode;
            set
            {
                systemDrawDropDownListMode = value;
                AdjustDrawMode();
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
                if (readOnly != value)
                {
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
                        if (base.DropDownStyle != ComboBoxStyle.DropDownList)
                        {
                            base.AutoCompleteMode = origCompleteMode;
                            base.AutoCompleteSource = origCompleteSource;
                        }
                    }

                    readOnly = value;
                    if (Enabled)
                    {
                        AdjustDrawMode();
                        ResetColor();
                    }

                    OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedComboBox"/>
        /// </summary>
        public AdvancedComboBox()
        {
            SetPaintMode();
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Clears <see cref="Text"/> property. If <see cref="AutoCompleteMode"/> property is set on a simple mode combo box, then
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
            styleChanging = true;
            try
            {
                // enabling
                if (Enabled)
                {
                    // restoring style
                    base.DropDownStyle = style;
                    SetPaintMode();

                    textAndFontChanging = true;
                    try
                    {
                        // resetting font must be before restoring text because "Font = font;" may select an item!

                        // Without these lines font would be replaced to some bold one after Enabled off->on
                        Font font = Font;
                        // ReSharper disable AssignNullToNotNullAttribute
                        Font = null;
                        // ReSharper restore AssignNullToNotNullAttribute
                        Font = font;

                        // without this text might remain selected even if not focused
                        if (!Focused && style != ComboBoxStyle.DropDownList)
                            SelectionLength = 0;

                        // restoring last selected item (strictly after resetting font!)
                        if (indexSaved >= 0)
                        {
                            if (indexSaved < Items.Count)
                                SelectedIndex = indexSaved;
                        }
                        else if (style != ComboBoxStyle.DropDownList)
                            base.Text = textSaved;
                    }
                    finally
                    {
                        textAndFontChanging = false;
                    }

                    // if readonly was changed in disabled style original auto complete should be restored here
                    if (!readOnly && origCompleteSource != AutoCompleteSource.None && base.DropDownStyle != ComboBoxStyle.DropDownList
                        && (base.AutoCompleteSource != origCompleteSource || base.AutoCompleteMode != origCompleteMode))
                    {
                        base.AutoCompleteMode = origCompleteMode;
                        base.AutoCompleteSource = origCompleteSource;
                    }

                    // if readonly was changed while control was disabled, this adjust is needed
                    AdjustDrawMode();
                    ResetColor();
                }
                // disabling
                else
                {
                    // saving current style/index/text/auto complete
                    style = base.DropDownStyle;
                    indexSaved = SelectedIndex;
                    textSaved = base.Text;
                    if (!readOnly)
                    {
                        origCompleteMode = base.AutoCompleteMode;
                        origCompleteSource = base.AutoCompleteSource;
                    }
                    // OnPaint works only in DropDownList mode. But Text/index may be lost that's why they were saved.
                    base.DropDownStyle = ComboBoxStyle.DropDownList;

                    SetPaintMode();
                }
            }
            finally
            {
                styleChanging = false;
            }
        }

        /// <inheritdoc />
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (styleChanging)
                return;
            SetPaintMode();
        }

        /// <inheritdoc />
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            if (bmpSaved != null)
            {
                bmpSaved.Dispose();
                bmpSaved = null;
            }
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
                || base.DropDownStyle == ComboBoxStyle.DropDownList && e.KeyCode is Keys.Space or Keys.Right or Keys.Left or Keys.Home or Keys.End))
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
            if (disposing)
                bmpSaved?.Dispose();
            bmpSaved = null;
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
                string label = GetItemText(Items[e.Index]);

                Brush brFore;
                Brush brBack;
                //// Disabled combo (dropdownlist mode)
                //if ((int)(e.State & DrawItemState.Disabled) > 0)
                //{
                //    brFore = new SolidBrush(disabledForeColor);
                //    brBack = new SolidBrush(disabledBackColor);
                //} else
                // Non focused list item
                if ((int)(e.State & DrawItemState.Selected) == 0)
                {
                    brFore = ForeColor.GetBrush();
                    brBack = BackColor.GetBrush();
                }
                // Focused list item
                else
                {
                    brFore = SystemBrushes.HighlightText;
                    brBack = SystemBrushes.Highlight;
                }
                e.Graphics.FillRectangle(brBack, e.Bounds);
                e.Graphics.DrawString(label, e.Font, brFore, e.Bounds.Left - 2, e.Bounds.Top);
                e.DrawFocusRectangle();
            }
            // drawing the unselected control in dropdownlist mode
            else
            {
                base.OnDrawItem(e);
            }
        }

        /// <summary>
        /// Drawing appearance in disabled mode (works only in DropDownList mode)
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!Enabled)
            {
                Graphics g = e.Graphics;

                ////This would be an elegant solution but actually would be very ugly (while colors cannot be affected):
                //// drawing textbox with text
                //Rectangle rectangle = ClientRectangle;
                //if (style != ComboBoxStyle.Simple)
                //    rectangle.Width -= 16;
                //ComboBoxRenderer.DrawTextBox(g, rectangle, textSaved, Font,  System.Windows.Forms.VisualStyles.ComboBoxState.Disabled);
                //// drawing button
                //if (style != ComboBoxStyle.Simple)
                //    ComboBoxRenderer.DrawDropDownButton(g, new Rectangle(ClientRectangle.Width - 16, 0, 16, ClientRectangle.Height), System.Windows.Forms.VisualStyles.ComboBoxState.Disabled);

                Rectangle bounds = ClientRectangle;

                // saving the disabled image of the control (this saves a dropdownlist appearance)
                if (bmpSaved == null)
                {
                    if (Width <= 0 || Height <= 0)
                        return;

                    SetStyle(ControlStyles.UserPaint, false);
                    bmpSaved = new Bitmap(bounds.Width, bounds.Height);
                    DrawToBitmap(bmpSaved, bounds);
                    SetStyle(ControlStyles.UserPaint, true);
                }

                // drawing background
                g.DrawImage(bmpSaved, 0, 0);

                // filling with disabled background
                bounds.X += 2;
                bounds.Y += 2;
                bounds.Width -= 4;
                bounds.Height -= 4;
                bool ltr = RightToLeft == RightToLeft.Yes;
                if (style != ComboBoxStyle.Simple)
                {
                    bounds.Width -= 16; // assuming that dropdown button is 16 px wide
                    if (ltr)
                        bounds.X += 16;
                }

                g.FillRectangle(disabledBackColor.GetBrush(), bounds);

                // drawing text
                bounds.Width += 6;
                if (style == ComboBoxStyle.DropDownList)
                    bounds.X -= 1;
                else
                {
                    bounds.X -= 2;
                    bounds.Y -= 1;
                }
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;
                if (ltr)
                    flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                TextRenderer.DrawText(g, textSaved, Font, bounds, Enabled ? enabledForeColor : disabledForeColor, flags);
            }
        }

        /// <inheritdoc />
        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            if (styleChanging)
                return;

            base.OnDropDownStyleChanged(e);

            style = base.DropDownStyle;
            AdjustDrawMode();
            if (!Enabled)
            {
                styleChanging = true;
                try
                {
                    base.DropDownStyle = ComboBoxStyle.DropDownList;
                }
                finally
                {
                    styleChanging = false;
                }
            }

            SetPaintMode();
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (bmpSaved != null)
            {
                bmpSaved.Dispose();
                bmpSaved = null;
            }
            Invalidate();
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

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        #endregion

        #region Private Methods

        private void InitHooks()
        {
            if (base.DropDownStyle == ComboBoxStyle.Simple)
            {
                // Hooking inner list box the same way as the base class does. In Simple mode the first child is the list box.
                IntPtr hwnd = User32.GetWindow(Handle, Constants.GW_CHILD);
                if (hwnd != IntPtr.Zero)
                {
                    nativeListBoxChild = new InnerListBoxWindow(this);
                    nativeListBoxChild.AssignHandle(hwnd);
                }
            }

            if (base.DropDownStyle != ComboBoxStyle.DropDownList)
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

        private void SetPaintMode()
        {
            if (DesignMode)
                return;
            bool userPaint = !Enabled && (style != ComboBoxStyle.DropDownList || SelectedIndex >= 0);
            SetStyle(ControlStyles.UserPaint, userPaint);
            Invalidate();
        }

        private void AdjustDrawMode()
        {
            bool customDraw = !Enabled || style != ComboBoxStyle.DropDownList || !systemDrawDropDownListMode;
            DrawMode drawMode = customDraw ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
            if (base.DrawMode != drawMode)
                base.DrawMode = drawMode;
        }

        private void ResetColor()
        {
            // BackColor when control is Enabled and not ReadOnly
            if (Enabled && !ReadOnly && base.BackColor != enabledBackColor)
                base.BackColor = enabledBackColor;

            // BackColor when control is not Enabled or is ReadOnly
            else if ((Enabled && ReadOnly || !Enabled) && base.BackColor != disabledBackColor)
                base.BackColor = disabledBackColor;

            // ForeColor in Enabled state (also ReadOnly)
            if (Enabled && base.ForeColor != enabledForeColor)
                base.ForeColor = enabledForeColor;

            // ForeColor in disabled state (ReadOnly state is indifferent)
            else if (!Enabled && base.ForeColor != disabledForeColor)
                base.ForeColor = disabledForeColor;

            Invalidate();
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
