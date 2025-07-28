using System;
using System.Drawing;
using System.Windows.Forms;
using KGySoft.WinForms.Forms;

namespace KGySoft.WinForms.Example
{
    public partial class Form1 : BaseForm
    {

        public Form1()
        {
            InitializeComponent();
            AutoScaleFont = Program.AutoScaleFont;
            AutoScaleMode = Program.AutoScaleMode;
            StartPosition = Program.StartPosition;
            Font = ScaleHelper.DefaultFont; // so Form/BaseForm results should be the same also on Framework
        }

        private void advancedCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            advancedButton1.Parent = advancedCheckBox1.Checked ? checkGroupBox1 : checkGroupBox2;
            button1.Parent = advancedCheckBox1.Checked ? checkGroupBox1 : checkGroupBox2;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dtbChangingW.Value = Width;
            dtbChangingH.Value = Height;
            dtbChangedW.Value = Width;
            dtbChangedH.Value = Height;
            dtbResizedW.Value = Width;
            dtbResizedH.Value = Height;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            base.OnDoubleClick(e);
            Font = new Font(Font, FontStyle.Bold);
        }

        protected override void OnDeviceScaleGetNewSize(DeviceScaleGetNewSizeEventArgs e)
        {
            base.OnDeviceScaleGetNewSize(e);
            if (cgbChanging.Checked)
            {
                e.DesiredSize = new Size((int)dtbChangingW.Value, (int)dtbChangingH.Value);
                e.Handled = true;
            }
        }

        protected override void OnDeviceScaleChanged(DeviceScaleChangeEventArgs e)
        {
            base.OnDeviceScaleChanged(e);
            if (cgbChanged.Checked)
            {
                var screen = Screen.FromRectangle(e.SuggestedBounds);
                var newSize = new Size((int)dtbChangedW.Value, (int)dtbChangedH.Value);
                Bounds = new Rectangle(e.SuggestedBounds.Location, newSize).EnsureScreen(screen, false);
            }
        }

        protected override void OnDeviceScaleAutoResized(EventArgs e)
        {
            base.OnDeviceScaleAutoResized(e);
            if (cgbAutoResized.Checked)
            {
                var screen = Screen.FromControl(this);
                var newSize = new Size((int)dtbResizedW.Value, (int)dtbResizedH.Value);
                Bounds = new Rectangle(Location, newSize).EnsureScreen(screen, false);
            }
        }
    }
}
