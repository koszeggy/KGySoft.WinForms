using KGySoft.Drawing;

namespace KGySoft.WinForms.Example.Forms
{

    internal partial class frmCommandLinkButton : ControlsTestBaseForm
    {
        public frmCommandLinkButton()
        {
            InitializeComponent();
            this.gbCustomBackground.BackgroundImage = Icons.Shield.ToBitmap();
        }
    }
}
