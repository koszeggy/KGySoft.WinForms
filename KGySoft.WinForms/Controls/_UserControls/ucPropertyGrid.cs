#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucPropertyGrid.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Libraries.Language;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A property grid control that translates description automatically.
    /// </summary>
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucPropertyGrid: ucBase
    {
        private bool showDescription = true;
        private bool readOnly = false;
        private bool allowPropertyRecursion = true;

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

        ///<summary>
        /// Gets or sets whether properties can be edited recursively.
        ///</summary>
        [DefaultValue(true)]
        [Description("Gets or sets whether properties can be edited recursively.")]
        [Category("ucPropertyGrid")]
        public bool AllowPropertyRecursion
        {
            get { return allowPropertyRecursion; }
            set
            {
                allowPropertyRecursion = value;
                SelectedObjects = SelectedObjects;
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
            get { return Unwrap(propertyGrid.SelectedObject); }
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
            if (values == null || !TranslationEnabled && !AllowPropertyRecursion)
            {
                propertyGrid.SelectedObjects = values;
                return;
            }

            propertyGrid.SelectedObjects = values.Select(Wrap).ToArray();
        }

        private object[] GetSelectedObjects()
        {
            object[] result = propertyGrid.SelectedObjects;
            if (result == null)
                return result;
            for (int i = 0; i < result.Length; i++)
                result[i] = Unwrap(result[i]);
            return result;
        }

        private object Wrap(object o)
        {
            if (AllowPropertyRecursion && !(o is RecursivelyEditableTypeDescriptor))
                o = new RecursivelyEditableTypeDescriptor(o);
            if (TranslationEnabled)
                o = new LocalizedObjectDescriptor(o);
            return o;
        }

        private static object Unwrap(object obj) =>
            obj is LocalizedObjectDescriptor localizedObjectDescriptor ? Unwrap(localizedObjectDescriptor.Object)
            : obj is RecursivelyEditableTypeDescriptor recursivelyEditableTypeDescriptor ? Unwrap(recursivelyEditableTypeDescriptor.Object)
            : obj;

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
