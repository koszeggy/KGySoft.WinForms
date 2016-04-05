/*
 * DriveComboBox by KGy
 * 
 * Meghajtókat listázó ComboBox. Összekapcsolható egy DirectoryTreeView-val
 */
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace KGySoft.Controls.FileControls
{
    public partial class DriveComboBox : ComboBox
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int GetDriveTypeA(string drive);

        #region típusok

        struct DriveItem
        {
            public DriveInfo Info;
            public char Letter;

            public override string ToString()
            {
                return Letter.ToString() + ": [" /*+ Info.VolumeLabel + " - "*/ + Info.DriveType.ToString() + "]";
            }
        }

        #endregion

        #region objektumváltozók

        DirectoryTreeView dtv = null;
        char drive = '\0';
        bool isChanging = false;

        #endregion

        #region property-k

        [Category("DriveComboBox")]
        [Description("A hozzá kapcsolt DirectoryTreeView")]
        public DirectoryTreeView DirectoryTreeView
        {
            get { return dtv; }
            set { dtv = value; }
        }

        [Category("DriveComboBox")]
        [Description("Az aktuális meghajtó")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public char Drive
        {
            get { return drive; }
            set { SetDrive(value); }
        }

        #endregion

        #region konstruktor, publikus metódusok

        public DriveComboBox()
        {
            InitializeComponent();
            DropDownStyle = ComboBoxStyle.DropDownList;
            SelectedIndexChanged += new EventHandler(DriveComboBox_SelectedIndexChanged);

            BuildList();
            try
            {
                Drive = Directory.GetCurrentDirectory()[0];
            }
            catch
            {
                SelectedIndex = 0;
            }
        }

        public override string ToString()
        {
            return drive.ToString() + ":";
        }

        #endregion

        #region privát és protected metódusok

        private void BuildList()
        {
            string[] drives = Directory.GetLogicalDrives();
            this.Items.Clear();

            foreach (string drv in drives)
            {
                DriveInfo info = new DriveInfo(drv);
                DriveItem item = new DriveItem();
                item.Letter = char.ToUpper(drv[0]);
                item.Info = info;
                this.Items.Add(item);
            }
        }

        private void SetDrive(char value)
        {
            if (value == '\0' || value == drive)
                return;
            value = char.ToUpper(value);
            foreach (DriveItem item in Items)
                if (item.Letter == value)
                {
                    SelectedItem = item;
                    return;
                }
            throw new Exception("Invalid drive letter: " + value.ToString() + ":");
        }

        #endregion

        #region Események

        void DriveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isChanging)
                return;

            isChanging = true;
            if (SelectedIndex >= 0)
            {
                
                if (((DriveItem)SelectedItem).Info.IsReady)
                {
                    drive = ((DriveItem)SelectedItem).Letter;
                    if (DirectoryTreeView != null)
                        DirectoryTreeView.Path = drive + ":";                  
                }
                else
                    Drive = drive; // visszaállunk az elõzõ meghajtóra
            }
            else drive = '\0';
            isChanging = false;
        }

        #endregion
    }
}
