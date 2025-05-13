#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedTextBox.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /*******************************************
     * AdvancedTextBox - TODO: into remarks
     *
     * Problems with original TextBox:
     * - If BackColor is not set, setting ReadOnly makes control gray, but does not turn gray if BackColor is set before
     * - Disabling the control makes the text gray and it is impossible to change it.
     *
     * Solution:
     * - DisabledBackColor: Color in case of ReadOnly or not Enabled
     * - DisabledForeColor: Text color in disabled state
     *
     * Other features:
     * - TextChangeOnLeave event: Fires on leave when content differs from the content at getting focused
     */

    /// <summary>
    /// Advanced version of <see cref="TextBox"/> control that supports customized coloring even in disabled state
    /// and has a <see cref="TextChangedOnLeave"/> event.
    /// </summary>
    public class AdvancedTextBox : TextBox, ISupportsDisabledColor
    {
        #region Fields

        private Color disabledBackColor = SystemColors.Control;
        private Color disabledForeColor = SystemColors.ControlDarkDark;
        private Color enabledBackColor = SystemColors.Window;
        private Color enabledForeColor = SystemColors.WindowText;
        private string origValue = String.Empty; // content at getting focused

        #endregion

        #region Events

        /// <summary>
        /// Occurs on leaving the control when content is different from the original one when the control was focused.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused.")]
        public event EventHandler? TextChangedOnLeave;

        #endregion

        #region Properties

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
        /// ForeColor when control is Enabled.
        /// </summary>
        [Category("AdvancedTextBox")]
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
        [Category("AdvancedTextBox")]
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
        [Category("AdvancedTextBox")]
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
        [Category("AdvancedTextBox")]
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

        #endregion

        #region Constructors

        ///<summary>
        /// Creates a new instance of <see cref="AdvancedTextBox"/>.
        ///</summary>
        public AdvancedTextBox()
        {
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ResetEnabledAndReadOnly();
        }

        /// <inheritdoc/>
        protected override void OnReadOnlyChanged(EventArgs e)
        {
            base.OnReadOnlyChanged(e);
            ResetEnabledAndReadOnly();
        }

        /// <inheritdoc/>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            origValue = Text;
        }

        /// <inheritdoc/>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (origValue != Text)
                OnTextChangedOnLeave(e);
        }

        /// <summary>
        /// Triggers TextChangedOnLeave event
        /// </summary>
        protected virtual void OnTextChangedOnLeave(EventArgs e) => TextChangedOnLeave?.Invoke(this, e);

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Painting with disabled colors
            if (!Enabled)
            {
                //TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;

                //if (!Multiline)
                //    flags |= TextFormatFlags.SingleLine;

                //if (WordWrap)
                //    flags |= TextFormatFlags.WordBreak;

                //switch (TextAlign)
                //{
                //    case HorizontalAlignment.Center:
                //        flags |= TextFormatFlags.HorizontalCenter;
                //        break;
                //    case HorizontalAlignment.Left:
                //        flags |= TextFormatFlags.Left;
                //        break;
                //    case HorizontalAlignment.Right:
                //        flags |= TextFormatFlags.Right;
                //        break;
                //}

                //if (this.IsRightToLeft())
                //    flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;

                using (Brush b = new SolidBrush(disabledBackColor))
                {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }
                // TODO: Adjust rectangle size to DPI (this +5 width is good for 96 DPI but 120 DPI requires +6)
                Rectangle rectangle = new Rectangle(new Point(-2, 1), new Size(ClientRectangle.Width + 5, ClientRectangle.Height - 2));
                TextFormatFlags flags = this.GetFormatFlags();
                if (!UseSystemPasswordChar)
                    TextRenderer.DrawText(e.Graphics, Text.Substring(GetFirstCharIndexFromLine(GetFirstVisibleLine())), Font, rectangle, disabledForeColor, flags);
                else
                    TextRenderer.DrawText(e.Graphics, new string(PasswordChar, Text.Length), Font, rectangle, disabledForeColor, flags);
            }
            else
                base.OnPaint(e);
        }

        /// <summary>
        /// Prevents consuming Enter by the parent form/control if this.<see cref="TextBox.Multiline"/> is enabled.
        /// </summary>
        /// <inheritdoc/>
        protected override bool IsInputKey(Keys keyData)
        {
            return (((((keyData & Keys.KeyCode) == Keys.Return) && this.Multiline) && ((keyData & Keys.Alt) == Keys.None)) || base.IsInputKey(keyData));
        }

        #endregion

        #region Private Methods

        private void ResetEnabledAndReadOnly()
        {
            SetStyle(ControlStyles.UserPaint, !Enabled);
            if (Enabled)
            {
                // without these font text may change to weird style when control is re-enabled.
                Font font = Font;
                Font = null!;
                Font = font;
            }

            ResetColor();
        }

        private int GetFirstVisibleLine()
        {
            return User32.SendMessage(Handle, Constants.EM_GETFIRSTVISIBLELINE, 0, 0).ToInt32();
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
    }
}
