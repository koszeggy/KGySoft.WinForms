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
    /// <summary>
    /// Advanced version of <see cref="TextBox"/> control that provides some advanced features and fixes for the original <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdvancedTextBox"/> control offers the following features in addition to <see cref="TextBox"/>:
    /// <list type="bullet">
    /// <item>Adjustable colors in disabled state (see <see cref="DisabledBackColor"/> and <see cref="DisabledForeColor"/> properties).</item>
    /// <item><see cref="TextBoxBase.AcceptsTab"/> and <see cref="TextBox.AcceptsReturn"/> are ignored in <see cref="TextBoxBase.ReadOnly"/> mode.</item>
    /// <item><see cref="TextChangedOnLeave"/> event: occurs when leaving the control and <see cref="TextBox.Text"/> is different from the value when the control received focus.</item>
    /// <item>Ctrl+A (Select All) works even if auto appending is enabled.</item>
    /// </list>
    /// </remarks>
    [Description(@"A text box that provides the following features in addition to regular TextBox:
- Adjustable colors in disabled state
- AcceptsTab and AcceptsReturn are ignored in ReadOnly mode
- TextChangedOnLeave event
- Ctrl+A works even if auto appending is enabled")]
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

        // NOTE: Unlike in ButtonBase descendants, we always set the base back and fore colors (see ResetColors) because we don't have a reimplemented adapter here,
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
        /// It fires after the <see cref="Control.Leave"/> and before the <see cref="Control.Validating"/> event.
        /// </summary>
        [Category("AdvancedTextBox")]
        [Description("Occurs on leaving the control when content is different from the original one when the control was focused. "
            + "It fires after the Leave and before the Validating event.")]
        public event EventHandler? TextChangedOnLeave;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> and <see cref="TextBox.ReadOnly"/> state.
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
            if (Enabled)
            {
                base.OnPaint(e);
                return;
            }

            // Painting with disabled colors
            var clientRect = ClientRectangle;
            e.Graphics.FillRectangle(BackColor.GetBrush(), clientRect);

            // TODO: Adjust rectangle size to DPI (this +5 width is good for 96 DPI but 120 DPI requires +6)
            Rectangle textRect = Multiline
                ? new Rectangle(clientRect.X + 1, clientRect.Y + 1, clientRect.Width - 1, clientRect.Height - 1)
                : new Rectangle(new Point(-2, 1), new Size(clientRect.Width + 5, clientRect.Height - 2));
            TextFormatFlags flags = this.GetFormatFlags();
            if (!UseSystemPasswordChar)
                TextRenderer.DrawText(e.Graphics, Text.Substring(GetFirstCharIndexFromLine(GetFirstVisibleLine())), Font, textRect, ForeColor, flags);
            else
                TextRenderer.DrawText(e.Graphics, new string(PasswordChar, Text.Length), Font, textRect, ForeColor, flags);
        }

        /// <inheritdoc/>
        protected override bool IsInputKey(Keys keyData)
        {
            if (Multiline && ReadOnly)
            {
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Return when (keyData & Keys.Alt) == 0:
                        return false;
                    case Keys.Tab when (keyData & Keys.Control) == 0:
                        return false;
                }
            }

            return base.IsInputKey(keyData);
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.A when ShortcutsEnabled:
                    SelectAll();
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion

        #region Private Methods

        private void ResetEnabledAndReadOnly()
        {
            SetStyle(ControlStyles.UserPaint, !Enabled);
            if (Enabled)
            {
                //// without these font text may change to weird style when control is re-enabled.
                //Font font = Font;
                //Font = null!;
                //Font = font;
            }

            ResetColors();
        }

        private int GetFirstVisibleLine() => User32.SendMessage(Handle, Constants.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32();

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

        private bool ShouldSerializeBackColor() => false;
        private bool ShouldSerializeForeColor() => false;
        private bool ShouldSerializeEnabledBackColor() => !enabledBackColor.IsEmpty;
        private bool ShouldSerializeEnabledForeColor() => !enabledForeColor.IsEmpty;
        private bool ShouldSerializeDisabledBackColor() => !disabledBackColor.IsEmpty;
        private bool ShouldSerializeDisabledForeColor() => !disabledForeColor.IsEmpty;

        #endregion

        #endregion
    }
}
