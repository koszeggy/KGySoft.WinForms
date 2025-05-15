using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace KGySoft.WinForms.Test.Forms
{
    internal partial class ControlsTestBaseForm : Form
    {
        [DefaultValue("Click the items to see their properties")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string InstructionsText
        {
            get => lblInstuction.Text;
            set => lblInstuction.Text = value;
        }

        public ControlsTestBaseForm()
        {
            InitializeComponent();
        }

        private void ControlsTestBaseForm_Load(object sender, EventArgs e)
        {
            Subscribe(this, true);
        }

        private void Subscribe(Control parentControl, bool add)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (control == grdProperties)
                    continue;

                if (add)
                    control.Click += new EventHandler(control_Click);
                else
                    control.Click -= control_Click;

                if (control.HasChildren)
                    Subscribe(control, add);
            }
        }

        void control_Click(object sender, EventArgs e)
        {
            grdProperties.SelectedObject = sender;
        }

        private void ControlsTestBaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Subscribe(pnlTestArea, false);
        }

        private void miResetValue_Click(object sender, EventArgs e)
        {
            var selectedObject = grdProperties.SelectedObject;
            if (selectedObject == null)
                return;

            PropertyDescriptor descriptor = grdProperties.SelectedGridItem?.PropertyDescriptor;
            if (descriptor == null)
                return;

            if (descriptor.CanResetValue(selectedObject) == true)
            {
                descriptor.ResetValue(selectedObject);
                return;
            }

            if (descriptor.IsReadOnly)
                return;

            // If the property is not resettable (e.g. Image), we set it to its default value
            var defaultValue = descriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault() is DefaultValueAttribute d ? d.Value
                : descriptor.PropertyType.IsValueType ? Activator.CreateInstance(descriptor.PropertyType)
                : null;

            descriptor.SetValue(selectedObject, defaultValue);

            // Select the property again to refresh the grid
            grdProperties.SelectedGridItem.Select();
        }
    }
}
