using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Globalization;
using KGySoft.Libraries;
using KGySoft.Libraries.Language;

namespace KGySoft.Controls
{

    //#region Namespace szintű típusok (enum, struct) - moved to *.Additional.cs

    //#region Namespace szintű classok - moved to *.Additional.cs

    /// <summary>
    /// A DataGridView-ből származik, így minden tulajdonságával rendelkezik.
    /// Jelenleg a legfontosabb szolgáltatása, hogy automatice bővíti vagy létrehozza a context menu-t, amiben
    /// az export funkciók találhatók! Vágólapra vagy Excel-be tud másolni.
    /// 
    /// MA, 2006.06
    /// --DataTable-es frissítés
    /// --Settings menüpont    
    /// --beépített szűrők
    /// --Checkboxcolumn
    /// --MergeRow képesség
    /// 
    /// KGy, 2007.??.??
    /// - AlternateRows
    /// - Export régióbeállítás függetlenítés
    /// - XML excel export fejlesztések
    /// - Fields: DataTable-ös RefreshGrid-ből jövő tartalom könnyebb formázásához + user által kalkulált mezők támogatása
    /// - PopupMenuBehaviour: Ha a user ad meg ContextMenuStrip-et, azt össze lehessen fésülni a saját menükkel
    /// </summary>
    public partial class AdvancedDataGridView : DataGridView
    {
        #region String constants

        public static string DecimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        const string postfixOfDisplayedID = "DISPLAYED";

        const string captionMenuExportToClipboard = "Export to clipboard";
        const string captionMenuExportToExcel = "Export to Excel";
        const string captionMenuPrint = "Print";

        const string captionMenuSettings = "Settings";
        const string captionMenuAddFilter = "Add to filters";
        const string captionMenuClearFilters = "Clear filters";
        const string captionMenuSelection = "Selection";
        const string captionMenuSelectAll = "All";
        const string captionMenuSelectNone = "None";
        const string captionMenuSelectInvert = "Invert";
        const string captionSelectionColumn = "Sel.";

		const string formatDelimiter = "__";

        #endregion

        //#region Típusok (Enum, Struct, Class) - moved to *.Additional.cs

        #region Objektumváltozók

        DataGridViewCheckBoxColumn SelectionColumn;
        ToolStripDropDownButton mnuSelection;
        ToolStripTextBox txtSelected;
        ToolStripMenuItem mnuExport;
        ToolStripMenuItem mnuExportToExcel;
        ToolStripMenuItem mnuSettings;
        ToolStripMenuItem mnuAddFilter;
        ToolStripMenuItem mnuClearFilters;
        ToolStripMenuItem mnuSelectionAll;
        ToolStripMenuItem mnuSelectionInvert;
        ToolStripMenuItem mnuSelectionNone;
        ToolStripMenuItem mnuPrint; // SM, 2006.08.31    

        private FieldsCollection fields = new FieldsCollection(); // mezőlista a RefreshGrid-es generáláshoz

        object ClickedCellValue;
        int ClickedCellColumnIndex;

        bool mergeEventsSet = false;
        List<int> rowsToMerge = new List<int>();

        ContextMenuStrip context; // az aktualizált popup menülista
        private PopupMenuBehaviours popupMenuBehaviour = PopupMenuBehaviours.MergeUserWithDefault;

        private bool alternatingRows = true;
        private DataGridViewCellStyle storedAlternatingStyle;
        private DataGridViewCellStyle defaultRowStyle = new DataGridViewCellStyle();
        private DataGridViewCellStyle defaultAlternatingStyle = new DataGridViewCellStyle();


        bool hasSelection = false;
        bool hasFilters = false;
        Dictionary<int, object> FilterDictionary;
        protected bool showSelectedCellValue = true;
        bool formatCaptions = true;
        Color amountNegativeForeColor = Color.Red;
        decimal amountThresholdValue = 0;
        bool showSettingsMenu = true;
        bool showPrintMenu = true;

        #endregion

        #region Delegate-ek

        public delegate void CalculateFieldDelegate(object sender, CalculateFieldEventArgs e);
        public delegate void FieldCellClickDelegate(object sender, FieldCellClickEventArgs e);
        public delegate void FieldCellFormattingDelegate(object sender, FieldCellFormattingEventArgs e);
        public delegate void BeforeGenerateColumnDelegate(object sender, BeforeGenerateColumnEventArgs e);

        public delegate void SettingsClickedDelegate(object sender);
        protected delegate void FiltersClearedDelagate(object sender);
        protected delegate void FilterAddedDelagate(object sender, FilterAddedEventArgs e);

        #endregion

        #region Az osztályban definiált események

        public event SettingsClickedDelegate SettingsClicked;
        protected event FiltersClearedDelagate FiltersCleared;
        protected event FilterAddedDelagate FilterAdded;

        /// <summary>
        /// print menu click handler
        /// </summary>
        protected event EventHandler PrintMenuClick;

        /// <summary>
        /// Ha vettünk fel kliens oldalon számítandó Calculated Fieldeket a gridhez, a RefreshGrid hívásra
        /// ez az esemény lefut a kiszámítandó mezőkre
        /// </summary>
        [Category("AdvancedDataGridView")]
        [Description("Ha vettünk fel kliens oldalon számítandó CalcFieldeket a gridhez, a RefreshGrid hívásra ez az esemény lefut minden kiszámítandó mezőre")]
        public event CalculateFieldDelegate CalculateField;

        [Category("AdvancedDataGridView")]
        [Description("Ha RefreshGrid-es feltöltéssel létrejött cellába kattintunk, akkor ez lefut (használható a sima CellClick is, de ebben több infó van)")]
        public event FieldCellClickDelegate CellClicked;

        [Category("AdvancedDataGridView")]
        [Description("Ha RefreshGrid-es feltöltéssel létrejött cellába kattintunk, akkor ez lefut (használható a sima CellDoubleClick is, de ebben több infó van)")]
        public event FieldCellClickDelegate CellDoubleClicked;

        [Category("AdvancedDataGridView")]
        [Description("Ha RefreshGrid-es feltöltéssel létrejött link cellába kattintunk, akkor ez lefut (használható a sima CellContentClick is, de ebben több infó van)")]
        public event FieldCellClickDelegate LinkCellClicked;

        [Category("AdvancedDataGridView")]
        [Description("A RefreshGrid-es feltöltéssel létrejött cellákra ez lefut, amikor egy cellát kell formázni (használható a sima CellFormatting is, de ebben több infó van)")]
        public event FieldCellFormattingDelegate FormatCell;

        [Category("AdvancedDataGridView")]
        [Description("A RefreshGrid-es feltöltésnél az oszlopok generálása előtt fut le. Üres Fields lista esetén érdemes használni az automatikusan generált Fields infók finomhangolásához.")]
        public event BeforeGenerateColumnDelegate BeforeGenerateColumn;

        #endregion

        #region Properties

        /// <summary>
        /// Alternáló színű sorok legyenek-e
        /// </summary>
        [Category("AdvancedDataGridView")]
        [Description("Alternáló színű sorok legyenek-e")]
        [DefaultValue(true)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [RefreshProperties(RefreshProperties.All)]
        public bool AlternatingRows
        {
            get { return alternatingRows; }
            set
            {
                alternatingRows = value;
                if (!value) // elmentjük a régi alternáló stílust, és beállítjuk egységesre a dolgot
                {
                    storedAlternatingStyle = base.AlternatingRowsDefaultCellStyle;
                    AlternatingRowsDefaultCellStyle = RowsDefaultCellStyle;
                }
                else if (storedAlternatingStyle != null) // elmentett stílus visszaállítása
                {
                    AlternatingRowsDefaultCellStyle = storedAlternatingStyle;
                }
                else if (RowsDefaultCellStyle == AlternatingRowsDefaultCellStyle && RowsDefaultCellStyle.Equals(defaultRowStyle))
                {
                    AlternatingRowsDefaultCellStyle = defaultAlternatingStyle;
                }
            }
        }

        public new DataGridViewCellStyle AlternatingRowsDefaultCellStyle
        {
            get
            {
                if (alternatingRows)
                    return base.AlternatingRowsDefaultCellStyle;
                else
                {
                    base.AlternatingRowsDefaultCellStyle = RowsDefaultCellStyle;
                    return RowsDefaultCellStyle;
                }
            }
            set
            {
                if (!alternatingRows)
                {
                    storedAlternatingStyle = base.AlternatingRowsDefaultCellStyle;
                    base.AlternatingRowsDefaultCellStyle = RowsDefaultCellStyle;
                }
                else
                    base.AlternatingRowsDefaultCellStyle = value;
            }
        }

        /// <summary>
        /// Azok a mezők, amik DataTable-ös RefreshGrid jelenjenek meg a gridben (ha üres, automatikus mezőgenerálás lesz).
        /// A kalkulált mezők értékeinek kitöltéséhez implementáljuk a CalculateField eseményt.
        /// Speciális cellaformázáshoz használjuk a FieldCellFormatting eseményt
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        [Browsable(true)]
        [Category("AdvancedDataGridView")]
        [Description("A megjelenő oszlopok RefreshGrid(DataTable) jellegű feltöltésnél. Ha üres a Fields, automatikus a feltöltés.")]
        //[Editor("???", typeof(System.Drawing.Design.UITypeEditor))]
        public FieldsCollection Fields
        {
            get { return fields; }
            set
            {
                if (value == null)
                    fields = new FieldsCollection();
                else
                    fields = value;
                Field.colIndex = 0;
            }
        }

        [Category("AdvancedDataGridView")]
        [Description("Oszlopok gyors felvétele (pl. Query Analyzerből való bemásolás után). DESINGMODE-ONLY PROPERTY!")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FieldsQuickAssigner
        {
            get { return ""; }
            set
            {
                if (!DesignMode)
                    throw new Exception("Using of this property is allowed only in DesignMode!");

                switch (Dialogs.ConfirmMessage(true, "Do you want to clear original Fields list before you add new fields?"))
                {
                    case DialogResult.Cancel: return;
                    case DialogResult.Yes: Fields.Clear(); break;
                }

                while (value.Contains("  "))
                    value = value.Replace("  ", " ");
                string[] list = value.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in list)
                    Fields.Add(new Field(item));

                Dialogs.InfoMessage("Fields have been added:{0}{1}", Environment.NewLine, value.Replace(" ", Environment.NewLine));
            }
        }

        [Category("AdvancedDataGridView")]
        [Description("Hogy viselkedjenek a felhasználó által definiált menük")]
        [DefaultValue(typeof(PopupMenuBehaviours), "MergeUserWithDefault")]
        public PopupMenuBehaviours PopupMenuBehaviour
        {
            get { return popupMenuBehaviour; }
            set { popupMenuBehaviour = value; }
        }

        [Category("AdvancedDataGridView")]
        [Description("Felhasználó által definiált menük. Lásd még a PopupMenuBehaviour propertyt!")]
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return context;
            }
            set
            {
                context = value;
                BuildMenu();
            }
        }

        [
        Category("AdvancedDataGridView"),
        DefaultValue(true),
        Description("Az egér alatt lévő cella tartalma megjelenik a ContextMenu tetején egy TextBoxban.")
        ]
        public bool ShowSelectedCellValue
        {
            get { return showSelectedCellValue; }
            set
            {
                if (showSelectedCellValue != value)
                {
                    showSelectedCellValue = value;
                    BuildMenu();
                }
            }
        }

        [
        Category("AdvancedDataGridView"),
        DefaultValue(true),
        Description("Lehetőség van griden belüli szűrésre, az adott mezőre ráklikkelve.")
        ]
        public bool HasFilters
        {
            get { return hasFilters; }
            set
            {
                if (value != hasFilters)
                {
                    hasFilters = value;
                    BuildMenu();
                    if (value)
                        FilterDictionary = new Dictionary<int, object>();
                }
            }
        }

        [
        Category("AdvancedDataGridView"),
        DefaultValue(true),
        Description("A ContextMenu tartalmazza a Settings menüpontot.")
        ]
        public bool ShowSettingsMenu
        {
            get { return showSettingsMenu; }
            set
            {
                if (showSettingsMenu != value)
                {
                    showSettingsMenu = value;
                    BuildMenu();
                }
            }
        }

        [
        Category("AdvancedDataGridView"),
        DefaultValue(true),
        Description("A ContextMenu tartalmazza a Print menüpontot.")
        ]
        public bool ShowPrintMenu
        {
            get { return showPrintMenu; }
            set
            {
                if (showPrintMenu != value)
                {
                    showPrintMenu = value;
                    BuildMenu();
                }
            }
        }

        [
        DefaultValue(false),
        Category("AdvancedDataGridView"),
        Description("A legelső oszlopban megadható, hogy melyik sorok legyenek kiválasztva (Csak autogenerate modban mukodik)")
        ]
        public bool HasSelection
        {
            get
            {
                return hasSelection;
            }
            set
            {
                if (hasSelection == value)
                    return;

                hasSelection = value;
                BuildMenu();

                if (value && SelectionColumn == null)
                {

                    SelectionColumn = new DataGridViewCheckBoxColumn();
                    SelectionColumn.HeaderText = Language.Translate(captionSelectionColumn);
                    SelectionColumn.Frozen = true;
                    SelectionColumn.Width = 30;
                }

            }
        }

        [
        DefaultValue(true),
        Category("AdvancedDataGridView"),
        Description("A DataTable-ből vett oszlopneveket megformázza, ha nincsenek megadva az oszlopnevek")
        ]
        public bool FormatCaptions
        {
            get
            {
                return formatCaptions;
            }
            set
            {
                formatCaptions = value;
            }
        }

        [
        Category("AdvancedDataGridView"),
        Description("Az AmountThresholdValue értéknél kisebb értékű Money típusú oszlopok celláinak ForeColorját erre a színre állítja")
        ]
        public Color AmountNegativeForeColor
        {
            get
            {
                return amountNegativeForeColor;
            }
            set
            {
                amountNegativeForeColor = value;
            }
        }

        [
        DefaultValue(0),
        Category("AdvancedDataGridView"),
        Description("Az ennél kisebb értékű Money típusú oszlopok celláinak ForeColorját AmountNegativeForeColor színre állítjuk")
        ]
        public decimal AmountThresholdValue
        {
            get
            {
                return amountThresholdValue;
            }
            set
            {
                amountThresholdValue = value;
            }
        }

        public bool IsCheckedRow(int i)
        {
            if (i >= this.RowCount || i < 0 || !(hasSelection))
            {
                return false;
            }

            if (this[SelectionColumn.Index, i].Value == null)
                return false;

            return ((bool)(this[SelectionColumn.Index, i].Value));
        }

        [DefaultValue(null)] // felüldefiniálás, hogy mindig mentsen file-ba
        public new bool MultiSelect
        {
            get { return base.MultiSelect; }
            set { base.MultiSelect = value; }
        }

        [DefaultValue(null)] // felüldefiniálás, hogy mindig mentsen file-ba
        public new DataGridViewSelectionMode SelectionMode
        {
            get { return base.SelectionMode; }
            set { base.SelectionMode = value; }
        }

        #endregion

        #region Publikus metódusok

        /// <summary>
        /// 
        /// SM, 2006.07.17
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsRowMerged(int index)
        {
            return rowsToMerge.Contains(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="merge"></param>
        public void MergeRows(int rowIndex, bool merge)
        {
            if ((this.RowCount) <= rowIndex || rowIndex < 0)
                return;

            if (merge)
            {
                SetMergeEvents(true);

                this.AllowUserToOrderColumns = false;

                if (!rowsToMerge.Contains(rowIndex))
                    rowsToMerge.Add(rowIndex);
            }
            else
            {
                if (rowsToMerge.Contains(rowIndex))
                    rowsToMerge.Remove(rowIndex);

                if (rowsToMerge.Count == 0)
                    SetMergeEvents(false);
            }
        }

        public bool Locate(DataGridViewColumn column, object value)
        {
            int? found = null;
            for (int i = 0; i < this.RowCount; i++)
                if (Equals(this[column.Index, i].Value, value))
                {
                    found = i;
                    break;
                }
            if (this.RowCount > 0 && found != null)
            {
                this.ClearSelection();
                base.FirstDisplayedScrollingRowIndex = found.Value;
                switch (SelectionMode)
                {
                    case DataGridViewSelectionMode.CellSelect:
                        this[column.Index, found.Value].Selected = true;
                        break;
                    case DataGridViewSelectionMode.FullRowSelect:
                        this.Rows[found.Value].Selected = true;
                        break;
                }
            }
            return found != null;
        }

        public bool Locate(string columnName, object value)
        {
            return Locate(this.Columns[columnName], value);
        }

        public bool Locate(int columnIndex, object value)
        {
            return Locate(this.Columns[columnIndex], value);
        }

        /// <summary>
        /// A paraméterben megadott sorszámú oszlop rejtett-e (nem visible, hanem oszlop definíció szerint)
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public bool IsHiddenColumn(int columnIndex)
        {

            if (!(this.Columns[columnIndex].Tag is ColumnTag))
                return false;

            return ((ColumnTag)(this.Columns[columnIndex].Tag)).ActualAppearanceType == AppearanceType.Hidden;
        }

        /// <summary>
        /// Megnézi, hogy szám van-e a cellában, és ha igen, akkor true-val tér vissza, valamint megparsolja
        /// </summary>
        public static bool TryParseCellNumber(out double number, string cellvalue)
        {
            number = 0;

            //Szám van-e az adott cellában? Ha számtípusú, akkor biztos - Kiszedve by KGy: lehet, hogy a datasource egy int mező, de az lehet ID is, ami egy nevet jelenít meg
            //bool isNumber = Common.IsNumberType(cell.ValueType);

            //Vagy ha lehet parsolni számmá
            bool isNumber = false;
            number = Utils.ToDouble(out isNumber, cellvalue);

            return isNumber;
        }

        /// <summary>
        /// Export to clipboard
        /// </summary>
        public void ExportToClipboard(bool onlyVisible, bool needHeader)
        {
            string origDecimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            try
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                ci.NumberFormat = (NumberFormatInfo)Thread.CurrentThread.CurrentCulture.NumberFormat.Clone();
                ci.NumberFormat.NumberDecimalSeparator = DecimalSeparator;
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;

                StringBuilder sb = new StringBuilder();

                string headerline = "";
                foreach (DataGridViewColumn col in this.Columns)
                {
                    if (onlyVisible)
                    {
                        if (col.Visible) headerline += col.HeaderText + "\t";
                    }
                    else
                    {
                        headerline += col.HeaderText + "\t";
                    }
                }

                if (needHeader)
                {
                    headerline = headerline.Substring(0, headerline.Length - 1); // utolsó tab levágása
                    sb.AppendLine(headerline);
                }

                int lines = 0;
                double numberValue;

                foreach (DataGridViewRow row in this.Rows)
                {
                    string line = "";
                    string lastValue = "";
                    bool merged = this.rowsToMerge.Contains(row.Index);
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string cellvalue;
                        if (cell is DataGridViewImageCell) // kép cella esetén a FormattedValue csak "System.Drawing.Image" lenne
                            cellvalue = (cell.Value == null ? "" : cell.Value.ToString());
                        else
                            cellvalue = (cell.FormattedValue == null ? "" : cell.FormattedValue.ToString());
                        cellvalue = Utils.RemoveControlCharacters(cellvalue).Trim();
                        if (TryParseCellNumber(out numberValue, cellvalue))
                        {
                            cellvalue = numberValue.ToString(System.Globalization.CultureInfo.CurrentCulture);
                            lastValue = ""; // csak stringnél fontos
                        }
                        else
                        {
                            if (merged && cellvalue == lastValue)
                                cellvalue = ""; // merged celláknál az egymás után következő egyforma string értékeket nem exportáljuk
                            else
                                lastValue = cellvalue;
                        }
                        if (onlyVisible)
                        {
                            if (cell.Visible) line += cellvalue + "\t";
                        }
                        else
                        {
                            line += cellvalue + "\t";
                        }
                    }
                    line = line.Substring(0, line.Length - 1); // utolsó tab levágása
                    sb.AppendLine(line.TrimEnd(new char[] { '\t' }));
                    lines++;
                }

                Clipboard.SetText(sb.ToString());

                MessageBox.Show("Data exported to clipoard! [" + lines.ToString() + " lines]", "Grid Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                ci.NumberFormat = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat;
                ci.NumberFormat.NumberDecimalSeparator = origDecimalSeparator;
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            }
        }

        /// <summary>
        /// Lefordítja az oszlopok fejléceit. RefreshGrid-es töltés esetén ezt nem kell használni, mert ott automatikusan megtörténik, csak akkor kell használni, ha DataSource-os feltöltés van
        /// </summary>
        public void TranslateColumnHeaders()
        {
            foreach (DataGridViewColumn c in Columns)
            {
                if (c.Visible)
                    c.HeaderText = Language.Translate(c.HeaderText);
            }
        }

        /// <summary>
        /// Gridet készít business-objektum listából
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        public void RefreshGrid<T>(List<T> list)
        {
            this.DataSource = null;
            this.DataSource = list.ToArray();
            this.TranslateColumnHeaders();
            int displayIndex = 0;

            //Rendezi a property-ket definiciójuk sorrendjében, mert egyébként ABC sorrendben lennének
            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                if (this.Columns.Contains(pi.Name))
                    this.Columns[pi.Name].DisplayIndex = displayIndex++;
            }
        }

        /// <summary>
        /// Feltölti a gridet a DataTable értékeivel, oszlopaival. Formázás lehetséges az oszlop nevében is,
        /// ha nem töltjük ki a Fields property-t. Ez esetben a formázás:
        ///     [Oszlopnév]__[Formázó sztring] (két aláhúzás közte)
        /// [Formázó sztring]::=
        ///     trans: lefordítja az oszlop tartalmát
        ///     link: link jelenik meg a cellákban
        ///     drop!: nem jelenik meg a gridben az oszlop
        ///     Amount: tördel 3-as bontásban
        ///     datetime: az időt is kiírja
        ///     Currency: devizanem oszlop, lesz benne zászlócska (ezt akkor is megteszi, ha az [Oszlopnév] tartalmazza a Currency vagy Devizanem szöveget )
        ///     Ticket: lesz benne Ticket link és klikkeljéskor kiváltja a ClickedOnTicket eseményt (ezt akkor is megteszi, ha az [Oszlopnév] tartalmazza a Ticket szöveget)
        ///     id vagy hidden: ID oszlop lesz, tehát nem fog látszódni, viszont benne lesz a gridben (ellentétben a drop!-pal)
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public virtual void RefreshGrid(DataTable dt)
        {
            this.AutoGenerateColumns = false;

            this.DataSource = null;
            this.Columns.Clear();

            try
            {
                this.Paint += new PaintEventHandler(AdvancedDataGridView_Paint);
                this.Refresh(); // invalidate nem elég, mert úgy sem fog "levegőhöz jutni" frissítés alatt a grid
                this.SuspendLayout();
                if (dt == null)
                    return;

                if (hasSelection)
                    this.Columns.Add(SelectionColumn);

                // minden oszlopot automatikusan generálunk a DataTable oszlopai szerint
                if (fields.Count == 0)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                        AutoGenerateColumn(dt.Columns[i]);
                }
                // a felvett Fields lista alapján generálunk
                else
                {
                    foreach (Field f in fields)
                    {
                        Debug.Assert(f.ColumnName != "");

                        if (f.FieldType == FieldType.Calculated)
                            GenerateCalcField(f, dt);

                        if (!dt.Columns.Contains(f.ColumnName))
                            throw new InvalidOperationException("There is no column named '" + f.ColumnName + "' in DataSource");

                        GenerateColumn(f, dt.Columns[f.ColumnName]);
                    }
                    FillCalcFields(dt);
                }

                this.DataSource = dt;
                this.ClearFilters();
                TranslateFields();
            }
            finally
            {
                this.ResumeLayout();
                Paint -= AdvancedDataGridView_Paint;
            }
        }

        void AdvancedDataGridView_Paint(object sender, PaintEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            e.Graphics.FillRectangle(new SolidBrush(BackgroundColor), e.ClipRectangle);
            SolidBrush drawBrush = new SolidBrush(Color.FromName("ControlText"));
            e.Graphics.DrawString(Language.Translate("Refreshing grid..."), Font, drawBrush, e.ClipRectangle, sf);
        }

        /// <summary>
        /// DataTable-ös Grid feltöltés utolsó SORSZÁMÚ aktív sorra ugrással. Lehetséges formázó postfixeket lásd a RefreshGrid(DataTable dt) overload-olt változatnál.
        /// </summary>
        /// <param name="dt">Forrás adattábla (megjelenítés szabályozásához Fields property vagy formázó postfixek használhatók)</param>
        /// <param name="withRelocateRow">Frissítés után legyen-e ugrás a frissítés előtti SORSZÁMÚ sorra</param>
        public void RefreshGrid(DataTable dt, bool withRelocateRow)
        {
            int row = -1;

            //if (this.SelectedRows.Count == 0)
            if (this.Rows.Count>0)
             row = this.FirstDisplayedCell.RowIndex;
            //else
              //  row = this.SelectedRows[0].Index;

            RefreshGrid(dt);

            if (withRelocateRow)
            {
                // ha nincs annyi sor, az alsóra ugrunk
                if (this.RowCount <= row)
                    row = RowCount - 1;

                this.ClearSelection();
                if (row >= 0)
                {
                    base.FirstDisplayedScrollingRowIndex = row;
                 
                }


            }
        }

        /// <summary>
        /// DataTable-ös Grid feltöltés, utána meg a frissítés előtti sor kikeresése a megadott kulcs oszlop utolsó aktív értéke alapján. Lehetséges formázó postfixeket lásd a RefreshGrid(DataTable dt) overload-olt változatnál.
        /// </summary>
        /// <param name="dt">Forrás adattábla (megjelenítés szabályozásához Fields property vagy formázó postfixek használhatók)</param>
        /// <param name="keyFieldToLocate">Ennek az oszlopnak az utolsó aktív értékét kerssük ki frissítés után.</param>
        public void RefreshGrid(DataTable dt, string keyFieldToLocate)
        {
            object keyValue = null;

            if (this.SelectedRows.Count != 0)
                keyValue = this[keyFieldToLocate, SelectedRows[0].Index].Value;
            else if (this.SelectedCells.Count != 0)
                keyValue = this[keyFieldToLocate, SelectedCells[0].RowIndex].Value;

            RefreshGrid(dt);
            if (keyValue != null)
                Locate(keyFieldToLocate, keyValue);
        }

        /// <summary>
        /// DataTable-ös Grid feltöltés, utána meg az adott érték kikeresése az dott oszlopból. Lehetséges formázó postfixeket lásd a RefreshGrid(DataTable dt) overload-olt változatnál.
        /// </summary>
        /// <param name="dt">Forrás adattábla (megjelenítés szabályozásához Fields property vagy formázó postfixek használhatók)</param>
        /// <param name="keyFieldToLocate">Ennek az oszlopnak az utolsó aktív értékét kerssük ki frissítés után.</param>
        /// <param name="keyValueToLocate">Ennek az oszlopnak az utolsó aktív értékét kerssük ki frissítés után.</param>
        public void RefreshGrid(DataTable dt, string keyFieldToLocate, object keyValueToLocate)
        {
            RefreshGrid(dt);
            if (keyValueToLocate != null)
                Locate(keyFieldToLocate, keyValueToLocate);
        }




        public void SetRowsSelection(ItemSelectionMode selectionMode)
        {
            if (!(hasSelection))
            {
                return;
            }
            for (int i = 0; i < this.RowCount; i++)
            {
                if (!(this.Rows[i].Visible))
                {
                    this[SelectionColumn.Index, i].Value = false;
                }
                else
                {
                    switch (selectionMode)
                    {
                        case ItemSelectionMode.All:
                            this[SelectionColumn.Index, i].Value = true;
                            break;

                        case ItemSelectionMode.None:
                            this[SelectionColumn.Index, i].Value = false;
                            break;

                        case ItemSelectionMode.Invert:
                            if (this[SelectionColumn.Index, i].Value == null)
                                this[SelectionColumn.Index, i].Value = true;
                            else
                                this[SelectionColumn.Index, i].Value = !((bool)this[SelectionColumn.Index, i].Value);
                            break;
                    }
                }
            }
        }

        public void ExportToExcel()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel file (xls)|*.xls";
            sfd.Title = "Save grid contents to Excel...";
            sfd.DefaultExt = "xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExcelXMLDataGridViewExporter exp = new ExcelXMLDataGridViewExporter();
                exp.AddGrid(this, DateTime.Now.ToString("exported yyyyMMdd HHmmss"));
                exp.ExportExcelFile(sfd.FileName);
            }
        }

        #endregion

        #region Protected metódusok

        protected void ClearFilters()
        {
            if (FilterDictionary == null || !hasFilters)
                return;

            FilterDictionary.Clear();
            MarkFilteredColumns();
            FilterRows();

            if (FiltersCleared != null)
                FiltersCleared(this);
        }

        protected void MarkFilteredColumns()
        {
            for (int i = 0; i < this.ColumnCount; i++)
            {
                if (FilterDictionary.ContainsKey(i))
                    this.Columns[i].HeaderCell.Style.Font = new System.Drawing.Font(this.Columns[i].HeaderCell.InheritedStyle.Font, System.Drawing.FontStyle.Underline);
                else
                    this.Columns[i].HeaderCell.Style = null;
            }
        }

        protected void FilterRows()
        {
            if (FilterDictionary == null || !hasFilters)
                return;

            for (int i = 0; i < this.RowCount; i++)
            {
                this.Rows[i].Visible = true;
                if (this.SelectedRows.Contains(Rows[i]))
                    continue;

                for (int j = 0; j < this.ColumnCount; j++)
                    if (FilterDictionary.ContainsKey(j) && !Equals(this[j, i].Value, FilterDictionary[j]))
                        try
                        {
                            this.Rows[i].Visible = false;
                        }
                        catch
                        {
                            //Row associated with the currency manager's position cannot be made invisible
                        }
            }
        }

        protected void BuildMenu()
        {
            if (DesignMode)
                return;

            // Fontos: NEM saját contextmenüt buildelünk, hozzáadva a User saját menüjét, mert az Add metódus
            // sajnos leveszi a menüpontokat az eredeti menüről, miközben a saját menühöz hozzáadogatjuk őket.

            // rebuild előtt eltávolítjuk a saját menüpontjainkat
            if (context != null)
            {
                int i = 0;
                while (i < context.Items.Count)
                {
                    if (context.Items[i].Tag is FxGridPopupMenuTag)
                        context.Items.RemoveAt(i);
                    else i++;
                }
                if (context.Items.Count > 0 && popupMenuBehaviour == PopupMenuBehaviours.MergeUserWithDefault)
                {
                    context.Items.Add(GetNewToolStripSeparator());
                }
            }
            else
            {
                context = new ContextMenuStrip();
            }

            if (popupMenuBehaviour == PopupMenuBehaviours.AlwaysUserMenu ||
                (context.Items.Count > 0 && popupMenuBehaviour == PopupMenuBehaviours.DefaultWhenNoUserMenu))
            {
                // A user menüjét jelenítjük meg default menüpontok nélkül
                return;
            }
            else
            {
                // default menüpontok beszúrása

                // ezek az elejére
                if (showSelectedCellValue)
                {
                    context.Items.Insert(0, txtSelected);
                    context.Items.Insert(1, GetNewToolStripSeparator());

                    this.CellMouseDown += new DataGridViewCellMouseEventHandler(AdvancedDataGridView_CellMouseDown);
                }

                // a többi a végére
                if (hasFilters)
                {
                    context.Items.Add(mnuAddFilter);
                    context.Items.Add(mnuClearFilters);
                    context.Items.Add(GetNewToolStripSeparator());
                }

                context.Items.Add(mnuExport);
                context.Items.Add(mnuExportToExcel);

                if (showPrintMenu)
                {
                    context.Items.Add(mnuPrint);
                }

                if (hasSelection)
                {
                    context.Items.Add(GetNewToolStripSeparator());
                    context.Items.Add(mnuSelection);
                }

                if (showSettingsMenu)
                {
                    context.Items.Add(GetNewToolStripSeparator());
                    context.Items.Add(mnuSettings);
                }
            }
        }

        /// <summary>
        /// Auto AppearanceType esetén eldönti az oszlop típusát.
        /// Elsősorban formázó postfix, másodsorban a forrás adattípusa alapján
        /// </summary>
        protected virtual AppearanceType GetActualAppearanceType(Field f, DataColumn col)
        {
            Debug.Assert(f.AppearanceType == AppearanceType.Auto); // Ennek a hívásnak csak auto formázás esetén van értelme
            Debug.Assert(f.ColumnName != "");

            string format = GetFormat(f.ColumnName);
            string caption = GetCaption(f.ColumnName);

            // döntés formázó postfix alapján (használata már ellenjavallt)
            if (format != "")
            {
                switch (format)
                {
                    case "id":
                    case "hidden":
                        return AppearanceType.Hidden;
                    case "trans":
                        return AppearanceType.TranslatedText;
                    case "link":
                        return AppearanceType.Link;
                    case "amount":
                        return AppearanceType.Amount;
                    case "datetime":
                        return AppearanceType.DateTime;
                    case "date":
                        return AppearanceType.Date;
                    case "currency":
                        return AppearanceType.CaptionedCurrency;
                }
            }

            // ha a postfix ismeretlen volt, vagy nem volt megadva, adattípus szerint döntünk
            // Lehetséges adattípusok: ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.en/cpref4/html/P_System_Data_DataColumn_DataType.htm
            switch (col.DataType.Name)
            {
                case "Boolean":
                    return AppearanceType.Checkbox;
                case "Byte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "SByte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return AppearanceType.Integer;
                case "Single":
                case "Double":
                    return AppearanceType.Float;
                case "Decimal":
                    return AppearanceType.Amount;
                case "DateTime":
                    return AppearanceType.Date;
                default:
                    return AppearanceType.Text;
            }
        }

        protected string GetFormat(string colName)
        {
            if (!colName.Contains(formatDelimiter))
                return "";

			int ix = colName.IndexOf(formatDelimiter);

			return colName.Substring(ix + formatDelimiter.Length).ToLower();
        }

        protected string GetCaption(string colName)
        {
			if (!colName.Contains(formatDelimiter))
                return colName;

			int ix = colName.IndexOf(formatDelimiter);

            return colName.Substring(0, ix);
        }

        #endregion

        #region Private metódusok

        /// <summary>
        /// Calculated field hozzáadása
        /// </summary>
        private void GenerateCalcField(Field f, DataTable dt)
        {
            dt.Columns.Add(f.ColumnName, f.ValueType);
        }

        /// <summary>
        /// A CalculatedField-ek kitöltése
        /// </summary>
        private void FillCalcFields(DataTable dt)
        {
            if (CalculateField != null)
            {
                foreach (Field f in fields)
                {
                    Debug.Assert(dt.Columns.Contains(f.ColumnName), "A FillCalcFields hívásakor már igaznak kell lennie, hogy Fields minden mezője benne van a forrás DataTable-ben!");

                    if (f.FieldType == FieldType.Calculated)
                    {
                        int colIndex = dt.Columns.IndexOf(f.ColumnName);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            CalculateFieldEventArgs e = new CalculateFieldEventArgs(f, dt, i, colIndex);
                            CalculateField(this, e);
                            dt.Rows[i][colIndex] = e.CellValue ?? DBNull.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A grid-beli oszlop megjelenésének beállítása
        /// </summary>
        private void SetColumnStyle(ref DataGridViewColumn col, ColumnTag field)
        {
            Debug.Assert(field.ActualAppearanceType != AppearanceType.Auto);

            if (!field.FieldData.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                col.DefaultCellStyle = new DataGridViewCellStyle(field.FieldData.DefaultCellStyle);

            if (field.ActualAppearanceType == AppearanceType.Hidden)
            {
                col.CellTemplate = new DataGridViewTextBoxCell();
                col.Visible = false;
                return;
            }

            col.SortMode = DataGridViewColumnSortMode.Automatic;
            if (field.FieldData.DisplayIndex >= 0)
                col.DisplayIndex = field.FieldData.DisplayIndex;
            col.AutoSizeMode = (DataGridViewAutoSizeColumnMode)field.FieldData.AutoSizeMode;
            col.Width = field.FieldData.Width;
            col.Resizable = DataGridViewTriState.True;
            col.Resizable = field.FieldData.Resizable;

            // Az alábbiakban azért van CellStyle állítás, mert AutoGenerated esetben nem volt cellstyle állítás a designerben
            switch (field.ActualAppearanceType)
            {
                case AppearanceType.Text:
                case AppearanceType.TranslatedText:
                    col.CellTemplate = new DataGridViewTextBoxCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultLeftCellStyle);
                    break;
                case AppearanceType.Integer:
                    col.CellTemplate = new DataGridViewTextBoxCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultIntegerCellStyle);
                    break;
                case AppearanceType.Float:
                case AppearanceType.Amount:
                    col.CellTemplate = new DataGridViewTextBoxCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultFloatCellStyle);
                    break;
                case AppearanceType.Percent:
                    col.CellTemplate = new DataGridViewTextBoxCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultPercentCellStyle);
                    break;
                case AppearanceType.Checkbox:
                    col.CellTemplate = new DataGridViewCheckBoxCell();
                    break;
                case AppearanceType.Image:
                case AppearanceType.Currency:
                    col.CellTemplate = new DataGridViewImageCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultCenteredCellStyle);
                    break;
                case AppearanceType.TranslatedImageText:
                case AppearanceType.ImageAndText:
                    col.CellTemplate = new DataGridViewTextAndImageCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultCenteredCellStyle);
                    break;
                case AppearanceType.Link:
                    col.CellTemplate = new DataGridViewLinkCell();
                    (col.CellTemplate as DataGridViewLinkCell).LinkBehavior = LinkBehavior.HoverUnderline;
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultLeftCellStyle);
                    break;
                case AppearanceType.Date:
                    col.CellTemplate = new CalendarCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultDateCellStyle);
                    break;
                case AppearanceType.DateTime:
                    col.CellTemplate = new DataGridViewTextBoxCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultDateTimeCellStyle);
                    break;
                case AppearanceType.CaptionedCurrency:
                    col.CellTemplate = new DataGridViewTextAndImageCell();
                    if (col.DefaultCellStyle.Equals(Field.NeutralCellStyle))
                        col.DefaultCellStyle = new DataGridViewCellStyle(Field.DefaultCurrencyCellStyle);
                    break;
            }
        }

        private void TranslateFields()
        {
            if (RowCount <= 0)
                return;

            bool saveReadOnly;

            for (int col = 0; col < this.Columns.Count; col++)
            {
                if (!(Columns[col].Tag is ColumnTag))
                    continue;
                ColumnTag tag = (ColumnTag)Columns[col].Tag;

                if (!Utils.InSet(tag.ActualAppearanceType, AppearanceType.TranslatedText, AppearanceType.TranslatedImageText))
                    continue;

                saveReadOnly = Columns[col].ReadOnly;
                Columns[col].ReadOnly = false;

                for (int row = 0; row < RowCount; row++)
                {
                    this[col, row].Value = Language.Translate(this[col, row].Value.ToString());
                }

                Columns[col].ReadOnly = saveReadOnly;
            }
        }

        private void SelectAllRows(object sender, EventArgs e)
        {
            SetRowsSelection(ItemSelectionMode.All);
        }

        private void SelectNoRows(object sender, EventArgs e)
        {
            SetRowsSelection(ItemSelectionMode.None);
        }

        private void SelectInvertRows(object sender, EventArgs e)
        {
            SetRowsSelection(ItemSelectionMode.Invert);
        }

        private ToolStripItem GetNewToolStripSeparator()
        {
            ToolStripSeparator sep = new ToolStripSeparator();
            sep.Tag = new FxGridPopupMenuTag();
            return sep;
        }

        /// <summary>
        /// Oszlop generálása Field definíció nélkül
        /// </summary>
        private void AutoGenerateColumn(DataColumn col)
        {
            Field f = new Field(col.ColumnName, "", FieldType.Data, DataType.AnyObject, AppearanceType.Auto);
            GenerateColumn(f, col);
        }

        /// <summary>
        /// Oszlop generálása Field definíció alapján
        /// </summary>
        private void GenerateColumn(Field f, DataColumn col)
        {
            if (BeforeGenerateColumn != null)
                BeforeGenerateColumn(this, new BeforeGenerateColumnEventArgs(col, f));

            AppearanceType actualAppearanceType = f.AppearanceType; // a tényleges oszloptípus

            if (f.AppearanceType == AppearanceType.Auto)
            {
                // Auto formátum esetén "drop!" formázó postfix hatására nem kerül bele az oszlop a gridbe
                if (GetFormat(f.ColumnName) == "drop!")
                    return;

                actualAppearanceType = GetActualAppearanceType(f, col);
            }

            DataGridViewColumn gridcol = new DataGridViewColumn();
            gridcol.DataPropertyName = f.ColumnName;

            ColumnTag coltag = new ColumnTag(f, actualAppearanceType);
            SetColumnStyle(ref gridcol, coltag);

            // kompatibilitás kedvéért: auto formázás esetén az esetleges formázó postfixet levágjuk a névről
            gridcol.Name = f.AppearanceType == AppearanceType.Auto ? GetCaption(f.ColumnName) : f.ColumnName;
            gridcol.Tag = coltag;

            // Ha nincs megadva a fejléc szövege, az oszlopnévből "gyártjuk le"
            if (string.IsNullOrEmpty(f.HeaderText))
            {
                string caption = GetCaption(f.ColumnName);
                // csak ha nem eleve csupa nagybetű
                if (formatCaptions && caption.ToUpper() != caption)
                    caption = Language.FormatCaption(caption);

                gridcol.HeaderText = Language.Translate(caption);
            }
            else
                gridcol.HeaderText = Language.Translate(f.HeaderText);

            this.Columns.Add(gridcol);
        }

        private void InitMenu()
        {
            // default menüpontok "legyártása" előre

            // content of selected cell
            txtSelected = new ToolStripTextBox();
            txtSelected.ReadOnly = true;
            txtSelected.Tag = new FxGridPopupMenuTag();

            mnuAddFilter = new ToolStripMenuItem(Language.Translate(captionMenuAddFilter));
            mnuAddFilter.Click += new EventHandler(mnuAddFilter_Click);
            mnuAddFilter.Tag = new FxGridPopupMenuTag();

            mnuClearFilters = new ToolStripMenuItem(Language.Translate(captionMenuClearFilters));
            mnuClearFilters.Click += new EventHandler(mnuClearFilters_Click);
            mnuClearFilters.Tag = new FxGridPopupMenuTag();

            // export to clipboard
            mnuExport = new ToolStripMenuItem(Language.Translate(captionMenuExportToClipboard));
            mnuExport.Click += new EventHandler(mnuExportToClipboard_Click);
            mnuExport.Tag = new FxGridPopupMenuTag();

            // export to excel
            mnuExportToExcel = new ToolStripMenuItem(Language.Translate(captionMenuExportToExcel));
            mnuExportToExcel.Click += new EventHandler(mnuExportToExcel_Click);
            mnuExportToExcel.Tag = new FxGridPopupMenuTag();

            // print
            mnuPrint = new ToolStripMenuItem(Language.Translate(captionMenuPrint));
            mnuPrint.Click += new EventHandler(mnuPrint_Click);
            mnuPrint.Tag = new FxGridPopupMenuTag();


            // selection
            mnuSelection = new ToolStripDropDownButton(Language.Translate(captionMenuSelection));
            mnuSelection.Tag = new FxGridPopupMenuTag();

            mnuSelectionAll = new ToolStripMenuItem(Language.Translate(captionMenuSelectAll));
            mnuSelectionAll.Tag = new FxGridPopupMenuTag();
            mnuSelectionAll.Click += new EventHandler(SelectAllRows);

            mnuSelectionInvert = new ToolStripMenuItem(Language.Translate(captionMenuSelectInvert));
            mnuSelectionInvert.Tag = new FxGridPopupMenuTag();
            mnuSelectionInvert.Click += new EventHandler(SelectInvertRows);

            mnuSelectionNone = new ToolStripMenuItem(Language.Translate(captionMenuSelectNone));
            mnuSelectionNone.Tag = new FxGridPopupMenuTag();
            mnuSelectionNone.Click += new EventHandler(SelectNoRows);

            mnuSelection.DropDown.Items.Add(mnuSelectionAll);
            mnuSelection.DropDown.Items.Add(mnuSelectionInvert);
            mnuSelection.DropDown.Items.Add(mnuSelectionNone);

            // settings
            mnuSettings = new ToolStripMenuItem(Language.Translate(captionMenuSettings));
            mnuSettings.Tag = new FxGridPopupMenuTag();
            mnuSettings.Click += new EventHandler(mnuSettings_Click);
        }

        private void Construct()
        {
            // átállított alapértelmezésű property-k, de designerből visszállíthatók
            MultiSelect = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            defaultRowStyle.BackColor = System.Drawing.SystemColors.ControlLightLight;
            RowsDefaultCellStyle = defaultRowStyle;
            defaultAlternatingStyle.BackColor = System.Drawing.SystemColors.ControlLight;
            AlternatingRowsDefaultCellStyle = defaultAlternatingStyle;

            // designer beolvasás
            InitializeComponent();

            // egyéb inicializálások
            Field.colIndex = 0;
            InitMenu();
            BuildMenu();
            this.CellClick += new DataGridViewCellEventHandler(AdvancedDataGridView_CellClick);
            this.CellDoubleClick += new DataGridViewCellEventHandler(AdvancedDataGridView_CellDoubleClick);
            this.CellContentClick += new DataGridViewCellEventHandler(GridCellContentClick);
            this.CellFormatting += new DataGridViewCellFormattingEventHandler(GridCellFormatting);
			Language.LanguageChanged += TranslateContextMenuCaptions;
        }

        void SetMergeEvents(bool Subscribe)
        {
            if (Subscribe)
            {
                if (mergeEventsSet)
                    return;

                mergeEventsSet = true;

                this.RowPrePaint += new DataGridViewRowPrePaintEventHandler(MergeRows);
                this.ColumnWidthChanged += new DataGridViewColumnEventHandler(AdvancedDataGridView_ColumnWidthChanged);
                this.Scroll += new ScrollEventHandler(AdvancedDataGridView_Scroll);
                this.Resize += new EventHandler(AdvancedDataGridView_Resize);
                this.SelectionChanged += new EventHandler(AdvancedDataGridView_SelectionChanged);
            }
            else //unsubscribe
            {
                if (!mergeEventsSet)
                    return;

                mergeEventsSet = false;

                this.RowPrePaint -= new DataGridViewRowPrePaintEventHandler(MergeRows);
                this.ColumnWidthChanged -= new DataGridViewColumnEventHandler(AdvancedDataGridView_ColumnWidthChanged);
                this.Scroll -= new ScrollEventHandler(AdvancedDataGridView_Scroll);
                this.Resize -= new EventHandler(AdvancedDataGridView_Resize);
                this.SelectionChanged -= new EventHandler(AdvancedDataGridView_SelectionChanged);
            }
        }

        void AdvancedDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (rowsToMerge.Count == 0)
                return;

            this.Invalidate();
        }

        void AdvancedDataGridView_Resize(object sender, EventArgs e)
        {
            if (rowsToMerge.Count == 0)
                return;

            this.Invalidate();
        }

        void AdvancedDataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            if (rowsToMerge.Count == 0)
                return;

            this.Invalidate();
        }


        void AdvancedDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (rowsToMerge.Count == 0)
                return;

            this.Invalidate();
        }

        void MergeRows(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (rowsToMerge.Count == 0)
                return;

            bool rowMerged;
            for (int r = 0; r < this.RowCount; r++)
            {
                rowMerged = false;
                if (rowsToMerge.Contains(r))
                {
                    for (int c = 0; c < this.ColumnCount - 1; c++)
                        if (Equals(this[c, r].Value, this[c + 1, r].Value))
                        {
                            MergeCells(r, ref c, e);
                            rowMerged = true;
                        }

                    if (!rowMerged)
                        DrawRow(r, e);
                }
                else
                {
                    DrawRow(r, e);
                }
            }
            e.Handled = true;
        }

        private void DrawRow(int rowIndex, DataGridViewRowPrePaintEventArgs e)
        {
            Rectangle rect = this.GetRowDisplayRectangle(rowIndex, false);
            e.PaintCells(rect, DataGridViewPaintParts.All);
        }

        private void MergeCells(int r, ref int c, DataGridViewRowPrePaintEventArgs e)
        {
            int mergeTo = c + 1;

            Rectangle rectToMerge = this.GetCellDisplayRectangle(c, r, false);
            Rectangle rectLeft = this.GetRowDisplayRectangle(r, false);

            Rectangle rectRight = rectLeft;

            while (mergeTo < this.ColumnCount && Equals(this[c, r].Value, this[mergeTo, r].Value))
            {
                if (this.Columns[mergeTo].Visible)
                    rectToMerge.Width += this.GetCellDisplayRectangle(mergeTo, r, false).Width;
                mergeTo++;
            }

            rectLeft.Width = rectToMerge.X - 2;
            rectRight.X = rectToMerge.Right;

            if (rectLeft.Width > 0)
                e.PaintCells(rectLeft, DataGridViewPaintParts.All);

            if (rectRight.Width > 0)
                e.PaintCells(rectRight, DataGridViewPaintParts.All);

            //e.Graphics.FillRectangle(new SolidBrush(Color.Green), rectLeft);
            //e.Graphics.FillRectangle(new SolidBrush(Color.Blue), rectRight);
            //e.Graphics.DrawRectangle(new Pen(Color.Green), rectLeft);
            //e.Graphics.DrawRectangle(new Pen(Color.Green), rectRight);


            rectToMerge.Height--;
            rectToMerge.Width -= 1;
            //rectToMerge.X -= this.HorizontalScrollingOffset;
            //rectToMerge.Width += this.HorizontalScrollingOffset;

            object value = this.Rows.SharedRow(r).Cells[c].Value;

            if (value == null)
                return;

            string text = value.ToString();

            SolidBrush forebrush = null;
            SolidBrush backbrush = null;

            bool mergedCellsSelected = false;

            for (int i = c; i < mergeTo; i++)
                if (this.SelectedCells.Contains(this[i, r]))
                {
                    mergedCellsSelected = true;
                    break;
                }

            try
            {
                if (mergedCellsSelected)
                {
                    forebrush = new SolidBrush(this[c, r].InheritedStyle.SelectionForeColor);
                    backbrush = new SolidBrush(this[c, r].InheritedStyle.SelectionBackColor);
                }
                else
                {
                    forebrush = new SolidBrush(this[c, r].InheritedStyle.ForeColor);
                    backbrush = new SolidBrush(this[c, r].InheritedStyle.BackColor);
                }

                System.Drawing.StringFormat sf = new System.Drawing.StringFormat(StringFormatFlags.FitBlackBox);

                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.PaintCells(rectToMerge, DataGridViewPaintParts.Border | DataGridViewPaintParts.Focus | DataGridViewPaintParts.ErrorIcon);
                //e.Graphics.DrawRectangle(new Pen(Color.Red), rectToMerge);
                e.Graphics.FillRectangle(backbrush, rectToMerge);
                e.Graphics.DrawString(text, this[c, r].InheritedStyle.Font, forebrush, (RectangleF)rectToMerge, sf);
            }
            finally
            {
                forebrush.Dispose();
                backbrush.Dispose();
            }
            c = mergeTo;
            return;
        }

        #endregion

        #region Implemented events

        void mnuExportToClipboard_Click(object sender, EventArgs e)
        {
            ExportToClipboard(true, true);
        }

        void mnuExportToExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        void mnuSettings_Click(object sender, EventArgs e)
        {
            if (SettingsClicked != null)
                SettingsClicked(sender);
        }

        private void mnuClearFilters_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        private void mnuAddFilter_Click(object sender, EventArgs e)
        {
            if (FilterDictionary == null || !hasFilters)
                return;

            if (FilterDictionary.ContainsKey(ClickedCellColumnIndex))
                FilterDictionary.Remove(ClickedCellColumnIndex);

            FilterDictionary.Add(ClickedCellColumnIndex, ClickedCellValue);
            MarkFilteredColumns();
            FilterRows();

            if (FilterAdded != null)
                FilterAdded(this, new FilterAddedEventArgs(ClickedCellValue, ClickedCellColumnIndex));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mnuPrint_Click(object sender, EventArgs e)
        {
            if (PrintMenuClick == null) return;
            PrintMenuClick(sender, e);
        }

        void AdvancedDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                txtSelected.Text = "";
                ClickedCellColumnIndex = -1;
                ClickedCellValue = null;
                return;
            }

            //this.CurrentCell = this[e.ColumnIndex, e.RowIndex];
            ClickedCellColumnIndex = e.ColumnIndex;
            ClickedCellValue = this[e.ColumnIndex, e.RowIndex].Value;

            if (ClickedCellValue != null)
                txtSelected.Text = ClickedCellValue.ToString();
            else
                txtSelected.Text = "";

        }

        private void AdvancedDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (CellClicked == null)
                return;

            if (this.Columns[e.ColumnIndex].Tag is ColumnTag)
            {
                ColumnTag tag = (ColumnTag)this.Columns[e.ColumnIndex].Tag;
                CellClicked(this, new FieldCellClickEventArgs(this, tag.FieldData, tag.ActualAppearanceType,
                    e.RowIndex, e.ColumnIndex, this[e.ColumnIndex, e.RowIndex].Value));
            }
        }

        void AdvancedDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (CellDoubleClicked == null)
                return;

            if (this.Columns[e.ColumnIndex].Tag is ColumnTag)
            {
                ColumnTag tag = (ColumnTag)this.Columns[e.ColumnIndex].Tag;
                CellDoubleClicked(this, new FieldCellClickEventArgs(this, tag.FieldData, tag.ActualAppearanceType,
                    e.RowIndex, e.ColumnIndex, this[e.ColumnIndex, e.RowIndex].Value));
            }
        }

        private void GridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (SelectionColumn != null && e.ColumnIndex == SelectionColumn.Index && this.ReadOnly)
            {
                if (this[e.ColumnIndex, e.RowIndex].Value == null)
                {
                    this[e.ColumnIndex, e.RowIndex].Value = true;
                    return;
                }

                this[e.ColumnIndex, e.RowIndex].Value = !((bool)(this[e.ColumnIndex, e.RowIndex].Value));
                return;
            }

            object value = this[e.ColumnIndex, e.RowIndex].Value;

            if (value == null || LinkCellClicked == null)
                return;

            if ((this.Columns[e.ColumnIndex].Tag is ColumnTag) &&
                ((ColumnTag)this.Columns[e.ColumnIndex].Tag).ActualAppearanceType == AppearanceType.Link &&
                this.Columns[e.ColumnIndex].CellTemplate is DataGridViewLinkCell)
            {
                ColumnTag tag = (ColumnTag)this.Columns[e.ColumnIndex].Tag;
                LinkCellClicked(this, new FieldCellClickEventArgs(this, tag.FieldData, tag.ActualAppearanceType,
                    e.RowIndex, e.ColumnIndex, value));
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        private void GridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || this.Columns[e.ColumnIndex].Tag == null)
                return;

            if (!(this.Columns[e.ColumnIndex].Tag is ColumnTag))
                return;

            AppearanceType actApp = ((ColumnTag)(this.Columns[e.ColumnIndex].Tag)).ActualAppearanceType;
            Field f = ((ColumnTag)(this.Columns[e.ColumnIndex].Tag)).FieldData;

            // az alábbiak amiatt, hogy a RowTemplate-ek és (AlternatingRow)DefaultCellStyle-ok sokszor meg vannak adva, és akkor hiába állítgatjuk
            if (f.DefaultCellStyle.BackColor != Color.Empty)
                e.CellStyle.BackColor = f.DefaultCellStyle.BackColor;
            if (f.DefaultCellStyle.Font != null)
                e.CellStyle.Font = f.DefaultCellStyle.Font;

            if (actApp == AppearanceType.Amount)
            {
                if (!((e.Value == DBNull.Value)) && (Convert.ToDecimal(e.Value)) < amountThresholdValue)
                    e.CellStyle.ForeColor = amountNegativeForeColor;
            }
            else if (actApp == AppearanceType.Currency || actApp == AppearanceType.CaptionedCurrency)
            {
                if (e.Value != DBNull.Value)
                {
                    if (actApp == AppearanceType.CaptionedCurrency)
                        ((DataGridViewTextAndImageCell)(this[e.ColumnIndex, e.RowIndex])).Image = Currencies.CurrencyImage((string)e.Value);
                    else
                        e.Value = Currencies.CurrencyImage((string)e.Value);
                }
            }

            // FormatCell esemény
            if (FormatCell != null)
            {
                ColumnTag tag = (ColumnTag)this.Columns[e.ColumnIndex].Tag;
                FieldCellFormattingEventArgs args = new FieldCellFormattingEventArgs(this, tag.FieldData, tag.ActualAppearanceType,
                    e.RowIndex, e.ColumnIndex, e.Value, e.DesiredType, e.CellStyle, e.FormattingApplied);
                FormatCell(this, args);
                e.CellStyle = args.CellStyle;
                e.FormattingApplied = args.FormattingApplied;
                e.Value = args.CellValue;
            }

            if (actApp == AppearanceType.Checkbox && e.Value is DBNull)
            {
                e.Value = false; // CheckBox nem kaphat null-t FormattedValue-ként
            }
        }

        private void TranslateContextMenuCaptions(object sender, LanguageChangedEventArgs e)
        {
            if (SelectionColumn != null)
                SelectionColumn.HeaderText = Language.Translate(captionSelectionColumn);

            mnuSelection.Text = Language.Translate(captionMenuSelection);
            mnuExport.Text = Language.Translate(captionMenuExportToClipboard);
            mnuExportToExcel.Text = Language.Translate(captionMenuExportToExcel);
            mnuSettings.Text = Language.Translate(captionMenuSettings);
            mnuAddFilter.Text = Language.Translate(captionMenuAddFilter);
            mnuClearFilters.Text = Language.Translate(captionMenuClearFilters);
            mnuSelectionAll.Text = Language.Translate(captionMenuSelectAll);
            mnuSelectionInvert.Text = Language.Translate(captionMenuSelectInvert);
            mnuSelectionNone.Text = Language.Translate(captionMenuSelectNone);
        }

        #endregion

        #region Constructors

        public AdvancedDataGridView()
        {
            Construct();
        }

        public AdvancedDataGridView(IContainer container)
        {
            container.Add(this);

            Construct();
        }

        #endregion

    }
}
