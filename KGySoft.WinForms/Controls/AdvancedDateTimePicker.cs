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

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a date-time picker that supports coloring in disabled state.
    /// Its <see cref="Value"/> property is redefined so it returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.Checked"/> is <see langword="false"/>&#160;and
    /// instead of throwing exception when invalid date is assigned to it, it simpy changes <see cref="DateTimePicker.Checked"/> false (if checkbox is visible), or just ignores the value.
    /// </summary>
    [Description("A date-time picker with custom colors (no custom enabled fore color so far though), and improved Value")]
    public class AdvancedDateTimePicker : DateTimePicker, ISupportsDisabledColor
    {
        #region Fields

        private Color disabledBackColor = SystemColors.Control;
        private Color disabledForeColor = SystemColors.ControlDarkDark;
        Color backColor = SystemColors.Window;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        /// <value>Returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.ShowCheckBox"/> is <see langword="true"/>&#160;and <see cref="DateTimePicker.Checked"/> is false.</value>
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
                // ignoring invalid value (eg. when control is databound, DateTime.MinValue may come)
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
        /// Gets or sets disabled fore color.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Gets or sets disabled fore color.")]
        [DefaultValue(typeof(Color), "ControlDarkDark")]
        public Color DisabledForeColor
        {
            get => disabledForeColor;
            set
            {
                disabledForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets disabled back color.
        /// </summary>
        [Category("AdvancedDateTimePicker")]
        [Description("Gets or sets disabled back color.")]
        [DefaultValue(typeof(Color), "Control")]
        public Color DisabledBackColor
        {
            get => disabledBackColor;
            set
            {
                disabledBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the background color of the control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor // hidden in the base DateTimePicker
        {
            get => backColor;
            set
            {
                backColor = value;
                //base.BackColor = value;
                Invalidate();
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            SetStyle(ControlStyles.UserPaint, !Enabled);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Brush brFore;
            Brush brBack;

            if (Enabled)
            {
                brFore = ForeColor.GetBrush();
                brBack = backColor.GetBrush();
            }
            else
            {
                brFore = disabledForeColor.GetBrush();
                brBack = disabledBackColor.GetBrush();
            }
            e.Graphics.FillRectangle(brBack, ClientRectangle);
            if (Checked)
            {
                e.Graphics.DrawString(Text, Font, brFore, 0, 0);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // That's how we can fill the enabled control with BackColor
            if (m.Msg == 0x14 && Enabled) // WM_ERASEBKGND
            {
                using Graphics g = Graphics.FromHdc(m.WParam);
                g.FillRectangle(backColor.GetBrush(), ClientRectangle);
                return;

            }
            base.WndProc(ref m);
        }

        #endregion

        #endregion
    }
}
