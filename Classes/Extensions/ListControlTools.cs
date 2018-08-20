extern alias lang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using KGySoft.Libraries;
using KGySoft.Libraries.Reflection;
using KGySoft.Libraries.Collections;

using Language = lang::KGySoft.Libraries.Language.Language;

// TODO: Enum-os LoadFrom-nál meg lehessen adni, hogy int vagy enum legyen-e a ValueMember (EnumToDataTable-nél ez mód megadható)
namespace KGySoft.Controls
{
    /// <summary>
    /// Extension methods for <see cref="ListControl"/> class.
    /// </summary>
    public static class ListControlTools
    {
        /// <summary>
        /// Gets whether the there is no selected item in the list control (<see cref="ListControl.SelectedValue"/> or is <see langword="null"/>, <see cref="DBNull"/> or equals with <see cref="ControlTools.NotSelectedValue"/>)
        /// </summary>
        public static bool IsEmpty(this ListControl control)
        {
            IConvertible convertible;
            return control.SelectedValue == null || control.SelectedValue == DBNull.Value 
                || ((convertible = control.SelectedValue as IConvertible) != null && convertible.ToInt32(CultureInfo.InvariantCulture) == ControlTools.NotSelectedValue);
        }

        /// <summary>
        /// Allows DBNull for non display/value columns.
        /// </summary>
        private static void SetAllowDBNull(DataTable dt, DataColumn colDisplay, DataColumn colValue)
        {
            if (dt.Columns.Count <= 2)
                return;

            foreach (DataColumn column in dt.Columns)
            {
                if (column.In(colDisplay, colValue))
                    continue;
                if (!column.AllowDBNull)
                    column.AllowDBNull = true;
            }
        }

        private static void BindControl(ListControl control, object dataSource, string valueMember, string displayMember)
        {
            ListBox lst = null;
            ComboBox cmb = control as ComboBox;
            if (cmb != null)
                cmb.BeginUpdate();
            else
            {
                lst = control as ListBox;
                if (lst != null)
                    lst.BeginUpdate();
            }
            try
            {
                control.DisplayMember = displayMember;
                control.ValueMember = valueMember;
                control.DataSource = dataSource;
            }
            finally
            {
                if (cmb != null)
                    cmb.EndUpdate();
                else if (lst != null)
                    lst.EndUpdate();
            }
        }

        private static object ChangeType(Type type, object value)
        {
            if (type == value.GetType())
                return value;
            if (type.IsEnum)
                return Enum.ToObject(type, value);
            return Convert.ChangeType(value, type);
        }

        #region Binding to DataTable

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="dataTable">The data source table. If <paramref name="translateNames"/> is <see langword="true"/> or <paramref name="plusItems"/> is not <see cref="SelectionPlusItems.None"/>, then the data table will be modified.
        /// If the source table must be kept intact, then clone the table before calling this method by <see cref="DataTable.Clone"/>.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. Works only if the displayed column contains string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type and the displayed column must have string data type.</param>
        public static void LoadFrom(this ListControl control, DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (dataTable == null)
                throw new ArgumentNullException("dataTable");
            int indDisplay = dataTable.Columns.IndexOf(displayMember);
            if (indDisplay == -1)
                throw new ArgumentException(Language.Translate("Display member column not found"), "displayMember");
            int indValue = dataTable.Columns.IndexOf(valueMember);
            if (indValue == -1)
                throw new ArgumentException(Language.Translate("Value member column not found"), "valueMember");

            DataColumn colDisplay = dataTable.Columns[indDisplay];

            // Preventing problems due to too long values
            if (plusItems != SelectionPlusItems.None || translateNames)
                colDisplay.MaxLength = -1;

            // translating names
            if (translateNames && colDisplay.DataType == typeof(string))
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    // translating if not null/dbnull:
                    if (dr[colDisplay] is string)
                        dr[colDisplay] = Language.Translate((string)dr[colDisplay] + (!String.IsNullOrEmpty(distinctionPostfix) ? Language.DistinctionSeparator + distinctionPostfix : String.Empty));
                }
            }

            if (sortByDisplayedValues)
            {
                DataView dv = dataTable.DefaultView;
                dv.Sort = "[" + displayMember + "] ASC";
                dataTable = dv.ToTable();

                colDisplay = dataTable.Columns[indDisplay];
            }

            DataColumn colValue = dataTable.Columns[indValue];

            if (plusItems != SelectionPlusItems.None)
                SetAllowDBNull(dataTable, colDisplay, colValue);

            // In case of all selected plus items the order is: Not selected, All, None
            if ((plusItems & SelectionPlusItems.ItemNone) != 0)
            {
                DataRow dr = dataTable.NewRow();

                dr[colValue] = ChangeType(colValue.DataType, ControlTools.NoneSelectedValue);
                dr[colDisplay] = Language.Translate(ControlTools.NoneSelectedText);
                dataTable.Rows.InsertAt(dr, 0);
            }
            if ((plusItems & SelectionPlusItems.ItemAll) != 0)
            {
                DataRow dr = dataTable.NewRow();
                dr[colValue] = ChangeType(colValue.DataType, ControlTools.AllSelectedValue);
                dr[colDisplay] = Language.Translate(ControlTools.AllSelectedText);
                dataTable.Rows.InsertAt(dr, 0);
            }
            if ((plusItems & SelectionPlusItems.ItemNotSelected) != 0)
            {
                DataRow dr = dataTable.NewRow();

                // reference: DBNull
                if (!colValue.DataType.IsValueType)
                    dr[colValue] = null;
                else
                    dr[colValue] = ChangeType(colValue.DataType, ControlTools.NotSelectedValue);
                dr[colDisplay] = Language.Translate(ControlTools.NotSelectedText);
                dataTable.Rows.InsertAt(dr, 0);
            }

            // binding
            BindControl(control, dataTable, valueMember, displayMember);
        }

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public static void LoadFrom(this ListControl control, DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            LoadFrom(control, dataTable, valueMember, displayMember, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds the list control to a <see cref="DataTable"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the the list control.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the list control.</param>
        public static void LoadFrom(this ListControl control, DataTable dataTable, string valueMember, string displayMember)
        {
            LoadFrom(control, dataTable, valueMember, displayMember, false, null, false, SelectionPlusItems.None);
        }

        #endregion

        #region Binding to Enum

        /// <summary>
        /// Binds list control box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public static void LoadFrom(this ListControl control, Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (enumType == null)
                throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new ArgumentException(Language.Translate("Specified type is not an enum type"), "enumType");

            if (valueMemberType == null)
                valueMemberType = enumType;
            if (!typeof(IConvertible).IsAssignableFrom(valueMemberType))
                throw new ArgumentException(Language.Translate("Specified type must is not an IConvertible type"), "valueMemberType");

            // adding items and translating
            Array values = Enum.GetValues(enumType);
            CircularList<KeyValuePair<object, string>> result = new CircularList<KeyValuePair<object, string>>(values.Length + 3);

            foreach (object enumValue in values)
            {
                object key = ChangeType(valueMemberType, enumValue);
                string value = !translateNames ? enumValue.ToString() : Language.Translate(enumValue + (!String.IsNullOrEmpty(distinctionPostfix) ? Language.DistinctionSeparator + distinctionPostfix : String.Empty));
                result.Add(new KeyValuePair<object, string>(key, value));
            }

            // sorting
            if (sortByDisplayedValues)
            {
                StringComparer comparer = translateNames ? StringComparer.Create(Language.FormattingCulture, true) : StringComparer.InvariantCultureIgnoreCase;
                result.Sort((item1, item2) => comparer.Compare(item1.Value, item2.Value));
            }

            // In case of all selected plus items the order is: Not selected, All, None
            if ((plusItems & SelectionPlusItems.ItemNone) != 0)
            {
                object key = ChangeType(valueMemberType, ControlTools.NoneSelectedValue);
                string value = Language.Translate(ControlTools.NoneSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }
            if ((plusItems & SelectionPlusItems.ItemAll) != 0)
            {
                object key = ChangeType(valueMemberType, ControlTools.AllSelectedValue);
                string value = Language.Translate(ControlTools.AllSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }
            if ((plusItems & SelectionPlusItems.ItemNotSelected) != 0)
            {
                object key = !valueMemberType.IsValueType ? DBNull.Value : ChangeType(valueMemberType, ControlTools.NotSelectedValue);
                string value = Language.Translate(ControlTools.NotSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }

            // binding
            BindControl(control, result, "Key", "Value");
        }

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public static void LoadFrom(this ListControl control, Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
        {
            LoadFrom(control, enumType, valueMemberType, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds list control box to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the list control. If <see langword="null"/>, then original enum value will used as value member.</param>
        public static void LoadFrom(this ListControl control, Type enumType, Type valueMemberType)
        {
            LoadFrom(control, enumType, valueMemberType, false, null, false, SelectionPlusItems.None);
        }

        /// <summary>
        /// Binds the list control to the values of an <see cref="Enum"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        public static void LoadFrom(this ListControl control, Type enumType)
        {
            LoadFrom(control, enumType, null, false, null, false, SelectionPlusItems.None);
        }

        #endregion

        #region Binding to IEnumerable

        /// <summary>
        /// Binds the list control to a <paramref name="collection"/>.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public static void LoadFrom<T>(this ListControl control, IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (valueMember == null)
                throw new ArgumentNullException("valueMember");
            if (displayMember == null)
                throw new ArgumentNullException("displayMember");

            Type elementType = typeof(T);
            PropertyInfo propValue = elementType.GetProperty(valueMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propValue == null)
                throw new ArgumentException(Language.Translate("Value member instance property not found"), "valueMember");
            PropertyInfo propDisplay = elementType.GetProperty(displayMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propDisplay == null)
                throw new ArgumentException(Language.Translate("Display member instance property not found"), "displayMember");

            // simple case
            if (!translateNames && !sortByDisplayedValues && plusItems == SelectionPlusItems.None)
            {
                BindControl(control, collection is IList ? collection : collection.ToList(), valueMember, displayMember);
                return;
            }

            // adding items and translating
            List<KeyValuePair<object, string>> result = new List<KeyValuePair<object, string>>(
                from item in collection
                let displayValue = (Reflector.GetProperty(item, propDisplay) ?? String.Empty).ToString()
                select new KeyValuePair<object, string>(
                    Reflector.GetProperty(item, propValue),
                    !translateNames ? displayValue : Language.Translate(displayValue + (!String.IsNullOrEmpty(distinctionPostfix) ? Language.DistinctionSeparator + distinctionPostfix : String.Empty))));

            // sorting
            if (sortByDisplayedValues)
            {
                StringComparer comparer = translateNames ? StringComparer.Create(Language.FormattingCulture, true) : StringComparer.InvariantCultureIgnoreCase;
                result.Sort((item1, item2) => comparer.Compare(item1.Value, item2.Value));
            }

            // In case of all selected plus items the order is: Not selected, All, None
            if ((plusItems & SelectionPlusItems.ItemNone) != 0)
            {
                object key = ChangeType(propValue.PropertyType, ControlTools.NoneSelectedValue);
                string value = Language.Translate(ControlTools.NoneSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }
            if ((plusItems & SelectionPlusItems.ItemAll) != 0)
            {
                object key = ChangeType(propValue.PropertyType, ControlTools.AllSelectedValue);
                string value = Language.Translate(ControlTools.AllSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }
            if ((plusItems & SelectionPlusItems.ItemNotSelected) != 0)
            {
                object key = !propValue.PropertyType.IsValueType ? DBNull.Value : ChangeType(propValue.PropertyType, ControlTools.NotSelectedValue);
                string value = Language.Translate(ControlTools.NotSelectedText);
                result.Insert(0, new KeyValuePair<object, string>(key, value));
            }

            // binding
            BindControl(control, result, "Key", "Value");
        }

        /// <summary>
        /// Binds the list control to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public static void LoadFrom<T>(this ListControl control, IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            LoadFrom(control, collection, valueMember, displayMember, false, null, false, plusItems);
        }

        /// <summary>
        /// Binds list control box to a <paramref name="collection"/>. Items will not be sorted and translated.
        /// </summary>
        /// <param name="control">The target <see cref="ListControl"/> instance.</param>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the the list control.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the list control.</param>
        public static void LoadFrom<T>(this ListControl control, IEnumerable<T> collection, string valueMember, string displayMember)
        {
            LoadFrom(control, collection, valueMember, displayMember, false, null, false, SelectionPlusItems.None);
        }

        #endregion
    }
}
