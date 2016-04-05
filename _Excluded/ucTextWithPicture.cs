using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KGySoft.Controls
{
    public partial class ucTextWithPicture : ucText
    {
        public ucTextWithPicture()
        {
            InitializeComponent();
            //Size = new Size(45, 20);
        }

        /// <summary>
        /// kép
        /// </summary>
        public Image Picture
        {
            get { return pbImg.Image;  }
            set { pbImg.Image = value; }
        }

        /// <summary>
        /// A belső PictureBox
        /// </summary>
        [
        Description("A belső PictureBox"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public PictureBox PictureBox
        {
            get { return pbImg; }
            set { pbImg = value; }
        }
    }
}
