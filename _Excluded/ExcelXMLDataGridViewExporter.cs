using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using KGySoft.Libraries;
using System.IO;

namespace KGySoft.Controls
{

    /// <summary>
    /// Egy stlilus elemet reprezental.
    /// </summary>
    public class ExcelXMLStyle : IComparable
    {
        public string m_StyleName = "";
        public string m_StyleId = "";

        #region props
        /// <summary>
        /// style name, ie. 'Hyperlink style'
        /// </summary>
        public string StyleName
        {
            get
            {
                return m_StyleName;
            }
            set
            {
                m_StyleName = value;
            }
        }

        /// <summary>
        /// style id, ie. s21
        /// </summary>
        public string StyleId
        {
            get
            {
                return m_StyleId;
            }
            set
            {
                m_StyleId = value;
            }
        }

        Font m_StyleFont = null;

        /// <summary>
        /// A font aminek alapjan a style-t osszeallitja.
        /// </summary>
        public Font StyleFont
        {
            get
            {
                return m_StyleFont;
            }
            set
            {
                m_StyleFont = value;

                Bold = m_StyleFont.Bold;
                Underline = m_StyleFont.Underline;
                Italic = m_StyleFont.Italic;
                FontName = m_StyleFont.Name;
            }
        }


        DataGridViewCell m_StyleCell = null;

        /// <summary>
        /// A cella aminek alapjan a style-t osszeallitja.
        /// </summary>
        public DataGridViewCell StyleCell
        {
            get
            {
                return m_StyleCell;
            }
            set
            {
                m_StyleCell = value;

                StyleFont = (m_StyleCell.Style.Font == null ? m_StyleCell.DataGridView.DefaultCellStyle.Font : m_StyleCell.Style.Font);

                if (!m_StyleCell.Style.BackColor.IsEmpty)
                    BackColor = m_StyleCell.Style.BackColor;
                else
                    BackColor = m_StyleCell.DataGridView.DefaultCellStyle.BackColor;

                if (!m_StyleCell.Style.ForeColor.IsEmpty)
                    ForeColor = m_StyleCell.Style.ForeColor;
                else
                    ForeColor = m_StyleCell.DataGridView.DefaultCellStyle.ForeColor;
            }
        }

        #endregion

        public bool Bold = false;
        public bool Underline = false;
        public bool Italic = false;
        public string FontName = "";

        public Color BackColor = Color.Empty;
        public Color ForeColor = Color.Empty;


        public string WriteXMLStyleString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<Style ss:ID=\"" + m_StyleId + "\" ss:Name=\"" + m_StyleName + "\">");

            //----------------------------------------[font]
            sb.AppendLine("<Font");
            if (Bold) sb.Append(" ss:Bold=\"1\"");
            if (Underline) sb.Append(" ss:Underline=\"Single\"");
            if (Italic) sb.Append(" ss:Italic=\"1\"");

            if (ForeColor != Color.Empty)
                sb.Append(" ss:Color=\"#" + ColorToHTMLColorString(ForeColor) + "\"");
            sb.AppendLine("/>"); // font

            //----------------------------------------------
            if (BackColor != Color.Empty)
                sb.AppendLine("<Interior ss:Color=\"#" + ColorToHTMLColorString(BackColor) + "\" ss:Pattern=\"Solid\"/>");

            sb.AppendLine("</Style>");

            return sb.ToString();
        }

		private string ColorToHTMLColorString(System.Drawing.Color color)
		{
			return color.R.ToString("X").PadLeft(2, '0') +
				color.G.ToString("X").PadLeft(2, '0') +
				color.B.ToString("X").PadLeft(2, '0');
		}

        #region IComparable Members

        public int CompareTo(object obj)
        {
            //Notes to Implementers For objects A, B, and C, the following must be true: 
            //A.CompareTo(A) is required to return zero. If A.CompareTo(B) returns zero, 
            //then B.CompareTo(A) is required to return zero. If A.CompareTo(B) returns 
            //zero and B.CompareTo(C) returns zero, then A.CompareTo(C) is required to return zero. 
            //If A.CompareTo(B) returns a value other than zero, then B.CompareTo(A) is 
            //required to return a value of the opposite sign. If A.CompareTo(B) returns 
            //a value x that is not equal to zero, and B.CompareTo(C) returns a value y 
            //of the same sign as x, then A.CompareTo(C) is required to return a value 
            //of the same sign as x and y. 
            ExcelXMLStyle st;
            st = obj as ExcelXMLStyle;

            if (this.BackColor == st.BackColor &&
                this.Bold == st.Bold &&
                this.FontName == st.FontName &&
                this.ForeColor == st.ForeColor &&
                this.Italic == st.Italic &&
                this.Underline == st.Underline)
            {
                return 0;
            }
            else
            {
                return 100;
            }
        }

        #endregion
    }

    /// <summary>
    /// A stilus elemek tombje.
    /// </summary>
    public class ExcelXMLStyles
    {
        List<ExcelXMLStyle> m_Styles = new List<ExcelXMLStyle>();
        string m_StylePrefix = "s";
        //DataGridView m_Grid = null;

        //public ExcelXMLStyles(DataGridView grid)
        //{
        //    m_Grid = grid;            
        //}

        public ExcelXMLStyles()
        {
            //m_Grid = grid;
        }

        /// <summary>
        /// Hozzad egy style-t a sheet-hez. Visszadja a style id-t.
        /// </summary>
        /// <param name="style"></param>
        /// <returns>Visszater a stilus id-javal.</returns>
        public string AddStyle(ExcelXMLStyle style)
        {
            foreach (ExcelXMLStyle st in m_Styles)
            {
                if (st.CompareTo(style) == 0) //ugyanaz a ketto
                {
                    return st.m_StyleId;
                }
            }

            // ez a resz csak akkor fut le, ha nem volt egyezo
            style.m_StyleId = m_StylePrefix + m_Styles.Count.ToString();
            style.m_StyleName = "Style " + style.m_StyleId;
            m_Styles.Add(style);

            return style.m_StyleId;
        }

        /// <summary>
        /// Hozzad egy style-t a sheet-hez. Visszadja a style id-t.
        /// </summary>
        /// <param name="style"></param>
        /// <returns>Visszater a stilus id-javal.</returns>
        public string AddStyleBasedOnCell(DataGridViewCell cell)
        {
            ExcelXMLStyle s = new ExcelXMLStyle();
            s.StyleCell = cell;

            return AddStyle(s);
        }


        /// <summary>
        /// Kiirja a Styles reszt.
        /// </summary>
        /// <returns></returns>
        public string WriteXMLStyleString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<Styles>");

            sb.AppendLine("    <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
            sb.AppendLine("    <Alignment ss:Vertical=\"Bottom\"/>");
            sb.AppendLine("    <Borders/>");
            sb.AppendLine("    <Font/>");
            sb.AppendLine("    <Interior/>");
            sb.AppendLine("    <NumberFormat/>");
            sb.AppendLine("    <Protection/>");
            sb.AppendLine("    </Style>");

            sb.AppendLine("    <Style ss:ID=\"header_style\">");
            sb.AppendLine("     <Alignment ss:Vertical=\"Bottom\"/>");
            sb.AppendLine("     <Borders/>");
            sb.AppendLine("     <Font x:CharSet=\"238\" x:Family=\"Swiss\" ss:Bold=\"1\"/>");
            sb.AppendLine("     <Interior ss:Color=\"#B5B5B5\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("     <NumberFormat/>");
            sb.AppendLine("     <Protection/>");
            sb.AppendLine("    </Style>");


            foreach (ExcelXMLStyle st in m_Styles)
            {
                sb.AppendLine(TabbifyLines(st.WriteXMLStyleString()));
            }

            sb.AppendLine("</Styles>");

            return sb.ToString();
        }

		private string TabbifyLines(string lines)
		{
			lines = "\t" + lines;
			lines = lines.Replace("\n", "\n\t");
			if (lines[lines.Length - 1] == '\t') lines = lines.Remove(lines.Length - 1);
			return lines;
		}

    }


    /// <summary>
    /// Egy excel sheet. Tipikusan egy grid tartalma.
    /// PM-ből
    /// SM
    /// </summary>
    public class ExcelXMLWorksheet
    {
        StringBuilder m_SpreadSheetText = new StringBuilder();
        DataGridView m_Grid;
        string m_SheetName;
        ExcelXMLStyles m_Styles = null;

        /// <summary>
        /// A kapott grid-et a kapott lapnevvel generálja.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="sheetName"></param>
        public ExcelXMLWorksheet(DataGridView grid, string sheetName, ref ExcelXMLStyles styles)
        {
            m_Grid = grid;
            m_SheetName = sheetName;
            m_Styles = styles;
        }

        /// <summary>
        /// Visszadja a grid-nek megfelelő sheet-et.
        /// </summary>
        public void ProcessGrid()
        {
            //string styleId = "";
            m_SpreadSheetText.AppendLine("");
            m_SpreadSheetText.AppendLine("<!-- data -->");
            m_SpreadSheetText.AppendLine("<Worksheet ss:Name=\"" + m_SheetName + "\">");
            m_SpreadSheetText.AppendLine("<Table>");

            // column sizes
            for (int column = 0; column < m_Grid.Columns.Count; column++)
            {
                if (!m_Grid.Columns[column].Visible) continue;

                // Cell Tags              
                m_SpreadSheetText.AppendFormat("\t<Column ss:AutoFitWidth=\"0\" ss:Width=\"{0}\"/>{1}", 
                    m_Grid.Columns[column].Width > 1024 ? 1024 : m_Grid.Columns[column].Width, // by KGy: túl nagy szélességtől elszáll az Excel
                    Environment.NewLine);
            }

            // header
            m_SpreadSheetText.AppendLine("<Row ss:StyleID=\"header_style\">");
            for (int column = 0; column < m_Grid.Columns.Count; column++)
            {
                if (!m_Grid.Columns[column].Visible) continue;
                // Cell Tags 
                //m_SpreadSheetText.Append("  <Cell ss:StyleID=\"" + "Default" + "\"><Data ss:Type=\"String\">");
                m_SpreadSheetText.Append("  <Cell><Data ss:Type=\"String\">");
                m_SpreadSheetText.Append(ToXMLText(m_Grid.Columns[column].HeaderText));
                m_SpreadSheetText.Append("</Data></Cell>");
                m_SpreadSheetText.AppendLine();
            }
            m_SpreadSheetText.AppendLine("</Row>");

            // data
            for (int row = 0; row < m_Grid.RowCount; row++)
            {
                AddRow(m_Grid.Rows[row]);
            }
            m_SpreadSheetText.AppendLine("</Table>");
            m_SpreadSheetText.AppendLine(ExcelWorkSheetOptions());
            m_SpreadSheetText.AppendLine("</Worksheet>");
        }

        private string ToXMLText(string p)
        {
            string result = p.Replace("&", "&amp;"); // ez legyen az első, különben a lecserélt egyéb karakterekben is lecseréli a "&"-t
            result = result.Replace("'", "&apos;");
            result = result.Replace('"'.ToString(), "&quot;");
            result = result.Replace("<", "&lt;");
            result = result.Replace(">", "&gt;");
            return result;
        }

        private string ExcelWorkSheetOptions()
        {
            // This is Required Only Once ,	But this has to go after the First Worksheet's First Table		
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("\n<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\n<Selected/>\n </WorksheetOptions>\n");
            return sb.ToString();
        }

        /// <summary>
        /// A kapott sort feldolgozza es legenaralja a sorban levo
        /// cellaknak megfelelo xml bejegyzeseket...
        /// </summary>
        /// <param name="dataGridViewRow">A tabla egy sora.</param>
        private void AddRow(DataGridViewRow dataGridViewRow)
        {
            //<Row>
            // <Cell><Data ss:Type=\"String\">Sheet1Row18Col1</Data></Cell>
            // <Cell><Data ss:Type=\"String\">Sheet1Row18Col2</Data></Cell>
            // <Cell><Data ss:Type=\"String\">Sheet1Row18Col3</Data></Cell>
            //</Row>

            // Row Tag

            m_SpreadSheetText.AppendLine("<Row>");
            for (int column = 0; column < dataGridViewRow.Cells.Count; column++)
            {
                if (!dataGridViewRow.DataGridView.Columns[column].Visible) continue;

                string styleId = m_Styles.AddStyleBasedOnCell(dataGridViewRow.Cells[column]);

                double numberValue;
                string cellvalue;
                if (dataGridViewRow.Cells[column] is DataGridViewImageCell) // kép cella esetén a FormattedValue csak "System.Drawing.Image" lenne
                    cellvalue = (dataGridViewRow.Cells[column].Value == null ? "" : dataGridViewRow.Cells[column].Value.ToString());
                else
                    cellvalue = (dataGridViewRow.Cells[column].FormattedValue == null ? "" : dataGridViewRow.Cells[column].FormattedValue.ToString());

                bool isNumber = AdvancedDataGridView.TryParseCellNumber(out numberValue, cellvalue);

                m_SpreadSheetText.AppendFormat("  <Cell ss:StyleID=\"" + styleId + "\"><Data ss:Type=\"{0}\">", isNumber ? "Number" : "String");

                if (isNumber)
                    m_SpreadSheetText.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}", numberValue);
                else //Képes / szöveges celláknál a szöveget trimeljük
                    m_SpreadSheetText.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}", ToXMLText(cellvalue.Trim()));
                m_SpreadSheetText.Append("</Data></Cell>");
                m_SpreadSheetText.AppendLine();
            }
            m_SpreadSheetText.AppendLine("</Row>");

        }

        public bool IsNumberFormat(object cellValue)
        {
            if (cellValue == null)
                return false;

            double d;

            return double.TryParse(cellValue.ToString(), out d);
        }

        public string GetWorkSheetCode()
        {
            return m_SpreadSheetText.ToString();
        }


    }


    /// <summary>
    /// DataGridView-t Excel-be exportal.
    /// </summary>
    public class ExcelXMLDataGridViewExporter
    {
        StringBuilder m_File = null;

        List<ExcelXMLWorksheet> m_Worksheets = new List<ExcelXMLWorksheet>();
        List<DataGridView> m_Grids = new List<DataGridView>();
        List<string> m_SheetNames = new List<string>();
        ExcelXMLStyles m_Styles = new ExcelXMLStyles();

        /// <summary>
        /// Egy DataGridView-bol allitja ossze az excel spreadsheet-et.
        /// </summary>
        public ExcelXMLDataGridViewExporter()
        {
            m_File = new StringBuilder();
        }

        /// <summary>
        /// Creates Excel Header 		
        /// </summary>
        /// <returns>Excel Header Strings</returns>
        private string ExcelHeader()
        {
            // Excel header
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.AppendLine("xmlns:o=\"urn:schemas-microsoft-com:office:office\" ");
            sb.AppendLine("xmlns:x=\"urn:schemas-microsoft-com:office:excel\" ");
            sb.AppendLine("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.AppendLine("xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            sb.AppendLine("<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
            sb.AppendLine("<Author>xxdsade4342x@gmail.com</Author>");
            sb.AppendLine("</DocumentProperties>");
            sb.AppendLine("<ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">");
            sb.AppendLine("<ProtectStructure>False</ProtectStructure>");
            sb.AppendLine("<ProtectWindows>False</ProtectWindows>");
            sb.AppendLine("</ExcelWorkbook>");

            return sb.ToString();
        }

        public void ExportExcelFile(string excelFileName)
        {
            foreach (ExcelXMLWorksheet ws in m_Worksheets)
            {
                ws.ProcessGrid();
            }

            m_File.AppendLine(ExcelHeader());
            m_File.AppendLine("");
            m_File.AppendLine("<!-- style definition -->");
            m_File.AppendLine(m_Styles.WriteXMLStyleString());


            foreach (ExcelXMLWorksheet ws in m_Worksheets)
            {
                m_File.AppendLine(ws.GetWorkSheetCode());
            }

            // Close the Workbook tag (in Excel header you can see the Workbook tag)
            m_File.AppendLine("</Workbook>");

            File.WriteAllText(excelFileName, m_File.ToString(), Encoding.UTF8);
        }



        public void AddGrid(DataGridView grid, string sheetname)
        {
            ///DateTime.Now.ToString("yyyyMMdd HHmmss")
            ExcelXMLWorksheet sheet = new ExcelXMLWorksheet(grid, sheetname, ref m_Styles);
            m_Worksheets.Add(sheet);
        }



    }
}
