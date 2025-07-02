using System.Drawing;
using System.Windows.Forms;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Example.Controls
{
    public partial class LocalizableControlDemo : BaseUserControl
    {
        public LocalizableControlDemo()
        {
            InitializeComponent();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (Dock != DockStyle.Top)
                return base.GetPreferredSize(proposedSize);
            var proposedAutoHeight = new Size(proposedSize.Width, 0);
            return new Size(proposedSize.Width, Padding.Vertical
                + lblLocalizableControlCaption.GetPreferredSize(proposedAutoHeight).Height
                + btnLocalizableControl.GetPreferredSize(proposedAutoHeight).Height
                + (Height - ClientSize.Height));
        }
    }
}
