using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KGySoft.Libraries.Language;
using KGySoft.Controls.Properties;
using KGySoft.Libraries;

namespace KGySoft.Controls
{

    public partial class ucPortfolioSelector : ucCustomSelector
    {
        #region Típusok

        public enum PortfolioItemType
        {
            All,
            Book,
            Group,
            Portfolio,
            Entity      // ez nem azonos a rendes entityvel, csak bizonyos anyaszervezeteket jelöl a sajátcég fölött a régi inforexekben
        }

        #endregion

        #region objektumváltozók

        public static PortfolioBrowseHandler PortfolioBrowse = null;
        PortfolioItemType itemType = PortfolioItemType.All;

        #endregion

        #region Property-k


        [Category("ucPortfolioSelector")]
        [Description("Választott könyv")]
        [DefaultValue(Constants.NoneSelectedValue)]
        [RefreshProperties(RefreshProperties.All)]
        public virtual int Book
        {
            get
            {
                if (itemType == PortfolioItemType.Book)
                    return (int)Value;

                return Constants.NoneSelectedValue;
            }
            set
            {
                itemType = PortfolioItemType.Book;
                Value = value;
            }
        }

        [Category("ucPortfolioSelector")]
        [Description("Választott csoport")]
        [DefaultValue(Constants.NoneSelectedValue)]
        [RefreshProperties(RefreshProperties.All)]
        public virtual int Group
        {
            get
            {
                if (itemType == PortfolioItemType.Group)
					return (int)Value;

                return Constants.NoneSelectedValue;
            }
            set
            {
                itemType = PortfolioItemType.Group;
                Value = value;
            }
        }

        [Category("ucPortfolioSelector")]
        [Description("Választott portfólió")]
        [DefaultValue(Constants.NoneSelectedValue)]
        [RefreshProperties(RefreshProperties.All)]
        public virtual int Portfolio
        {
            get
            {
                if (itemType == PortfolioItemType.Portfolio)
					return (int)Value;

                return Constants.NoneSelectedValue;
            }
            set
            {
                itemType = PortfolioItemType.Portfolio;
                Value = value;
            }
        }

        [Category("ucPortfolioSelector")]
        [Description("Választott entitás")]
        [DefaultValue(Constants.NoneSelectedValue)]
        [RefreshProperties(RefreshProperties.All)]
        public virtual int Entity
        {
            get
            {
                if (itemType == PortfolioItemType.Entity)
					return (int)Value;

                return Constants.NoneSelectedValue;
            }
            set
            {
                itemType = PortfolioItemType.Entity;
                Value = value;
            }
        }

        [Category("ucPortfolioSelector")]
        [Description("Választott elem típusa (A Value és ez egyértelmûen azonosítja a választott elemet)")]
        [DefaultValue(typeof(PortfolioItemType), "All")]
        [RefreshProperties(RefreshProperties.All)]
        public PortfolioItemType ItemType
        {
            get { return itemType; }
            set
            {
                if (value == PortfolioItemType.All)
                {
                    Value = Constants.AllSelectedValue;
                    return;
                }
				if ((int)Value > 0)
                    itemType = value;
                SetImage(itemType);
            }
        }

        [Category("ucPortfolioSelector")]
        [Description("Az \"Összes\" szûrés érvényes-e")]
        [DefaultValue(true)]
        [RefreshProperties(RefreshProperties.All)]
        public bool All
        {
            get
            {
                return itemType == PortfolioItemType.All;
            }
            set
            {
                if (value)
                    Value = Constants.AllSelectedValue;
                else ; // nos... ilyet azé' má' mégse így, inkább állítsa be a megfelelõ property-t
            }
        }

        #endregion

        #region konstruktor, metódusok

        public ucPortfolioSelector()
        {
            InitializeComponent();
            Value = Constants.AllSelectedValue;
            AutoImage = true; // ez csak amiatt, hogy Összesnél is legyen ikon, kiválasztásnál "nem default" ikonokat kap
            Buttons = SelectorButtons.Browse /*| SelectorButtons.SelectAll*/;
            Caption = "Portfolio";
            ImageClick += new EventHandler(ucPortfolioSelector_ImageClick);
        }

        void ucPortfolioSelector_ImageClick(object sender, EventArgs e)
        {
            DefaultBrowseClick();
        }

        private void SetImage(PortfolioItemType type)
        {
            itemType = type;
            // ha "összes" van kiválasztva, auto image van
			AutoImage = (int)Value <= 0;
            if (AutoImage)
            {
                RefreshImage();
                Caption = Language.Translate("Portfolio");
                return;
            }
            switch (itemType)
            {
                case PortfolioItemType.Book:
                    Image = Resources.Book;
                    Caption = Language.Translate("Book");
                    break;
                case PortfolioItemType.Group:
                    Image = Resources.Group;
                    Caption = Language.Translate("Group");
                    break;
                case PortfolioItemType.Portfolio:
                    Image = Resources.Box;
                    Caption = Language.Translate("Portfolio");
                    break;
                case PortfolioItemType.Entity:
                    Image = Resources.House;
                    Caption = Language.Translate("Entity");
                    break;
            }
        }

        protected override void SetValue(object value)
        {
            base.SetValue(value);
            if ((int)value == Constants.AllSelectedValue)
                itemType = PortfolioItemType.All;
            SetImage(itemType);
        }

        public override void DefaultBrowseClick()
        {
            if (PortfolioBrowse != null)
            {
				PortfolioBrowseArgs args = new PortfolioBrowseArgs(false, ((int)Value > 0 ? Text : ""), (int)Value, itemType);
                PortfolioBrowse(this, args);

				if (!args.Canceled && (args.Value != (int)Value || args.ItemType != itemType))
                {
					if ((args.Value <= 0 && args.Value != (int)State) || ((int)Value > 0 && State != SelectorStates.ValueSet))
                    {
                        if (args.ItemType == PortfolioItemType.All)
                            State = SelectorStates.All;
                        else
                            State = SelectorStates.ValueSet;
                    }
                    Assign(args.Value, args.InstrumentName);
                    SetImage(args.ItemType);
                    OnValueChanged(new EventArgs());
                }
            }
        }

        #endregion
    }
    #region Delegate, EventArg leszármaztatás

    public delegate void PortfolioBrowseHandler(ucPortfolioSelector sender, PortfolioBrowseArgs e);

    public class PortfolioBrowseArgs : EventArgs
    {
        private bool canceled;
        private string instrumentName;
        private int value;
        private ucPortfolioSelector.PortfolioItemType itemType;

        public bool Canceled
        {
            get { return canceled; }
            set { canceled = value; }
        }

        public string InstrumentName
        {
            get { return instrumentName; }
            set { instrumentName = value; }
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public ucPortfolioSelector.PortfolioItemType ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        public PortfolioBrowseArgs(bool canceled, string instrumentName, int value, ucPortfolioSelector.PortfolioItemType itemType)
        {
            Canceled = canceled;
            InstrumentName = instrumentName;
            Value = value;
            ItemType = itemType;
        }

    }

    #endregion
}

