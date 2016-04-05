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

    public partial class ucCurrencyText : ucTextWithPicture
    {
        public class CurrencySelectedEventArgs : EventArgs
        {
            string currency;

            public string Currency
            {
                get { return currency; }
            }

            public CurrencySelectedEventArgs(string currency)
            {
                this.currency = currency;
            }
        }

        public delegate void CurrencySelectedDelegate(object sender, CurrencySelectedEventArgs e);

        [
        Description("Akkor sül el, amikor kiválasztunk egy devizanemet."),
        Category("ucCurrencyText")
        ]
        public event CurrencySelectedDelegate CurrencySelected;

        public ucCurrencyText()
        {
            InitializeComponent();

            textControl.TextChanged += new EventHandler(txtValue_TextChanged);
            textControl.Leave += new EventHandler(txtValue_Leave);
            Caption = "Currency";
            textControl.MaxLength = 3;
        }


        void txtValue_TextChanged(object sender, EventArgs e)
        {
            Bitmap currencyBmp = null;
            if (Currencies.GetCurrencyImage(textControl.Text, out currencyBmp))
            {
                if (currencyBmp != null)
                {
                    currencyBmp.MakeTransparent();
                    pbImg.Image = currencyBmp;
                }
                else
                {
                    pbImg.Image = null;
                }
            }
            else
            {
                pbImg.Image = null;
            }

            int selStart = textControl.SelectionStart;
            textControl.Text = textControl.Text.ToUpper();
            textControl.SelectionStart = selStart;
            if (textControl.Text.Length == textControl.MaxLength && pbImg.Image!=null)
                if (CurrencySelected!=null)
                    CurrencySelected(this, new CurrencySelectedEventArgs(textControl.Text));
        }


        /// <summary>
        /// Ha nincs érvényes devizanem beírvva -> DbNull.Value
        /// Ha van -> Deviza szövegesen, a textboxban található szöveg. (txtValue.Text)
        /// </summary>
        public object Currency
        {
            get 
            {
                if (pbImg.Image != null)
                {
                    return textControl.Text;
                }
                else
                {
                    return DBNull.Value;
                }
            }
        }

        bool m_WhenNothingSetFocusCantLeave = false;
        /// <summary>
        /// ha nincs érvényes deviza, akkor a focust nem engedi át...
        /// </summary>
        [Description("ha nincs érvényes deviza, akkor a focust nem engedi át...")]        
        public bool WhenNothingSetFocusCantLeave
        {
            get { return m_WhenNothingSetFocusCantLeave; }
            set { m_WhenNothingSetFocusCantLeave = value; }
        }

        bool m_ShowWarningWhenNothingSet = false;
        /// <summary>
        /// dobjon-e warning-ot ha nincs érvényes deviza beírvva
        /// </summary>
        [Description("dobjon-e warning-ot ha nincs érvényes deviza beírvva")]        
        public bool ShowWarningWhenNothingSet
        {
            get { return m_ShowWarningWhenNothingSet; }
            set { m_ShowWarningWhenNothingSet = value; }
        }

        void txtValue_Leave(object sender, EventArgs e)
        {
            if (m_ShowWarningWhenNothingSet && pbImg.Image == null)
            {
                MessageBox.Show("Valid currency required!", "Currency warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (m_WhenNothingSetFocusCantLeave && pbImg.Image == null)
            {
                textControl.Focus();
            }
        }
    }
}
