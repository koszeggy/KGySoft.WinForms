/*******************************************
 * AdvancedDateTimePicker - KGy
 * 
 * Eredend? problémák:
 * - A felirat színe Enabled = false-ra mindenképpen szürkül
 * - Egyáltalán nincs háttérszín állítási lehet?ség
 * - Nincs el?térszín állítási lehet?ség (ezt nem is akarom megcsinálni, majd inkább egy rendes dátumválasztót írunk MaskEdit-b?l)
 * 
 * Megoldás:
 * - DisabledBackColor: Háttérszín letiltott állapotban
 * - DisabledForeColor: Felirat színe letiltott állapotban
 * - BackColor: Háttérszín engedélyezett állapotban
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;

namespace KGySoft.Controls
{
    // TODO: valamiért a BackColor-ra nem reagál, hiába lesz Gold modified állapotban (legalábbis Win7 alatt) - custom kirajzolás?
    // TODO: colorok mint gombnál
    /// <summary>
    /// Represents a date-time picker that supports coloring in disabled state.
    /// Its <see cref="Value"/> property is redefined so it returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.Checked"/> is <see langword="false"/> and
    /// instead of throwing exception when invalid date is assigned to it, it simpy changes <see cref="DateTimePicker.Checked"/> false (if checkbox is visible), or just ignores the value.
    /// </summary>
    public class AdvancedDateTimePicker : DateTimePicker, IDisabledColorCapable
    {
        #region Objektumváltozók

        private Color disabledBackColor = SystemColors.Control;
        private Color disabledForeColor = SystemColors.ControlDarkDark;
        Color backColor = SystemColors.Window;

        #endregion

        #region Konstruktorok

        /// <summary>
        /// Creates a new <see cref="AdvancedDateTimePicker"/> instance.
        /// </summary>
        public AdvancedDateTimePicker()
        {
            EnabledChanged += new EventHandler(AdvancedDateTimePicker_EnabledChanged);
        }

        #endregion

        #region Lekezelt események

        void AdvancedDateTimePicker_EnabledChanged(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.UserPaint, !Enabled);
            Invalidate();
        }

        #endregion

        #region Property-k

        /// <summary>
        /// Gets or sets the date/time value assigned to the control.
        /// </summary>
        /// <value>Returns <see cref="DateTime.MaxValue"/> if <see cref="DateTimePicker.ShowCheckBox"/> is <see langword="true"/> and <see cref="DateTimePicker.Checked"/> is false.</value>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // ne mentse a mai dátumot mindig a designerbe
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
            get { return disabledForeColor; }
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
            get { return disabledBackColor; }
            set
            {
                disabledBackColor = value;
                Invalidate();
            }
        }

        #endregion

        #region Override-olt property-k, metódusok

        /// <summary>
        /// Gets or sets a value indicating the background color of the control.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor // ez a property benne volna az ?sben, csak a DateTimePickerb?l ki van szedve
        {
            get { return backColor; }
            set
            {
                backColor = value;
                //base.BackColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Brush brFore;
            Brush brBack;

            if (Enabled)
            {
                brFore = new SolidBrush(ForeColor);
                brBack = new SolidBrush(backColor);
            }
            else
            {
                brFore = new SolidBrush(disabledForeColor);
                brBack = new SolidBrush(disabledBackColor);
            }
            e.Graphics.FillRectangle(brBack, ClientRectangle);
            if (Checked)
            {
                e.Graphics.DrawString(Text, Font, brFore, 0, 0);
            }
        }

        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            // Így tudjuk a BackColor-ral kiszínezni az engedélyezett controlt
            if (m.Msg == 0x14 && Enabled) // WM_ERASEBKGND
            {
                using (Graphics g = Graphics.FromHdc(m.WParam))
                {
                    g.FillRectangle(new SolidBrush(backColor), ClientRectangle);
                }
                return;

            }
            base.WndProc(ref m);
        }

        #endregion
    }
}
