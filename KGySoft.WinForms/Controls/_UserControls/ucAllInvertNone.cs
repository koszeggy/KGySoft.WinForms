using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace KGySoft.WinForms.Controls
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

            this.buttonNone.Image = Properties.Resources.None;
            this.buttonInvert.Image = Properties.Resources.Options;
            this.buttonAll.Image = Properties.Resources.All;

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
