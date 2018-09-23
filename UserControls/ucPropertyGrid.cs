using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KGySoft.Libraries.Language;

namespace KGySoft.Controls
{
    /// <summary>
    /// A property grid control that translates description automatically.
    /// </summary>
    [ToolboxItem(true)]
    public partial class ucPropertyGrid: ucBase
    {
        private bool showDescription = true;
        private bool readOnly = false;

        /// <summary>
        /// Creates a new instance of <see cref="ucPropertyGrid"/>
        /// </summary>
        public ucPropertyGrid()
        {
            InitializeComponent();
            txtDescription.Label.Font = new Font(txtDescription.Label.Font, FontStyle.Bold);
            Language.MarkLocalizable(false, propertyGrid);
        }

        ///<summary>
        /// Gets or sets whether description is shown.
        ///</summary>
        [DefaultValue(true)]
        [Description("Gets or sets whether description is shown.")]
        [Category("ucPropertyGrid")]
        public bool ShowDescription
        {
            get { return showDescription; }
            set
            {
                showDescription = value;
                splitter.Visible = showDescription;
                txtDescription.Visible = showDescription;
            }
        }

        /// <summary>
        /// Gets the inner property grid
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PropertyGrid PropertyGrid
        {
            get { return propertyGrid; }
        }

        /// <summary>
        /// Gets or sets the object for which the grid displays properties.
        /// </summary>
        [Description("Gets or sets the object for which the grid displays properties.")]
        [Category("ucPropertyGrid")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object SelectedObject
        {
            get
            {
                LocalizedObjectDescriptor descriptor = propertyGrid.SelectedObject as LocalizedObjectDescriptor;
                if (descriptor != null)
                    return descriptor.Object;
                return propertyGrid.SelectedObject;
            }
            set
            {
                if (value == null)
                    propertyGrid.SelectedObject = null;
                else
                    SetSelectedObjects(new object[] { value });
            }
        }

        /// <summary>
        /// Gets or sets the currently selected objects.
        /// </summary>
        [Description("Gets or sets the currently selected objects.")]
        [Category("ucPropertyGrid")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object[] SelectedObjects
        {
            get { return GetSelectedObjects(); }
            set { SetSelectedObjects(value); }
        }

        protected override Control MainControl
        {
            get { return propertyGrid; }
        }

        public override object ControlValue
        {
            get { return SelectedObject; }
            set { SelectedObject = value; }
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the property editor.
        /// </summary>
        [DefaultValue(false)]
        [Category("ucPropertyGrid")]
        [Description("Gets or sets the ReadOnly state of the property editor.")]
        public override bool ReadOnly
        {
            get { return readOnly; }
            set
            {
                readOnly = value;
                MainControl.Enabled = !value;
                base.ReadOnly = value;
            }
        }

        public override void Clear()
        {
            SelectedObject = null;
            base.Clear();
        }

        protected override void TranslateContent(ref bool translationFinished)
        {
            // preventing translation of inner content
            translationFinished = true;
        }

        private void SetSelectedObjects(object[] values)
        {
            if (TranslationEnabled && values != null && values.Length != 0)
            {
                LocalizedObjectDescriptor[] localizedObjectDescriptors = new LocalizedObjectDescriptor[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    localizedObjectDescriptors[i] = values[i] == null ? null : new LocalizedObjectDescriptor(values[i]);
                }
                propertyGrid.SelectedObjects = localizedObjectDescriptors;
            }
            else
                propertyGrid.SelectedObjects = values;
        }

        private object[] GetSelectedObjects()
        {
            object[] result = propertyGrid.SelectedObjects;
            if (result == null)
                return result;
            for (int i = 0; i < result.Length; i++)
            {
                LocalizedObjectDescriptor descriptor = result[i] as LocalizedObjectDescriptor;
                if (descriptor != null)
                    result[i] = descriptor.Object;
            }
            return result;
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (propertyGrid.SelectedGridItem.PropertyDescriptor == null)
            {
                txtDescription.Caption = propertyGrid.SelectedGridItem.Label;
                txtDescription.Text = null;
            }
            else
            {
                txtDescription.Caption = propertyGrid.SelectedGridItem.PropertyDescriptor.DisplayName;
                txtDescription.Text = propertyGrid.SelectedGridItem.PropertyDescriptor.Description;
            }
        }
    }
}
