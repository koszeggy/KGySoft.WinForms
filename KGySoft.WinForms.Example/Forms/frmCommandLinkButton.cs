using KGySoft.Drawing;

namespace KGySoft.WinForms.Example.Forms
{

    internal partial class frmCommandLinkButton : ControlsTestBaseForm
    {
        public frmCommandLinkButton()
        {
            InitializeComponent();
            gbCustomBackground.BackgroundImage = Icons.Shield.ToBitmap();
        }
    }
}
