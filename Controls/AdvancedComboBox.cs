/*******************************************
 * AdvancedComboBox - KGy
 * 
 * Problems with original ComboBox:
 * - Text is always gray when control is disabled
 * - In Simple mode auto complete does not work
 * - There is no ReadOnly mode
 * 
 * Further features:
 * - LoadFrom overloads
 * - ReadOnlyChanged event
 * - TextChangedOnLeave event
 * 
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using KGySoft.Controls.WinApi;
using KGySoft.Libraries;
using KGySoft.Libraries.Reflection;

namespace KGySoft.Controls
{
    // TODO: DisabledFore/Back color mint pl. az AdvancedButton-nál: a base colorok figyelembe vételével
    // TODO: Disabled állapot ne Image legyen, hanem paint a megfelelő rendererrel
    // TODO: readonly dropdownlist mód lehetőleg ne legyen disabled
    /// <summary>
    /// Advanced version of <see cref="ComboBox"/> control that supports read-only mode and customized coloring even in disabled state.
    /// Furthermore, supports numerous data-bound combo initializations and fixes an auto complete bug: in original combo box auto complete
    /// does not work in <see cref="ComboBoxStyle.Simple"/> mode.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBox))]
    public class AdvancedComboBox: ComboBox, IDisabledColorCapable, IListControl, IReadOnlyCapable
    {
        #region InnerEditorWindow class

        /// <summary>
        /// Hooks WndProc of the inner editor window to deny WM_PASTE
        /// </summary>
        private sealed class InnerEditorWindow: NativeWindow
        {
            private readonly AdvancedComboBox parent;

            public InnerEditorWindow(AdvancedComboBox parent)
            {
                this.parent = parent;
            }

            [DebuggerStepThrough]
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
        }

        #endregion

        #region Fields

        private Color disabledBackColor = SystemColors.Control;
        private Color disabledForeColor = SystemColors.ControlDarkDark;
        private Color enabledBackColor = SystemColors.Window;
        private Color enabledForeColor = SystemColors.WindowText;

        private static FieldAccessor stateField;

        private ComboBoxStyle style = ComboBoxStyle.DropDown;
        private bool styleChanging;
        private string textSaved;
        private int indexSaved = -1;
        private Bitmap bmpSaved;
        private bool systemDrawDropDownListMode = true;
        private bool readOnly;
        private InnerEditorWindow hook;
        private bool virtualEnabledChanging;
        private bool selfEnabled = true;
        private Control parent;
        private string textOnFocus;
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
        public event EventHandler ReadOnlyChanged;

        /// <summary>
        /// Occurs on leaving the control when content is different from the original one when the control was focused.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused.")]
        public event EventHandler TextChangedOnLeave;

        #endregion

        #region Properties

        #region Overridden properties

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        public override string Text
        {
            get
            {
                return !base.Enabled ? textSaved : base.Text;
            }
            set
            {
                base.Text = value;
                if (!base.Enabled)
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
            [DebuggerStepThrough]
            get
            {
                return base.BackColor;
            }
            set
            {
                if (ReadOnly || !base.Enabled)
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
            [DebuggerStepThrough]
            get
            {
                return base.ForeColor;
            }
            set
            {
                if (!base.Enabled)
                    DisabledForeColor = value;
                else
                    EnabledForeColor = value;
            }
        }

        #endregion

        #region Reintroduced Properties

        /// <summary>
        /// Do not set this property. DrawMode is automatically managed in <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode
        {
            get { return base.DrawMode; }
            set { throw new NotSupportedException("DrawMode cannot be set in AdvancedComboBox"); }
        }

        /// <summary>
        /// Gets or sets a value specifying the style of the combo box.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Description("Gets or sets a value specifying the style of the combo box.")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return style; }
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
        /// Gets or sets a value indicating whether the control can respond to user interaction.
        /// </summary>
        public new bool Enabled
        {
            get
            {
                // readonly dropdown list: actually disabled so returning combination of resultant and logically self enabled
                if (readOnly && style == ComboBoxStyle.DropDownList)
                {
                    if (Parent == null || Parent.Enabled)
                        return selfEnabled;
                    return Parent.Enabled;
                }

                // any other: regular enabled
                return base.Enabled;
            }
            set
            {
                selfEnabled = value;

                // disabling
                if (!value)
                {
                    base.Enabled = false;

                    // if just logical status was changed, then invalidating to trigger repaint
                    if (readOnly && style == ComboBoxStyle.DropDownList)
                        Invalidate();
                }
                // real enabling
                else if (!(readOnly && style == ComboBoxStyle.DropDownList))
                {
                    base.Enabled = true; // if parent is disabled, resultant enabled remains false
                    AdjustDrawMode();
                    ResetColor();
                }
                // only logical enabling in ReadOnly and DropDownList mode: repainting in disabled state with enabled color
                else
                    Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner combo box.
        /// </summary>
        public new AutoCompleteMode AutoCompleteMode
        {
            get { return readOnly ? origCompleteMode : base.AutoCompleteMode; }
            set
            {
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
            get { return readOnly ? origCompleteSource : base.AutoCompleteSource; }
            set
            {
                if (readOnly)
                    origCompleteSource = value;
                else
                    base.AutoCompleteSource = value;
            }
        }

        #endregion

        #region New properties

        /// <summary>
        /// ForeColor when control is Enabled.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("ForeColor when control is Enabled.")]
        [DefaultValue(typeof(Color), "WindowText")]
        public Color EnabledForeColor
        {
            get { return enabledForeColor; }
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
            get { return enabledBackColor; }
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
            get { return disabledBackColor; }
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
            get { return disabledForeColor; }
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
            get { return systemDrawDropDownListMode; }
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
            get { return readOnly; }
            set
            {
                if (readOnly != value)
                {
                    // getting the logical enabled state before setting read-only flag
                    bool virtualEnabled = this.Enabled;

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
                    if (virtualEnabled)
                    {
                        if (style == ComboBoxStyle.DropDownList)
                        {
                            virtualEnabledChanging = true;
                            try
                            {
                                bool realEnabled = !readOnly;

                                if (GetSelfEnabled() != realEnabled)
                                    base.Enabled = realEnabled;
                                else
                                    Invalidate();
                            }
                            finally
                            {
                                virtualEnabledChanging = false;
                            }
                        }

                        AdjustDrawMode();
                        ResetColor();
                    }
                    OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedComboBox"/>
        /// </summary>
        public AdvancedComboBox()
        {
            EnabledChanged += new EventHandler(AdvancedComboBox_EnabledChanged);
            SelectedIndexChanged += new EventHandler(AdvancedComboBox_SelectedIndexChanged);
            SystemColorsChanged += new EventHandler(AdvancedComboBox_SystemColorsChanged);
            HandleCreated += new EventHandler(AdvancedComboBox_HandleCreated);
            HandleDestroyed += new EventHandler(AdvancedComboBox_HandleDestroyed);
            ParentChanged += new EventHandler(AdvancedComboBox_ParentChanged);
            Enter += new EventHandler(AdvancedComboBox_Enter);
            Leave += new EventHandler(AdvancedComboBox_Leave);

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

        #region Protected methods

        /// <summary>
        /// Raises the <see cref="ReadOnlyChanged"/> event.
        /// </summary>
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            if (ReadOnlyChanged != null)
                ReadOnlyChanged.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="TextChangedOnLeave"/> event.
        /// </summary>
        protected virtual void OnTextChangedOnLeave(EventArgs e)
        {
            if (TextChangedOnLeave != null)
                TextChangedOnLeave(this, e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // suppressing event if changing is a workaround
            if (textAndFontChanging)
                return;
            base.OnTextChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            // suppressing event if changing is a workaround
            if (textAndFontChanging)
                return;
            base.OnFontChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // suppressing deleting and up/left (selecting item from list) because these cannot be suppressed in KeyPress
            if (readOnly && e.KeyCode.In(Keys.Delete, Keys.Back, Keys.Up, Keys.Down))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (readOnly)
            {
                // allowing only Ctrl+C (Copy) - Ctrl+Insert is not captured here
                e.Handled = e.KeyChar != (char)3; //!e.KeyChar.In((char)3, (char)13, (char)27);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (parent != null)
            {
                parent.EnabledChanged -= parent_EnabledChanged;
                parent = null;
            }

            base.Dispose(disposing);
            if (disposing)
            {
                if (bmpSaved != null)
                    bmpSaved.Dispose();
            }
            bmpSaved = null;
        }

        /// <summary>
        /// Draws an item in the dropdown area and also in the control area in dropdownlist mode.
        /// Works only if DrawMode is OwnerDrawFixed.
        /// </summary>
        [DebuggerStepThrough]
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // drawing an item in the dropdown area
            if (e.Index >= 0)
            {
                string label = GetItemText(Items[e.Index]);
                bool dispose = true;

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
                    brFore = new SolidBrush(ForeColor);
                    brBack = new SolidBrush(BackColor);
                }
                // Focused list item
                else
                {
                    brFore = SystemBrushes.HighlightText;
                    brBack = SystemBrushes.Highlight;
                    dispose = false;
                }
                e.Graphics.FillRectangle(brBack, e.Bounds);
                e.Graphics.DrawString(label, e.Font, brFore, e.Bounds.Left - 2, e.Bounds.Top);
                e.DrawFocusRectangle();
                if (dispose)
                {
                    brFore.Dispose();
                    brBack.Dispose();
                }
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
        [DebuggerStepThrough]
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!base.Enabled)
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

                // saving the disabled image of the control (this saves a dropdownlist appearance)
                if (bmpSaved == null)
                {
                    if (Width <= 0 || Height <= 0)
                        return;

                    SetStyle(ControlStyles.UserPaint, false);
                    bmpSaved = new Bitmap(Width, Height);
                    DrawToBitmap(bmpSaved, ClientRectangle);
                    SetStyle(ControlStyles.UserPaint, true);
                }

                // drawing background
                g.DrawImage(bmpSaved, 0, 0);
                
                // filling with disabled background
                Rectangle rectangle = ClientRectangle;
                rectangle.X += 2;
                rectangle.Y += 2;
                rectangle.Width -= 4;
                rectangle.Height -= 4;
                bool ltr = RightToLeft == RightToLeft.Yes;
                if (style != ComboBoxStyle.Simple)
                {
                    rectangle.Width -= 16; // assuming that dropdown button is 16 px wide
                    if (ltr)
                        rectangle.X += 16;
                }
                using (Brush b = new SolidBrush(disabledBackColor))
                {
                    g.FillRectangle(b, rectangle);
                }

                // drawing text
                rectangle.Width += 6;
                if (style == ComboBoxStyle.DropDownList)
                {
                    rectangle.X -= 1;
                }
                else
                {
                    rectangle.X -= 2;
                    rectangle.Y -= 1;
                }
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;
                if (ltr)
                    flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                TextRenderer.DrawText(g, textSaved, Font, rectangle, this.Enabled ? enabledForeColor : disabledForeColor, flags);
            }
        }

        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            if (styleChanging)
                return;

            base.OnDropDownStyleChanged(e);

            bool virtualEnabled = this.Enabled;
            style = base.DropDownStyle;
            AdjustEnabledByReadOnly(virtualEnabled);

            AdjustDrawMode();
            if (!base.Enabled)
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
            else if (style == ComboBoxStyle.Simple)
                FixSimpleAppearance();

            SetPaintMode();
        }

        [DebuggerStepThrough]
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
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            if (!ReadOnly)
            {
                base.WndProc(ref m);
                return;
            }

            // bug: ComboBox.WndProc does not see WM_PASTE and other messages so they are captured in InnerEditorWindow
            switch (m.Msg)
            {
                // suppressing dropping list down in DropDown mode (in DropDownList mode there is no COMMAND message)
                case Constants.WM_COMMAND:
                    return;
                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        #endregion

        #region Private Methods

        private void SetPaintMode()
        {
            if (DesignMode)
                return;
            bool userPaint = !base.Enabled && (style != ComboBoxStyle.DropDownList || SelectedIndex >= 0);
            SetStyle(ControlStyles.UserPaint, userPaint);
            Invalidate();
        }

        private void AdjustDrawMode()
        {
            bool customDraw = !base.Enabled || style != ComboBoxStyle.DropDownList || !systemDrawDropDownListMode;
            DrawMode drawMode = customDraw ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
            if (base.DrawMode != drawMode)
                base.DrawMode = drawMode;
        }

        private void ResetColor()
        {
            bool backColorChanged = false;
            // BackColor when control is Enabled and not ReadOnly
            if (base.Enabled && !ReadOnly && base.BackColor != enabledBackColor)
            {
                base.BackColor = enabledBackColor;
                backColorChanged = true;
            }
            // BackColor when control is not Enabled or is ReadOnly
            else if ((base.Enabled && ReadOnly || !base.Enabled) && base.BackColor != disabledBackColor)
            {
                base.BackColor = disabledBackColor;
                backColorChanged = true;
            }

            // ForeColor in Enabled state (also ReadOnly)
            if (base.Enabled && base.ForeColor != enabledForeColor)
                base.ForeColor = enabledForeColor;
            // ForeColor in disabled state (ReadOnly state is indifferent)
            else if (!base.Enabled && base.ForeColor != disabledForeColor)
                base.ForeColor = disabledForeColor;

            // workaround: changing back color of a Simple combobox causes to display a few pixels high dropdown listbox with 0 elements
            if (base.Enabled && backColorChanged && style == ComboBoxStyle.Simple)
                FixSimpleAppearance();

            Invalidate();
        }

        private void FixSimpleAppearance()
        {
            Debug.Assert(base.Enabled && style == ComboBoxStyle.Simple, "Appearance fixing is needless");

            styleChanging = true;
            try
            {
                base.DropDownStyle = ComboBoxStyle.DropDown;
                base.DropDownStyle = ComboBoxStyle.Simple;
            }
            finally
            {
                styleChanging = false;
            }
        }

        private void AdjustEnabledByReadOnly(bool virtualEnabled)
        {
            // dropdown list mode
            if (style == ComboBoxStyle.DropDownList)
            {
                virtualEnabledChanging = true;
                try
                {
                    bool newEnabled = !readOnly && virtualEnabled;

                    if (base.Enabled != newEnabled)
                        base.Enabled = newEnabled;
                    else if (!base.Enabled)
                        Invalidate();
                }
                finally
                {
                    virtualEnabledChanging = false;
                }
            }
            // logical and real enabled are different (changiing style from dropdown list to something other in readonly mode)
            else if (virtualEnabled != base.Enabled)
            {
                virtualEnabledChanging = true;
                try
                {
                    base.Enabled = virtualEnabled;
                    ResetColor();
                }
                finally
                {
                    virtualEnabledChanging = false;
                }
            }
            // logical and real enabled are the same (both disabled) but recoloring could be needed
            else
                Invalidate();
        }

        private bool GetSelfEnabled()
        {
            if (stateField == null)
                stateField = FieldAccessor.GetFieldAccessor(typeof(Control).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic));
            return ((int)stateField.Get(this) & 4) != 0;
        }

        #endregion

        #region Handled events
        // ReSharper disable InconsistentNaming

        void AdvancedComboBox_HandleCreated(object sender, EventArgs e)
        {
            // hooking inner text box to capture WM_PASTE and others
            IntPtr lhWnd = User32.FindWindowEx(Handle, IntPtr.Zero, "EDIT", null);
            if (lhWnd != IntPtr.Zero)
            {
                hook = new InnerEditorWindow(this);
                hook.AssignHandle(lhWnd);
            }
        }

        void AdvancedComboBox_HandleDestroyed(object sender, EventArgs e)
        {
            if (hook != null)
            {
                hook.ReleaseHandle();
                hook = null;
            }
        }

        void AdvancedComboBox_EnabledChanged(object sender, EventArgs e)
        {
            // obtaining self enabled state because Enabled can be changed by parent or base.Enabled. Suppressing change in case of virtual change.
            if (!virtualEnabledChanging)
                selfEnabled = GetSelfEnabled();

            // control has been enabled by base.Enabled but physically it should be disabled in read-only DropDownList mode
            if (!virtualEnabledChanging && selfEnabled && readOnly && style == ComboBoxStyle.DropDownList)
            {
                //setting virtualEnabledChanging for next EnabledChange will preserve selfEnabled true
                virtualEnabledChanging = true;
                try
                {
                    base.Enabled = false;
                    return;
                }
                finally
                {
                    virtualEnabledChanging = false;
                }
            }

            styleChanging = true;
            try
            {
                // real enabling (virtual one is handled above)
                if (base.Enabled)
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
                        else if (textSaved != null && style != ComboBoxStyle.DropDownList)
                        {
                            base.Text = textSaved;
                        }
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

        void AdvancedComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (styleChanging)
                return;
            SetPaintMode();
        }

        void AdvancedComboBox_SystemColorsChanged(object sender, EventArgs e)
        {
            if (bmpSaved != null)
            {
                bmpSaved.Dispose();
                bmpSaved = null;
            }
        }

        void AdvancedComboBox_ParentChanged(object sender, EventArgs e)
        {
            if (parent != null)
                parent.EnabledChanged -= parent_EnabledChanged;
            if (!IsDisposed)
            {
                parent = Parent;
                parent.EnabledChanged += new EventHandler(parent_EnabledChanged);
            }
        }

        void parent_EnabledChanged(object sender, EventArgs e)
        {
            // Assuring recoloring also in virtual enabled change:
            // changing Parent.Enabled while control is ReadOnly in DropDownList mode
            Invalidate();
        }

        void AdvancedComboBox_Enter(object sender, EventArgs e)
        {
            textOnFocus = Text;
        }

        void AdvancedComboBox_Leave(object sender, EventArgs e)
        {
            if (textOnFocus != Text)
                OnTextChangedOnLeave(e);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #endregion

        #region IListControl Members

        /// <summary>
        /// Gets whether the there is no selected item in the combo box (<see cref="ComboBox.SelectedValue"/> is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlTools.NotSelectedValue"/>)
        /// </summary>
        public bool IsEmpty
        {
            get { return this.IsEmpty(); }
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, dataTable, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, dataTable, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember)
        {
            ListControlTools.LoadFrom(this, dataTable, valueMember, displayMember);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, enumType, valueMemberType, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, enumType, valueMemberType, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        public void LoadFrom(Type enumType, Type valueMemberType)
        {
            ListControlTools.LoadFrom(this, enumType, valueMemberType);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        public void LoadFrom(Type enumType)
        {
            ListControlTools.LoadFrom(this, enumType);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, collection, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlTools.LoadFrom(this, collection, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember)
        {
            ListControlTools.LoadFrom(this, collection, valueMember, displayMember);
        }

        #endregion
    }
}
