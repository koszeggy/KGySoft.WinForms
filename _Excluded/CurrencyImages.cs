using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace KGySoft.Controls
{
    public class CurrencyItem
    {
        string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        Bitmap m_Image;
        public Bitmap Image
        {
            get { return m_Image; }
            set { m_Image = value; }
        }

        bool m_Checked;
        public bool Checked
        {
            get { return m_Checked; }
            set { m_Checked = value; }
        }

        public CurrencyItem(string name, Bitmap img)
        {
            m_Name = name;
            m_Image = img;
        }

        public CurrencyItem(string name, bool check)
        {
            m_Name = name;
            m_Checked = check;
        }

        public CurrencyItem(string name, Bitmap bmp, bool check)
        {
            m_Name = name;
            m_Checked = check;
            m_Image = bmp;
        }
    }

    public static class Currencies
    {
        public static List<CurrencyItem> CurrencyImageList = new List<CurrencyItem>();

        static Currencies()
        {
            FillCurrencyList();
        }

        /// <summary>
        /// visszaadja a parameterben megadott currency zaszlojat
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Bitmap CurrencyImage(string name)
        {
            Bitmap bm = null;
            GetCurrencyImage(name, out bm);
            return bm;
        }

        /// <summary>
        /// visszadja a curr nevehez tartozo kepet a Currencies nevű resource file-ból
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetCurrencyImage(string name, out Bitmap currencyImage)
        {
            bool isCurrency = true;
            switch (name.ToUpper().Trim())
            {
                case "ATS": currencyImage = CurrencyImages.ATS; break;
                case "AUD": currencyImage = CurrencyImages.AUD; break;
                case "BEF": currencyImage = CurrencyImages.BEF; break;
                case "CAD": currencyImage = CurrencyImages.CAD; break;
                case "CHF": currencyImage = CurrencyImages.CHF; break;
                case "CZK": currencyImage = CurrencyImages.CZK; break;
                case "DEM": currencyImage = CurrencyImages.DEM; break;
                case "DKK": currencyImage = CurrencyImages.DKK; break;
                case "DZD": currencyImage = CurrencyImages.DZD; break;
                case "ESP": currencyImage = CurrencyImages.ESP; break;
                case "EUR": currencyImage = CurrencyImages.EUR; break;
                case "FIM": currencyImage = CurrencyImages.FIM; break;
                case "FRF": currencyImage = CurrencyImages.FRF; break;
                case "GBP": currencyImage = CurrencyImages.GBP; break;
                case "HUF": currencyImage = CurrencyImages.HUF; break;
                case "IEP": currencyImage = CurrencyImages.IEP; break;
                case "ITL": currencyImage = CurrencyImages.ITL; break;
                case "JPY": currencyImage = CurrencyImages.JPY; break;
                case "KRW": currencyImage = CurrencyImages.KRW; break;
                case "LUF": currencyImage = CurrencyImages.LUF; break;
                case "LYD": currencyImage = CurrencyImages.LYD; break;
                case "NLG": currencyImage = CurrencyImages.NLG; break;
                case "NOK": currencyImage = CurrencyImages.NOK; break;
                case "PLN": currencyImage = CurrencyImages.PLN; break;
                case "PTE": currencyImage = CurrencyImages.PTE; break;
                case "SEK": currencyImage = CurrencyImages.SEK; break;
                case "SKK": currencyImage = CurrencyImages.SKK; break;
                case "TND": currencyImage = CurrencyImages.TND; break;
                case "TRL": currencyImage = CurrencyImages.TRL; break;
                case "TRY": currencyImage = CurrencyImages.TRY; break;
                case "USD": currencyImage = CurrencyImages.USD; break;
                case "XAG": currencyImage = CurrencyImages.XAG; break;
                case "XAU": currencyImage = CurrencyImages.XAU; break;
                case "XDR": currencyImage = CurrencyImages.XDR; break;
                case "XPT": currencyImage = CurrencyImages.XPT; break;

                default:
                    currencyImage = CurrencyImages.XPT;
                    isCurrency = false;
                    break;
            }
            currencyImage.MakeTransparent();

            return isCurrency;
        }


        /// <summary>
        /// feltolti a currency list-et..
        /// </summary>
        public static void FillCurrencyList()
        {
            CurrencyImageList.Clear();

            CurrencyImageList.Add(new CurrencyItem("ATS", CurrencyImages.ATS));
            CurrencyImageList.Add(new CurrencyItem("AUD", CurrencyImages.AUD));
            CurrencyImageList.Add(new CurrencyItem("BEF", CurrencyImages.BEF));
            CurrencyImageList.Add(new CurrencyItem("CAD", CurrencyImages.CAD));
            CurrencyImageList.Add(new CurrencyItem("CHF", CurrencyImages.CHF));
            CurrencyImageList.Add(new CurrencyItem("CZK", CurrencyImages.CZK));
            CurrencyImageList.Add(new CurrencyItem("DEM", CurrencyImages.DEM));
            CurrencyImageList.Add(new CurrencyItem("DKK", CurrencyImages.DKK));
            CurrencyImageList.Add(new CurrencyItem("DZD", CurrencyImages.DZD));
            CurrencyImageList.Add(new CurrencyItem("ESP", CurrencyImages.ESP));
            CurrencyImageList.Add(new CurrencyItem("EUR", CurrencyImages.EUR));
            CurrencyImageList.Add(new CurrencyItem("FIM", CurrencyImages.FIM));
            CurrencyImageList.Add(new CurrencyItem("FRF", CurrencyImages.FRF));
            CurrencyImageList.Add(new CurrencyItem("GBP", CurrencyImages.GBP));
            CurrencyImageList.Add(new CurrencyItem("HUF", CurrencyImages.HUF));
            CurrencyImageList.Add(new CurrencyItem("IEP", CurrencyImages.IEP));
            CurrencyImageList.Add(new CurrencyItem("ITL", CurrencyImages.ITL));
            CurrencyImageList.Add(new CurrencyItem("JPY", CurrencyImages.JPY));
            CurrencyImageList.Add(new CurrencyItem("KRW", CurrencyImages.KRW));
            CurrencyImageList.Add(new CurrencyItem("LUF", CurrencyImages.LUF));
            CurrencyImageList.Add(new CurrencyItem("LYD", CurrencyImages.LYD));
            CurrencyImageList.Add(new CurrencyItem("NLG", CurrencyImages.NLG));
            CurrencyImageList.Add(new CurrencyItem("NOK", CurrencyImages.NOK));
            CurrencyImageList.Add(new CurrencyItem("PLN", CurrencyImages.PLN));
            CurrencyImageList.Add(new CurrencyItem("PTE", CurrencyImages.PTE));
            CurrencyImageList.Add(new CurrencyItem("SEK", CurrencyImages.SEK));
            CurrencyImageList.Add(new CurrencyItem("SKK", CurrencyImages.SKK));
            CurrencyImageList.Add(new CurrencyItem("TND", CurrencyImages.TND));
            CurrencyImageList.Add(new CurrencyItem("TRL", CurrencyImages.TRL));
            CurrencyImageList.Add(new CurrencyItem("TRY", CurrencyImages.TRY));
            CurrencyImageList.Add(new CurrencyItem("USD", CurrencyImages.USD));
            CurrencyImageList.Add(new CurrencyItem("XAG", CurrencyImages.XAG));
            CurrencyImageList.Add(new CurrencyItem("XAU", CurrencyImages.XAU));
            CurrencyImageList.Add(new CurrencyItem("XDR", CurrencyImages.XDR));
            CurrencyImageList.Add(new CurrencyItem("XPT", CurrencyImages.XPT));

        }




    } // class
} // ns
