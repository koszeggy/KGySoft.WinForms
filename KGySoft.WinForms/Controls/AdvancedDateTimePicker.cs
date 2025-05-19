#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedDateTimePicker.cs
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
    /// Represents a date-time picker that supports coloring in disabled state.
    /// Its <see cref="Value"/> property is redefined so it returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.Checked"/> is <see langword="false"/> and
    /// instead of throwing exception when invalid date is assigned to it, it simpy changes <see cref="DateTimePicker.Checked"/> false (if checkbox is visible), or just ignores the value.
    /// </summary>
    [Description("A date-time picker with custom colors (no custom enabled fore color so far though), and improved Value")]
    public class AdvancedDateTimePicker : DateTimePicker, ISupportsDisabledColor
    {
        #region Fields

        #region Static Fields

        private static readonly Color defaultEnabledBackColor = SystemColors.Window;
        private static readonly Color defaultEnabledForeColor = SystemColors.WindowText;
        private static readonly Color defaultDisabledBackColor = SystemColors.Control;
        private static readonly Color defaultDisabledForeColor = SystemColors.GrayText;

        #endregion

        #region Instance Fields

        // NOTE: Similar to AdvancedTextBox, we always set the base back (and fore) colors (see ResetColors) because we don't have a reimplemented adapter here,
        // so the base drawing routines still rely on them. Setting them even with default colors is not a problem because this control never inherits colors from the parent control.
        // The control doesn't use the fore color in enabled state at all, even with disabled visual styles, and I don't even plan to implement it.
        private Color enabledBackColor;
        private Color enabledForeColor;
        private Color disabledBackColor;
        private Color disabledForeColor;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        /// <value>Returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.ShowCheckBox"/> is <see langword="true"/> and <see cref="DateTimePicker.Checked"/> is false.</value>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new DateTime Value
        {
            get
            {
                if (ShowCheckBox && !Checked)
                    return DateTime.MaxValue;
                else
                    return base.Value;
            }
            set
            {
                // ignoring invalid value (eg. when control is data bound, DateTime.MinValue may come)
                if (value < MinDate || value > MaxDate)
                {
                    if (ShowCheckBox)
                        Checked = false;
                }
                else
                    base.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the control in the current <see cref="Control.Enabled"/> and <see cref="DateTimePicker.Checked"/> state.
        /// </summary>
        [Browsable(true)]
        [Description("The background color in the current Enabled/Checked state. This property always sets EnabledBackColor or DisabledBackColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (Enabled && (!ShowCheckBox || Checked))
                    EnabledBackColor = value;
                else
                    DisabledBackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control in the current <see cref="Control.Enabled"/> and <see cref="DateTimePicker.Checked"/> state.
        /// </summary>
        [Browsable(true)]
        [Description("The text color in the current Enabled/Checked state. This property always sets EnabledForeColor or DisabledForeColor.\r\n\r\n"
            + "Please note that in the WinForms designer a control never actually turns disabled.")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (Enabled && (!ShowCheckBox || Checked))
                    EnabledForeColor = value;
                else
                    DisabledForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color when the control is <see cref="Control.Enabled"/> and not <see cref="TextBox.ReadOnly"/>.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
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
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the text color when the control is Enabled.")]
        public Color EnabledForeColor
        {
            get => !enabledForeColor.IsEmpty ? enabledForeColor : defaultEnabledForeColor;
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
        [Category("AdvancedDateTimePicker")]
        [Description("Determines the background when the control is not Enabled or is ReadOnly.")]
        public Color DisabledBackColor
        {
            get => !disabledBackColor.IsEmpty ? disabledBackColor : defaultDisabledBackColor;
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
        [Category("AdvancedDateTimePicker")]
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

        /// <summary>
        /// Creates a new <see cref="AdvancedDateTimePicker"/> instance.
        /// </summary>
        public AdvancedDateTimePicker()
        {
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_ERASEBKGND: // matters only when visual styles are not enabled; otherwise, the background is redrawn by the system anyway
                    using (Graphics g = Graphics.FromHdc(m.WParam))
                        g.FillRectangle(BackColor.GetBrush(), ClientRectangle);
                    return;

                case Constants.WM_PAINT when !Focused:
                    // Needed because there is no [On]CheckedChanged.
                    // It's important that it's before the base.WndProc call, so there will not be extra paint if color changes cause invalidation.
                    ResetColors();
                    base.WndProc(ref m);
                    Rectangle rect = ClientRectangle;

                    // with visual styles the border belongs to the client area
                    if (VisualStyleHelper.RenderWithVisualStyles)
                        rect.Inflate(-2, -2);

                    int paddingLeft = ShowCheckBox ? 17 : 0;
                    int paddingRight = ShowUpDown ? 18 : 33;
                    bool rtl = RightToLeftLayout && RightToLeft == RightToLeft.Yes;
                    if (rtl)
                        (paddingLeft, paddingRight) = (paddingRight, paddingLeft);
                    rect.X += paddingLeft;
                    rect.Width -= paddingLeft + paddingRight;
                    using (Graphics g = Graphics.FromHwnd(Handle))
                    {
                        // Strange behavior: if the control is RTL, VisibleClipBounds.X is -1 so the calculated rect is off by one pixel.
                        // Fixing it in a compatible way.
                        if (g.VisibleClipBounds.X < 0)
                            g.TranslateTransform(g.VisibleClipBounds.X, g.VisibleClipBounds.Y);
                        var flags =  this.GetFormatFlags();
                        g.FillRectangle(BackColor.GetBrush(), rect);

                        // Even stranger TextRenderer behavior: Somehow it recognizes the RTL layout (is it in the native DC somewhere?)
                        // so we have to undo the translation that we made for filling the background.
                        // This behavior is different from every other custom rendering that we use with TextRenderer and GetFormatFlags
                        if (rtl)
                            rect.X -= paddingLeft - paddingRight;
                        TextRenderer.DrawText(g, Text, Font, rect, ForeColor, BackColor, flags & ~TextFormatFlags.Right);

                        // Note that if we use g.DrawString instead, it needs the original flags and the original rectangle.
                        //g.DrawString(Text, Font, ForeColor.GetBrush(), rect, flags.ToStringFormat());
                    }

                    return;
            }

            base.WndProc(ref m);
        }

        /// <inheritdoc />
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (!VisualStyleHelper.RenderWithVisualStyles)
                Invalidate();
        }

        #endregion

        #region Private Methods

        private void ResetColors()
        {
            bool enabled = Enabled && (!ShowCheckBox || Checked);
            Color baseBackColor = base.BackColor;
            Color baseForeColor = base.ForeColor;

            if (enabled && EnabledBackColor is Color enabledBgColor && enabledBgColor != baseBackColor)
                base.BackColor = enabledBgColor;
            else if (!enabled && DisabledBackColor is Color disabledBgColor && disabledBgColor != baseBackColor)
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
