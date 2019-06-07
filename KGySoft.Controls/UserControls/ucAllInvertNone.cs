using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using KGySoft.Drawing;

namespace KGySoft.Controls
{

    public partial class ucAllInvertNone : UserControl
    {
		/// <summary>
		/// Occurs when a button is pressed.
		/// </summary>
        [Category("ucAllInvertNone")]
		[Description("Occurs when a button is pressed.")]
        public event EventHandler<AllInvertNoneEventArgs> ButtonPressed;

        public ucAllInvertNone()
        {
            InitializeComponent();

            this.buttonNone.Image = Images.None;
            this.buttonInvert.Image = Images.Options;
            this.buttonAll.Image = Images.All;

            buttonAll.Click += new EventHandler(buttonAll_Click);
            buttonInvert.Click += new EventHandler(buttonInvert_Click);
            buttonNone.Click += new EventHandler(buttonNone_Click);
        }

        void buttonNone_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.None));
        }

        void buttonInvert_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.Invert));
        }

        void buttonAll_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.All));
        }

    }

    public enum InvertButtonTypes
    { 
        All, Invert, None
    }


    public class AllInvertNoneEventArgs : EventArgs
    {
        InvertButtonTypes buttonType;

        public InvertButtonTypes ButtonType
        {
            get { return buttonType; }
        }

        public AllInvertNoneEventArgs(InvertButtonTypes buttonType)
        {
            this.buttonType = buttonType;
        }
    }
}
