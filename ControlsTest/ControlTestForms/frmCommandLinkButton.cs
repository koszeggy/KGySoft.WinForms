using System.Drawing;
using KGySoft.Controls;
using KGySoft.Drawing;

namespace ControlsTest
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
