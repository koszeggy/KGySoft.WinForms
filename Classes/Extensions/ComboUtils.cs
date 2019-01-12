using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    /// <summary>
    /// Contains extension methods for <see cref="ComboBox"/>.
    /// </summary>
    public static class ComboUtils
    {
        #region Methods

        #region Binding to DataTable

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="dataTable">The data source table. If <paramref name="translateNames"/> is <see langword="true"/>&#160;or <paramref name="plusItems"/> is not <see cref="SelectionPlusItems.None"/>, then the data table will be modified.
        /// If the source table must be kept intact, then clone the table before calling this method by <see cref="DataTable.Clone"/>.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. Works only if the displayed column contains string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type and the displayed column must have string data type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(dataTable, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(dataTable, valueMember, displayMember, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, DataTable dataTable, string valueMember, string displayMember)
        {
            cmb.LoadFrom(dataTable, valueMember, displayMember, false, null, false, SelectionPlusItems.None);
        }

        #endregion

        #region Binding to Enum

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(enumType, valueMemberType, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(enumType, valueMemberType, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will used as value member.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, Type enumType, Type valueMemberType)
        {
            cmb.LoadFrom(enumType, valueMemberType, false, null, false, SelectionPlusItems.None);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo(this ComboBox cmb, Type enumType)
        {
            cmb.LoadFrom(enumType, null, false, null, false, SelectionPlusItems.None);
        }

        #endregion

        #region Binding to IEnumerable

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/>&#160;to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo<T>(this ComboBox cmb, IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(collection, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo<T>(this ComboBox cmb, IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            cmb.LoadFrom(collection, valueMember, displayMember, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="cmb">The target <see cref="ComboBox"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        [Obsolete("Use LoadFrom instead")]
        public static void LoadCombo<T>(this ComboBox cmb, IEnumerable<T> collection, string valueMember, string displayMember)
        {
            cmb.LoadFrom(collection, valueMember, displayMember, false, null, false, SelectionPlusItems.None);
        }

        #endregion

        #endregion
    }
}
