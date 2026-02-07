#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucPropertyGrid.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucPropertyGrid: ucBase
    {
        #region Fields

        private bool showDescription = true;
        private bool readOnly;
        private bool allowPropertyRecursion = true;

        #endregion

        #region Properties

        #region Public Properties

        ///<summary>
        /// Gets or sets whether description is shown.
        ///</summary>
        [DefaultValue(true)]
        [Description("Gets or sets whether description is shown.")]
        [Category("ucPropertyGrid")]
        public bool ShowDescription
        {
            get => showDescription;
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
            get => allowPropertyRecursion;
            set
            {
                allowPropertyRecursion = value;
                SetSelectedObjects(GetSelectedObjects());
            }
        }

        /// <summary>
        /// Gets the inner property grid
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PropertyGrid PropertyGrid => propertyGrid;

        /// <summary>
        /// Gets or sets the object for which the grid displays properties.
        /// </summary>
        [Description("Gets or sets the object for which the grid displays properties.")]
        [Category("ucPropertyGrid")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object? SelectedObject
        {
            get => Unwrap(propertyGrid.SelectedObject);
            set
            {
                if (value == null)
                    propertyGrid.SelectedObject = null;
                else
                    SetSelectedObjects([value]);
            }
        }

        /// <summary>
        /// Gets or sets the currently selected objects.
        /// </summary>
        [Description("Gets or sets the currently selected objects.")]
        [Category("ucPropertyGrid")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Array, just like PropertyGrid.SelectedObjects")]
        public object[] SelectedObjects
        {
            get => GetSelectedObjects();
            set => SetSelectedObjects(value);
        }

        /// <summary>
        /// Gets or sets the selected object if the property grid.
        /// </summary>
        public override object? ControlValue
        {
            get => SelectedObject;
            set => SelectedObject = value;
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the property editor.
        /// </summary>
        [DefaultValue(false)]
        [Category("ucPropertyGrid")]
        [Description("Gets or sets the ReadOnly state of the property editor.")]
        public override bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;
                MainControl.Enabled = !value;
                base.ReadOnly = value;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the wrapped <see cref="PropertyGrid"/> control.
        /// </summary>
        protected override Control MainControl => propertyGrid;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="ucPropertyGrid"/>
        /// </summary>
        public ucPropertyGrid()
        {
            InitializeComponent();
            txtDescription.Label.Font = new Font(txtDescription.Label.Font, FontStyle.Bold);
            Language.MarkLocalizable(false, propertyGrid);
        }

        #endregion

        #region Methods

        #region Static Methods

        [return:NotNullIfNotNull(nameof(obj))]
        private static object? Unwrap(object? obj) =>
            obj is LocalizedObjectDescriptor localizedObjectDescriptor ? Unwrap(localizedObjectDescriptor.Object)
            : obj is RecursivelyEditableTypeDescriptor recursivelyEditableTypeDescriptor ? Unwrap(recursivelyEditableTypeDescriptor.Object)
            : obj;

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Clears the selected object.
        /// </summary>
        public override void Clear()
        {
            SelectedObject = null;
            base.Clear();
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void TranslateContent(ref bool translationFinished)
        {
            // preventing translation of inner content
            translationFinished = true;
        }

        #endregion

        #region Private Methods

        private void SetSelectedObjects(object[]? values)
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
            for (int i = 0; i < result.Length; i++)
                result[i] = Unwrap(result[i]);
            return result;
        }

        private object Wrap(object o)
        {
            if (AllowPropertyRecursion && o is not RecursivelyEditableTypeDescriptor)
                o = new RecursivelyEditableTypeDescriptor(o);
            if (TranslationEnabled)
                o = new LocalizedObjectDescriptor(o);
            return o;
        }

        #endregion

        #region Event handlers

        private void propertyGrid_SelectedGridItemChanged(object? sender, SelectedGridItemChangedEventArgs e)
        {
            if (propertyGrid.SelectedGridItem?.PropertyDescriptor == null)
            {
                txtDescription.Caption = propertyGrid.SelectedGridItem?.Label;
                txtDescription.Text = null;
            }
            else
            {
                txtDescription.Caption = propertyGrid.SelectedGridItem.PropertyDescriptor.DisplayName;
                txtDescription.Text = propertyGrid.SelectedGridItem.PropertyDescriptor.Description;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
