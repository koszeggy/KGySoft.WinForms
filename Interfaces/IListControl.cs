using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    using System.Collections;

    /// <summary>
    /// Represents a list control.
    /// </summary>
    interface IListControl
    {
        /// <summary>
        /// Gets whether the there is no selected item in the list control (<see cref="SelectedValue"/> is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlTools.NotSelectedValue"/>)
        /// </summary>
        bool IsEmpty { get; }

        ///// <summary>
        ///// Gets an object representing the collection of the items contained in the list control.
        ///// </summary>
        //IList Items { get; }
        
        /// <summary>
        /// Gets or sets the selected item(s) of the list control.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table. If <paramref name="translateNames"/> is <see langword="true"/>&#160;or <paramref name="plusItems"/> is not <see cref="SelectionPlusItems.None"/>, then the data table will be modified.
        /// If the source table must be kept intact, then clone the table before calling this method by <see cref="DataTable.Clone"/>.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        void LoadFrom(DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        void LoadFrom(DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        void LoadFrom(DataTable dataTable, string valueMember, string displayMember);

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        void LoadFrom(Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        void LoadFrom(Type enumType, Type valueMemberType);

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        void LoadFrom(Type enumType);

        /// <summary>
        /// Binds the list control to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems);

        /// <summary>
        /// Binds the list control to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember);

        /// <summary>
        /// Gets or sets currently selected item in the list control.
        /// </summary>
        object SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the value of the member property specified by the <see cref="ValueMember"/> property.
        /// </summary>
        object SelectedValue { get; set; }

        /// <summary>
        /// Gets or sets the text that is selected in the editable portion of a list control.
        /// </summary>
        string SelectedText { get; set; }
        
        /// <summary>
        /// Gets or sets the index specifying the currently selected item.
        /// </summary>
        int SelectedIndex { get; set; }
        
        /// <summary>
        /// Occurs when the <see cref="SelectedIndex"/> property has changed.
        /// </summary>
        event EventHandler SelectedIndexChanged;

        /// <summary>
        /// Occurs when the <see cref="SelectedValue"/> property changes.
        /// </summary>
        event EventHandler SelectedValueChanged;

        /// <summary>
        /// Gets or sets the data source for the list control.
        /// </summary>
        object DataSource { get; set; }

        /// <summary>
        /// Gets or sets the property to display for the list control.
        /// </summary>
        string DisplayMember { get; set; }

        /// <summary>
        /// Gets or sets the property to use as the actual value for the items in the list control.
        /// </summary>
        string ValueMember { get; set; }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner list control.
        /// </summary>
        AutoCompleteMode AutoCompleteMode { get; set; }

        ///<summary>
        /// Gets or sets a value specifying the source of complete strings used for automatic completion.
        ///</summary>
        AutoCompleteSource AutoCompleteSource { get; set; }

        ///<summary>
        /// Gets or sets a custom <see cref="AutoCompleteStringCollection"/> to <see cref="AutoCompleteSource"/> property is <see cref="System.Windows.Forms.AutoCompleteSource.CustomSource"/>.
        ///</summary>
        AutoCompleteStringCollection AutoCompleteCustomSource { get; set; }
    }
}
