#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCombo.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Unified user control version of <see cref="AdvancedComboBox"/>.
    /// </summary>
    [DefaultBindingProperty("SelectedValue")]
    [ToolboxItem(true)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucCombo: ucCaptionedBase
    {
        #region Constructor, Dispose

        public ucCombo()
        {
            InitializeComponent();
            this.cmbCombo.EnabledChanged += new System.EventHandler(this.cmbCombo_EnabledChanged);
            this.cmbCombo.SelectedValueChanged += new EventHandler(this.cmbCombo_SelectedValueChanged);
            this.cmbCombo.TextChanged += new EventHandler(cmbCombo_TextChanged);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            this.cmbCombo.EnabledChanged -= cmbCombo_EnabledChanged;
            this.cmbCombo.SelectedValueChanged -= cmbCombo_SelectedValueChanged;
            this.cmbCombo.TextChanged -= cmbCombo_TextChanged;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Mandatory overrides

        public override void Clear()
        {
            cmbCombo.SelectedIndex = -1;
            base.Clear();
        }

        /// <summary>
        /// Gets or sets read-only state of the combo box.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets read-only state of the combo box.")]
        public override bool ReadOnly
        {
            get { return cmbCombo.ReadOnly; }
            set
            {
                cmbCombo.ReadOnly = value;
                base.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// </summary>
        /// <value>If the combo box is data-bound, then this is the <see cref="SelectedValue"/>, otherwise, the <see cref="Text"/> property.</value>
        public override object ControlValue
        {
            get { return cmbCombo.DataSource == null ? cmbCombo.Text : cmbCombo.SelectedValue; }
            set
            {
                if (cmbCombo.DataSource == null)
                {
                    string text = String.Empty;
                    if (value != null)
                        text = value.ToString();

                    SetText(text);
                }
                else
                    cmbCombo.SelectedValue = value;
            }
        }

        protected override Control MainControl
        {
            get { return cmbCombo; }
        }

        #endregion

        #region ucCombo Properties

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets the text associated with this control.")]
        [Browsable(true)]
        [Bindable(BindableSupport.Yes)]
        public override string Text
        {
            get { return cmbCombo.Text; }
            set { cmbCombo.Text = value; }
        }

        /// <summary>
        /// Gets the inner <see cref="AdvancedTextBox"/>.
        /// </summary>
        [Category("ucCombo")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AdvancedComboBox Combo
        {
            get { return cmbCombo; }
        }

        /// <summary>
        /// Gets or sets a value specifying the style of the combo box.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets a value specifying the style of the combo box.")]
        [DefaultValue(typeof(ComboBoxStyle), "DropDown")]
        public ComboBoxStyle DropDownStyle
        {
            get { return cmbCombo.DropDownStyle; }
            set { cmbCombo.DropDownStyle = value; }
        }

        /// <summary>
        /// Gets or sets whether the enabled combo box should be drawn by the system in <see cref="ComboBoxStyle.DropDownList"/> mode.
        /// If this property is <see langword="false"/>, then drop-down list appearance will be the same as in case of <see cref="ComboBoxStyle.DropDown"/> mode
        /// even with Windows Vista/Windows 7 themes.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets whether the enabled combo box should be drawn by the system in DropDownList mode. " +
            "If this property is false, then drop-down list appearance will be the same as in case of DropDown mode " +
            "even with Windows Vista/Windows 7 themes.")]
        [DefaultValue(true)]
        public bool SystemDrawDropDownListMode
        {
            get { return cmbCombo.SystemDrawDropDownListMode; }
            set { cmbCombo.SystemDrawDropDownListMode = value; }
        }

        /// <summary>
        /// Gets an object representing the collection of the items contained in the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets an object representing the collection of the items contained in the inner AdvancedComboBox.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [MergableProperty(false)]
        public ComboBox.ObjectCollection Items
        {
            get { return cmbCombo.Items; }
        }

        #endregion

        #region Methods

        #region Private Methods

        private void SetText(string value)
        {
            if (String.IsNullOrEmpty(value))
                cmbCombo.Clear();
            else
                cmbCombo.Text = value;
        }

        #endregion

        #region Handled events

        private void cmbCombo_EnabledChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        private void cmbCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        void cmbCombo_TextChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        #endregion

        #endregion

        #region IListControl Members

        /// <summary>
        /// Gets or sets the value of the member property specified by the <see cref="ValueMember"/> property.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(BindableSupport.Default)]
        public object SelectedValue
        {
            get { return cmbCombo.SelectedValue; }
            set { cmbCombo.SelectedValue = value; }
        }

        /// <summary>
        /// Gets whether the there is no selected item in the combo box (<see cref="SelectedValue"/> or is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlExtensions.NotSelectedValue"/>)
        /// </summary>
        [Browsable(false)]
        public bool IsEmpty
        {
            get { return cmbCombo.IsEmpty(); }
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedIndex"/> property has changed.
        /// </summary>
        [Category("ucCombo")]
        public event EventHandler SelectedIndexChanged
        {
            add { cmbCombo.SelectedIndexChanged += value; }
            remove { cmbCombo.SelectedIndexChanged -= value; }
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedValue"/> property changes.
        /// </summary>
        [Category("ucCombo")]
        public event EventHandler SelectedValueChanged
        {
            add { cmbCombo.SelectedValueChanged += value; }
            remove { cmbCombo.SelectedValueChanged -= value; }
        }

        /// <summary>
        /// Gets or sets currently selected item in the combo box.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(BindableSupport.Yes)]
        public object SelectedItem
        {
            get { return cmbCombo.SelectedItem; }
            set { cmbCombo.SelectedItem = value; }
        }

        /// <summary>
        /// Gets or sets the text that is selected in the editable portion of a combo box.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string SelectedText
        {
            get { return cmbCombo.SelectedText; }
            set { cmbCombo.SelectedText = value; }
        }

        /// <summary>
        /// Gets or sets the index specifying the currently selected item.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int SelectedIndex
        {
            get { return cmbCombo.SelectedIndex; }
            set { cmbCombo.SelectedIndex = value; }
        }

        /// <summary>
        /// Gets or sets the data source for the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets the data source for the inner AdvancedComboBox.")]
        [DefaultValue(null)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get { return cmbCombo.DataSource; }
            set { cmbCombo.DataSource = value; }
        }

        /// <summary>
        /// Gets or sets the property to display for the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets the property to display for the inner AdvancedComboBox.")]
        [DefaultValue("")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get { return cmbCombo.DisplayMember; }
            set { cmbCombo.DisplayMember = value; }
        }

        /// <summary>
        /// Gets or sets the property to use as the actual value for the items in the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets the property to use as the actual value for the items in the inner AdvancedComboBox.")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get { return cmbCombo.ValueMember; }
            set { cmbCombo.ValueMember = value; }
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner combo box.
        /// </summary>
        [Category("ucCombo")]
        [Description("Gets or sets an option that controls how automatic completion works for the inner combo box.")]
        [DefaultValue(AutoCompleteMode.None)]
        public AutoCompleteMode AutoCompleteMode
        {
            get { return cmbCombo.AutoCompleteMode; }
            set { cmbCombo.AutoCompleteMode = value; }
        }

        ///<summary>
        /// Gets or sets a value specifying the source of complete strings used for automatic completion.
        ///</summary>
        [Category("ucCombo")]
        [Description("Gets or sets a value specifying the source of complete strings used for automatic completion.")]
        [DefaultValue(AutoCompleteSource.None)]
        public AutoCompleteSource AutoCompleteSource
        {
            get { return cmbCombo.AutoCompleteSource; }
            set { cmbCombo.AutoCompleteSource = value; }
        }

        ///<summary>
        /// Gets or sets a custom <see cref="AutoCompleteStringCollection"/> to <see cref="AutoCompleteSource"/> property is <see cref="System.Windows.Forms.AutoCompleteSource.CustomSource"/>.
        ///</summary>
        [Category("ucCombo")]
        [Description("Gets or sets a custom AutoCompleteStringCollection to AutoCompleteSource property is CustomSource.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get { return cmbCombo.AutoCompleteCustomSource; }
            set { cmbCombo.AutoCompleteCustomSource = value; }
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        public void LoadFrom(Type enumType, Type valueMemberType)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        public void LoadFrom(Type enumType)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember);
        }

        #endregion
    }
}
