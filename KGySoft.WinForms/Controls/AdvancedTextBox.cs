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
     * - If Multiline is true and the control is not ReadOnly, not allowing processing Enter by the parent form/control
     */

    /// <summary>
    /// Advanced version of <see cref="TextBox"/> control that supports customized coloring even in disabled state
    /// and has a <see cref="TextChangedOnLeave"/> event.
    /// </summary>
    public class AdvancedTextBox : TextBox, ISupportsDisabledColor
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledOrReadOnlyBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;
        private static readonly Color defaultReadOnlyForeColor = SystemColors.ControlText;

        #endregion

        #region Instance Fields

        // NOTE: Unlike in ButtonBase descendants, we always set the base enabled back (and fore) colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;
        private string origValue = String.Empty; // content at getting focused

        #endregion

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
        /// Gets or sets the background color of the control in the current state.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
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
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/> and not <see cref="TextBox.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the background color when the control is Enabled and not ReadOnly.")]
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
        [Category("AdvancedTextBox")]
        [Description("Determines the text color when the control is Enabled.")]
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
        /// Gets or sets the background color when the control is not <see cref="Control.Enabled"/> or is <see cref="TextBox.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Determines the background when the control is not Enabled or is ReadOnly.")]
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
        [Category("AdvancedTextBox")]
        [Description("Determines the text color when the control is not Enabled.")]
        public Color DisabledForeColor
        {
            get => !disabledForeColor.IsEmpty ? disabledForeColor : defaultDisabledForeColor;
            set
            {
                if (disabledForeColor == value)
                    return;
                disabledForeColor = value;
                ResetColors();
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
                e.Graphics.FillRectangle(BackColor.GetBrush(), ClientRectangle);

                // TODO: Adjust rectangle size to DPI (this +5 width is good for 96 DPI but 120 DPI requires +6)
                Rectangle rectangle = new Rectangle(new Point(-2, 1), new Size(ClientRectangle.Width + 5, ClientRectangle.Height - 2));
                TextFormatFlags flags = this.GetFormatFlags();
                if (!UseSystemPasswordChar)
                    TextRenderer.DrawText(e.Graphics, Text.Substring(GetFirstCharIndexFromLine(GetFirstVisibleLine())), Font, rectangle, ForeColor, flags);
                else
                    TextRenderer.DrawText(e.Graphics, new string(PasswordChar, Text.Length), Font, rectangle, ForeColor, flags);
            }
            else
                base.OnPaint(e);
        }

        protected override bool IsInputKey(Keys keyData)
            => ((keyData & Keys.KeyCode) == Keys.Return && !ReadOnly && Multiline && (keyData & Keys.Alt) == Keys.None) || base.IsInputKey(keyData);

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

            ResetColors();
        }

        private int GetFirstVisibleLine() => User32.SendMessage(Handle, Constants.EM_GETFIRSTVISIBLELINE, 0, 0).ToInt32();

        private void ResetColors()
        {
            bool enabled = Enabled;
            bool readOnly = ReadOnly;
            Color baseBackColor = base.BackColor;
            Color baseForeColor = base.ForeColor;

            if (enabled && !readOnly && EnabledBackColor is Color enabledBgColor && enabledBgColor != baseBackColor)
                base.BackColor = enabledBgColor;
            else if ((readOnly || !enabled) && DisabledBackColor is Color disabledBgColor && disabledBgColor != baseBackColor)
                base.BackColor = disabledBgColor;

            if (enabled && EnabledForeColor is Color enabledFgColor && enabledFgColor != baseForeColor)
                base.ForeColor = enabledFgColor;
            else if (!enabled && DisabledForeColor is Color disabledFgColor && disabledFgColor != baseForeColor)
                base.ForeColor = disabledFgColor;
        }

        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        #endregion

        #endregion
    }
}
