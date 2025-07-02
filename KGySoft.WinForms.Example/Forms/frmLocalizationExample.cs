using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: NeutralResourcesLanguage("en")]

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmLocalizationExample : ControlsTestBaseForm
    {
        public frmLocalizationExample()
        {
            InitializeComponent();
        }

        private void localizableControlDemo_DynamicStringLocalizationChanged(object sender, EventArgs e)
        {
            ApplyStringResources();
        }
    }
}
