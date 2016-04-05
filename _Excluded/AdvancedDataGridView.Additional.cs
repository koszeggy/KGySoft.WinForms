using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace KGySoft.Controls
{

    #region Namespace szintû classok

    public class CalculateFieldEventArgs : EventArgs
    {
        private object value;
        private AdvancedDataGridView.Field fieldData;
        private DataTable dt;
        private int rowIndex;
        private int colIndex;

        /// <summary>
        /// Ennek kell értéket adnunk, lehetõleg DataType típusút.
        /// Példa: if (e.ColumnName == "clcValami") e.CellValue = "Valami"
        /// </summary>
        public object CellValue
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// A beállítandó mezõ típusa
        /// </summary>
        public Type DesiredType
        {
            get { return fieldData.ValueType; }
        }

        /// <summary>
        /// A DataTable, ami a beállítandó cellát is tartalmazza (írásra csak a CellValue-t használjuk, ne bántsuk a DataTable többi értékét!)
        /// </summary>
        public DataTable DataTable
        {
            get { return dt; }
        }

        /// <summary>
        /// A DataRow, ami a beállítandó cellát is tartalmazza (írásra csak a CellValue-t használjuk, ne bántsuk a DataTable többi értékét!)
        /// </summary>
        public DataRow DataRow
        {
            get { return dt.Rows[rowIndex]; }
        }

        public int RowIndex
        {
            get { return rowIndex; }
        }

        public int ColumnIndex
        {
            get { return colIndex; }
        }

        /// <summary>
        /// Az oszlop neve, esetleges formázó postfixekkel együtt
        /// </summary>
        public string ColumnName
        {
            get { return dt.Columns[colIndex].ColumnName; }
        }

        public AdvancedDataGridView.Field FieldData
        {
            get { return fieldData; }
        }

        public CalculateFieldEventArgs(AdvancedDataGridView.Field fieldData, DataTable dt, int rowIndex, int colIndex)
        {
            this.fieldData = fieldData;
            this.dt = dt;
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
        }
    }

    public class FieldCellClickEventArgs : EventArgs
    {
        protected AdvancedDataGridView grid;
        protected object value;
        protected AdvancedDataGridView.Field fieldData;
        protected AdvancedDataGridView.AppearanceType actualAppearanceType;
        protected int rowIndex;
        protected int colIndex;

        /// <summary>
        /// Visszaadja a cella értékét
        /// </summary>
        public object CellValue
        {
            get { return value; }
        }

        /// <summary>
        /// Visszaadja az aktuális cellát
        /// </summary>
        public DataGridViewCell Cell
        {
            get { return grid[colIndex, rowIndex]; }
        }

        public int RowIndex
        {
            get { return rowIndex; }
        }

        public int ColumnIndex
        {
            get { return colIndex; }
        }

        /// <summary>
        /// Az oszlop neve, esetleges formázó postfixekkel együtt
        /// </summary>
        public string ColumnName
        {
            get { return fieldData.ColumnName; }
        }

        /// <summary>
        /// A mezõ definíciója a Fields listában, vagy az automatikusan generált információk, ha az üres volt (ne írjuk felül)
        /// </summary>
        public AdvancedDataGridView.Field FieldData
        {
            get { return fieldData; }
        }

        /// <summary>
        /// A ténylegesen generált oszlop megjelenési típusa
        /// </summary>
        public AdvancedDataGridView.AppearanceType ActualAppearanceType
        {
            get { return actualAppearanceType; }
        }

        protected FieldCellClickEventArgs() { } // leszármaztatás miatt kell

        public FieldCellClickEventArgs(AdvancedDataGridView grid, AdvancedDataGridView.Field fieldData, AdvancedDataGridView.AppearanceType actualAppearanceType,
            int rowIndex, int colIndex, object value)
        {
            this.grid = grid;
            this.fieldData = fieldData;
            this.actualAppearanceType = actualAppearanceType;
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.value = value;
        }
    }

    public class FieldCellFormattingEventArgs : FieldCellClickEventArgs
    {
        private Type desiredType;
        private DataGridViewCellStyle cellStyle;
        private bool formattingApplied;

        /// <summary>
        /// Visszaadja / beállítja a cella értékét
        /// </summary>
        public new object CellValue
        {
            get { return base.CellValue; }
            set { base.value = value; }
        }

        /// <summary>
        /// Ilyen típust formázunk
        /// </summary>
        public Type DesiredType
        {
            get { return desiredType; }
        }

        /// <summary>
        /// A beállítandó stílus
        /// </summary>
        public DataGridViewCellStyle CellStyle
        {
            get { return cellStyle; }
            set { cellStyle = value; }
        }

        /// <summary>
        /// Kész-e a formázás. Legyen true, ha nincs szükség további formázásra, azaz pl. a mezõhöz megadott DefaultCellStyle szerinti formázásra sem.
        /// </summary>
        public bool FormattingApplied
        {
            get { return formattingApplied; }
            set { formattingApplied = value; }
        }

        /// <summary>
        /// Egy cella (formázatlan) értékének lekérése az aktuális sorból
        /// </summary>
        /// <param name="columnIndex">Oszlop indexe</param>
        public object GetValue(int columnIndex)
        {
            return grid[columnIndex, rowIndex].Value;
        }

        /// <summary>
        /// Egy cella (formázatlan) értékének lekérése az aktuális sorból
        /// </summary>
        /// <param name="columnName">Oszlop neve</param>
        public object GetValue(string columnName)
        {
            return grid[columnName, rowIndex].Value;
        }

        public FieldCellFormattingEventArgs(AdvancedDataGridView grid, AdvancedDataGridView.Field fieldData, AdvancedDataGridView.AppearanceType actualAppearanceType,
            int rowIndex, int colIndex, object value, Type desiredType, DataGridViewCellStyle cellStyle, bool formattingApplied)
        {
            this.grid = grid;
            this.fieldData = fieldData;
            this.actualAppearanceType = actualAppearanceType;
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.value = value;
            this.desiredType = desiredType;
            this.cellStyle = cellStyle;
            this.formattingApplied = formattingApplied;
        }
    }

    public class BeforeGenerateColumnEventArgs : EventArgs
    {
        private DataColumn dataColumn;
        private AdvancedDataGridView.Field field;

        /// <summary>
        /// A generálandó mezõ adatai (módosítsuk az értékeit, ha kívánjuk)
        /// </summary>
        public AdvancedDataGridView.Field FieldData
        {
            get { return field; }
        }

        /// <summary>
        /// A generálás alapjául szolgáló oszlop
        /// </summary>
        public DataColumn DataColumn
        {
            get { return dataColumn; }
        }

        public BeforeGenerateColumnEventArgs(DataColumn col, AdvancedDataGridView.Field field)
        {
            dataColumn = col;
            this.field = field;
        }
    }

    #endregion

    public partial class AdvancedDataGridView
    {
        #region Osztályszintû enumok

		public enum ItemSelectionMode
		{
			All, None, Invert
		}

        /// <summary>
        /// A mezõ típusa adatforrás szerint
        /// </summary>
        public enum FieldType
        {
            /// <summary>
            /// A mezõ egy DataTable (pl. egy Recordsetben) egyik oszlopa 
            /// </summary>
            Data,

            /// <summary>
            /// A mezõt a kliens számolja ki - lásd CalculateField esemény
            /// </summary>
            Calculated
        }

        /// <summary>
        /// A mezõ típusa a forrásban (DataTable/Recordset-ben).
        /// Ha FieldType = Data, maradhat AnyObject, de Calculated mezõ esetén ez határozza meg létrehozott oszlop típusát (AnyObject esesetén Object)
        /// Csak azért van külön definiálva, mert Type típusú property nem szerkeszthetõ alapértelmezés szerint a Property gridben
        /// </summary>
        public enum DataType
        {
            AnyObject,
            Boolean,
            Byte,
            Int16,
            Int32,
            Int64,
            SByte,
            UInt16,
            UInt32,
            UInt64,
            Single,
            Double,
            Decimal,
            DateTime,
            Char,
            String
        }

        /// <summary>
        /// Az oszlop típusa a gridben. megjelenés 
        /// A legtöbb választás csak a formázás alapértelmezését befolyásolja, amik minden esetben finomhagolhatók
        /// </summary>
        public enum AppearanceType
        {
            /// <summary>
            /// A megjelenítendõ oszlop típusa a forrás oszlop adattípusa vagy a formázó postfixek alapján dõl el (a postfix az erõsebb, de a postfixek használata ellenjavallt).
            /// Ha calculated mezõ esetén a DataType = AnyObject és az AppearanceType = Auto, Text oszlop lesz belõle
            /// </summary>
            Auto,

            // Általánosab formázások
            Hidden,               // A grid tartalmazni fogja az oszlopot, de az nem jelenik meg (és jobbklikkes settingsben sem kapcsolható be)
            Text,                 // Szöveges oszlop (aé: balra igazított textbox)
            Integer,              // Egész szám oszlop (aé: jobbra igazított 0 tizedessel formázott szám)
            Float,                // Szám oszlop (aé: jobbra igazított 2 tizedessel formázott szám)
            Percent,              // Százalék oszlop (aé: jobbra igazított 2 tizedessel formázott százalékos érték)
            Checkbox,             // Checkboxokat tartalmazó oszlop
            Image,                // Kép oszlop
            ImageAndText,         // Kép és szöveg oszlop
            Link,                 // Link oszlop (hozzá LinkClicked esemény)
            Date,                 // Dátum oszlop
            DateTime,             // Dátum-idõ oszlop

            // Specifikusabb formázások
            TranslatedText,       // Aktuális nyelvre fordítandó szöveg
            TranslatedImageText,  // Kép és aktuális nyelvre fordítandó szöveg oszlop
            Amount,               // Mint a float, de az AmountThreshold alá esõ értékeket színezi AmountNegativeForeColor alapján
            Currency,             // Zászlók (szöveges alaptípus szükséges)
            CaptionedCurrency,    // Zászlók devizanemmel (aé: félkövér betûk)
        }

        /// <summary>
        /// Hogy viselkedjenek a felhasználó által definiált menük
        /// </summary>
        public enum PopupMenuBehaviours
        {
            /// <summary>
            /// Mindig megjelenik a default menü és a felhasználó által definiált menü is
            /// </summary>
            MergeUserWithDefault,

            /// <summary>
            /// Mindig csak felhasználó által definiált menü van, azaz ha nem definiált semmit, nincs PopupMenu  (mint az eredeti gridnél)
            /// </summary>
            AlwaysUserMenu,

            /// <summary>
            /// Ha a felhasználó definiált saját menüt, az jelenik meg, egyébként a default menü
            /// </summary>
            DefaultWhenNoUserMenu
        }

        #endregion

        #region Osztályszintû struct-ok

        public struct ColumnTag
        {
            private Field fieldData;
            private AppearanceType actualAppearanceType;

            /// <summary>
            /// A mezõ definíciója a Fields listában, vagy az automatikusan generált információk, ha az üres volt (ne írjuk felül)
            /// </summary>
            public Field FieldData
            {
                get { return fieldData; }
            }

            /// <summary>
            /// A ténylegesen generált oszlop megjelenési típusa
            /// </summary>
            public AppearanceType ActualAppearanceType
            {
                get { return actualAppearanceType; }
            }

            public ColumnTag(Field f, AppearanceType dt)
            {
                fieldData = f;
                actualAppearanceType = dt;
            }
        }

        #endregion

        #region Osztályszintû (FxDataGridView osztályon belüli) classok

        /// <summary>
        /// Mezõket tartalmazó lista. Csak azért van leszérmaztatva List&lt;Field&gt;-bõl, hogy lehessen a mezõkre string alapú indexert is használni
        /// </summary>
        public class FieldsCollection : List<Field>
        {
            public Field this[string columnName]
            {
                get
                {
                    foreach (Field f in this)
                        if (f.ColumnName == columnName)
                            return f;

                    throw new ArgumentException("Collection does not contain field " + columnName);
                }
            }
        }

        /// <summary>
        /// RefreshGrid(DataTable) jellegû feltöltéshez a grid mezõi.
        /// Ha nincsenek felvéve mezõk, automatikus mezõgenerálás van
        /// </summary>
        public class Field
        {
            #region Objektumváltozók

            internal static int colIndex = 0;
            private static DataGridViewCellStyle neutralCellStyle;
            private static DataGridViewCellStyle defaultLeftCellStyle;
            private static DataGridViewCellStyle defaultIntegerCellStyle;
            private static DataGridViewCellStyle defaultFloatCellStyle;
            private static DataGridViewCellStyle defaultPercentCellStyle;
            private static DataGridViewCellStyle defaultCenteredCellStyle;
            private static DataGridViewCellStyle defaultImageCellStyle;
            private static DataGridViewCellStyle defaultDateCellStyle;
            private static DataGridViewCellStyle defaultDateTimeCellStyle;
            private static DataGridViewCellStyle defaultCurrencyCellStyle;

            private DataType dataType = DataType.AnyObject;
            private string columnName = "";
            private int displayIndex = -1;
            private FieldType fieldType = FieldType.Data;
            private AppearanceType appearanceType = AppearanceType.Auto;
            private DataGridViewCellStyle defaultCellStyle = new DataGridViewCellStyle(NeutralCellStyle);
            private string headerText = "";
            private DataGridViewAutoSizeColumnsMode autoSizeMode = DataGridViewAutoSizeColumnsMode.None;
            private int width = 0;
            private Type valueType = typeof(object);
            private DataGridViewTriState resizable = DataGridViewTriState.NotSet;

            #endregion

            #region Publikus property-k

            /// <summary>
            /// Az oszlop neve (Data mezõtípus esetén ami a DataTable-bõl jön)
            /// </summary>
            [Description("Az oszlop neve (Data mezõtípus esetén ami a DataTable-bõl jön)")]
            [DefaultValue("")]
            public string ColumnName
            {
                get { return columnName; }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentNullException("ColumnName cannot be null or empty!");
                    }
                    columnName = value;
                }
            }

            /// <summary>
            /// Az oszlop átméretezhetõ-e
            /// </summary>
            [DefaultValue(typeof(DataGridViewTriState), "NotSet")]
            [Description("Az oszlop átméretezhetõ-e")]
            public DataGridViewTriState Resizable
            {
                get { return resizable; }
                set { resizable = value; }
            }

            /// <summary>
            /// Oszlop szélessége (ha nincs AutoSize)
            /// </summary>
            [DefaultValue(0)]
            [Description("Oszlopszélesség, ha nincs beállítva Auto szélesség")]
            public int Width
            {
                get { return width; }
                set { width = value; }
            }

            /// <summary>
            /// A fejléc felirata (üres string esetén a ColumnName-bõl)
            /// </summary>
            [DefaultValue("")]
            [Description("Fejléc szövege. Üres string esetén a ColumnName-bõl, fordítódik a beállított nyelvre")]
            public string HeaderText
            {
                get { return headerText; }
                set { headerText = value ?? ""; }
            }

            /// <summary>
            /// A mezõ típusa adatforrás szerint
            /// </summary>
            [Description("A mezõ típusa adatforrás szerint")]
            [DefaultValue(typeof(FieldType), "Data")]
            public FieldType FieldType
            {
                get { return fieldType; }
                set { fieldType = value; }
            }

            /// <summary>
            /// A forrásmezõ .NET-es belsõ típusa. A Designerben a DataType nyújt választható listát
            /// </summary>
            public Type ValueType
            {
                get { return valueType; }
            }

            /// <summary>
            /// A mezõ típusa a forrásban (DataTable/Recordset-ben).
            /// Ha FieldType = Data, maradhat AnyObject, de Calculated mezõ esetén ez határozza meg létrehozott oszlop típusát (Auto esesetén object)
            /// </summary>
            [Description("A mezõ típusa a forrásban (DataTable/Recordset-ben). Ha FieldType = Data, maradhat Auto, de Calculated mezõ esetén ez határozza meg létrehozott oszlop típusát (Auto esesetén object)")]
            [DefaultValue(typeof(DataType), "AnyObject")]
            [RefreshProperties(RefreshProperties.All)]
            public DataType DataType
            {
                get { return dataType; }
                set { SetDataType(value); }
            }

            /// <summary>
            /// Az oszlop típusa a gridben.
            /// A legtöbb választás csak a formázás alapértelmezését befolyásolja, amik minden esetben finomhagolhatók.
            /// FONTOS: Az AppearanceType állítgatásakor felülíródik a DefaultCellStyle property!
            /// </summary>
            [Description("Az oszlop típusa a gridben. A legtöbb választás csak a formázás alapértelmezését befolyásolja, amik minden esetben finomhagolhatók. FONTOS: Az AppearanceType állítgatásakor felülíródik a DefaultCellStyle property!")]
            [DefaultValue(typeof(AppearanceType), "Auto")]
            [RefreshProperties(RefreshProperties.All)]
            public AppearanceType AppearanceType
            {
                get { return appearanceType; }
                set { SetAppearanceType(value); }
            }

            /// <summary>
            /// Oszlop helye megjelenítéskor (-1: nincs beállítva)
            /// </summary>
            [Description("Oszlop helye megjelenítéskor (-1: nincs beállítva)")]
            [DefaultValue(-1)]
            public int DisplayIndex
            {
                get { return displayIndex; }
                set { displayIndex = value; }
            }

            /// <summary>
            /// Cellaméretezés
            /// </summary>
            [Description("Cellaméretezés")]
            [DefaultValue(typeof(DataGridViewAutoSizeColumnsMode), "None")]
            public DataGridViewAutoSizeColumnsMode AutoSizeMode
            {
                get { return autoSizeMode; }
                set { autoSizeMode = value; }
            }

            /// <summary>
            /// Alapértelmezett cellaformázás az oszlophoz
            /// </summary>
            [Description("Alapértelmezett cellaformázás az oszlophoz")]
            public DataGridViewCellStyle DefaultCellStyle
            {
                get { return defaultCellStyle; }
                set { defaultCellStyle = value; }
            }

            #endregion

            #region belsõ static property-k

            // Az alábbiak azért static property-k, mert elsõ eléréskor hozzuk létre a mögöttük lévõ konstansokat

            internal static DataGridViewCellStyle NeutralCellStyle
            {
                get
                {
                    if (neutralCellStyle == null)
                        neutralCellStyle = new DataGridViewCellStyle();
                    return neutralCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultLeftCellStyle
            {
                get
                {
                    if (defaultLeftCellStyle == null)
                    {
                        defaultLeftCellStyle = new DataGridViewCellStyle();
                        defaultLeftCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    }
                    return defaultLeftCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultIntegerCellStyle
            {
                get
                {
                    if (defaultIntegerCellStyle == null)
                    {
                        defaultIntegerCellStyle = new DataGridViewCellStyle();
                        defaultIntegerCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        defaultIntegerCellStyle.Format = "N0";
                    }
                    return defaultIntegerCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultFloatCellStyle
            {
                get
                {
                    if (defaultFloatCellStyle == null)
                    {
                        defaultFloatCellStyle = new DataGridViewCellStyle();
                        defaultFloatCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        defaultFloatCellStyle.Format = "N2";
                    }
                    return defaultFloatCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultPercentCellStyle
            {
                get
                {
                    if (defaultPercentCellStyle == null)
                    {
                        defaultPercentCellStyle = new DataGridViewCellStyle();
                        defaultPercentCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        defaultPercentCellStyle.Format = "###0.00 %";
                    }
                    return defaultPercentCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultCenteredCellStyle
            {
                get
                {
                    if (defaultCenteredCellStyle == null)
                    {
                        defaultCenteredCellStyle = new DataGridViewCellStyle();
                        defaultCenteredCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                    return defaultCenteredCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultImageCellStyle
            {
                get
                {
                    if (defaultImageCellStyle == null)
                    {
                        defaultImageCellStyle = new DataGridViewCellStyle();
                        defaultImageCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        defaultImageCellStyle.NullValue = null;
                    }
                    return defaultImageCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultDateCellStyle
            {
                get
                {
                    if (defaultDateCellStyle == null)
                    {
                        defaultDateCellStyle = new DataGridViewCellStyle();
                        defaultDateCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        defaultDateCellStyle.Format = "d";
                    }
                    return defaultDateCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultDateTimeCellStyle
            {
                get
                {
                    if (defaultDateTimeCellStyle == null)
                    {
                        defaultDateTimeCellStyle = new DataGridViewCellStyle();
                        defaultDateTimeCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        defaultDateTimeCellStyle.Format = "g";
                    }
                    return defaultDateTimeCellStyle;
                }
            }

            internal static DataGridViewCellStyle DefaultCurrencyCellStyle
            {
                get
                {
                    if (defaultCurrencyCellStyle == null)
                    {
                        defaultCurrencyCellStyle = new DataGridViewCellStyle();
                        defaultCurrencyCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        defaultCurrencyCellStyle.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                    }
                    return defaultCurrencyCellStyle;
                }
            }

            #endregion

            #region private metódusok

            private void SetDataType(DataType value)
            {
                dataType = value;
                if (value == AdvancedDataGridView.DataType.AnyObject)
                    valueType = typeof(object);
                else
                    valueType = Type.GetType("System." + value.ToString());
            }

            private void SetAppearanceType(AppearanceType value)
            {
                if (appearanceType == value)
                    return;

                appearanceType = value;
                defaultCellStyle = NeutralCellStyle;

                switch (value)
                {
                    case AppearanceType.Text:
                    case AppearanceType.TranslatedText:
                        defaultCellStyle = DefaultLeftCellStyle;
                        break;
                    case AppearanceType.Integer:
                        defaultCellStyle = DefaultIntegerCellStyle;
                        break;
                    case AppearanceType.Float:
                    case AppearanceType.Amount:
                        defaultCellStyle = DefaultFloatCellStyle;
                        break;
                    case AppearanceType.Percent:
                        defaultCellStyle = DefaultPercentCellStyle;
                        break;
                    case AppearanceType.Image:
                    case AppearanceType.Currency:
                        defaultCellStyle = DefaultImageCellStyle;
                        break;
                    case AppearanceType.TranslatedImageText:
                    case AppearanceType.ImageAndText:
                        defaultCellStyle = DefaultCenteredCellStyle;
                        break;
                    case AppearanceType.Link:
                        defaultCellStyle = DefaultLeftCellStyle;
                        break;
                    case AppearanceType.Date:
                        defaultCellStyle = DefaultDateCellStyle;
                        break;
                    case AppearanceType.DateTime:
                        defaultCellStyle = DefaultDateTimeCellStyle;
                        break;
                    case AppearanceType.CaptionedCurrency:
                        defaultCellStyle = DefaultCurrencyCellStyle;
                        break;
                }
            }

            #endregion

            #region konstruktorok

            public Field()
            {
                columnName = "Column" + colIndex.ToString();
                colIndex++;
                width = 100;
            }

            public Field(string columnName)
            {
                ColumnName = columnName;
                width = 100;
            }

            public Field(string columnName, string headerText, FieldType fieldType, DataType dataType, AppearanceType appearanceType)
            {
                ColumnName = columnName;
                HeaderText = headerText;
                FieldType = fieldType;
                DataType = dataType;
                AppearanceType = appearanceType;
                width = 100;
            }

            #endregion

            #region Override-olt metódusok

            public override string ToString()
            {
                return columnName + (!string.IsNullOrEmpty(headerText) ? " {" + headerText + "}" : "") +
                    " - " + appearanceType.ToString() + "/" + dataType.ToString() + " (" + fieldType.ToString() + ")";
            }

            #endregion
        }

        public class FilterAddedEventArgs : EventArgs
        {
            object value;
            int columnIndex;

            public object Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            public int ColumnIndex
            {
                get { return columnIndex; }
                set { columnIndex = value; }
            }

            public FilterAddedEventArgs(object value, int columnIndex)
            {
                this.value = value;
                this.columnIndex = columnIndex;
            }
        }

        /// <summary>
        /// A ContextMenu-ben az FXGrides saját menüpontok ilyen Tag-et kapnak
        /// </summary>
        private class FxGridPopupMenuTag
        {
            public FxGridPopupMenuTag()
            {
            }
        }

        #endregion
    }
}
