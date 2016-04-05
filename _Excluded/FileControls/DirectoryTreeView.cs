using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace KGySoft.Controls.FileControls
{
    public partial class DirectoryTreeView : TreeView
    {
        #warning NINCS KÉSZ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        #region objektumváltozók

        string path = "";
        string root = "";
        bool showFiles = false;

        #endregion

        #region property-k

        [Category("DirectoryTreeView")]
        [Description("Aktuális könyvtár")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Path
        {
            get { return path; }
            set { SetDirectory(value); }
        }

        #endregion

        #region konstruktor, publikus metódusok

        public DirectoryTreeView()
        {
            InitializeComponent();
        }

        #endregion

        #region privát metódusok

        private void SetDirectory(string value)
        {
            if (!Directory.Exists(value))
            {
                throw new Exception("...");
            }
            if (Directory.GetDirectoryRoot(value) != root)
            {

            }
            //Directory.GetDirectoryRoot
        }
        
        #endregion
    }
}
