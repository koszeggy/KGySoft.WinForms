#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumCheckedListBox.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

using KGySoft.Libraries.Language;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms
#pragma warning disable CS8605 // Unboxing a possibly null value - inconsistent nullability annotations on different platforms
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A CheckListBox that gains its values from a Flags enum (<see cref="EnumCheckedListBox.EnumType"/>).
    /// The state of the checkboxes can be got or set via <see cref="EnumCheckedListBox.Value"/> property.
    /// </summary>
    [Obsolete("This class uses obsolete techniques and is not recommended to use it anymore.")]
    [ToolboxItem(false)]
    public partial class EnumCheckedListBox : CheckedListBox
    {
        #region Fields

        private readonly List<int> values = new List<int>();

        private bool translate = true;
        private ucAllInvertNone? allInvertNone;
        private Type? enumType;
        private bool itemChecking;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the state of a check box has been changed.
        /// </summary>
        [Category("EnumCheckedListBox")]
        [Description("Occurs when the state of a check box has been changed.")]
        public event EventHandler? CheckedChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the items should be translated.
        /// </summary>
        [DefaultValue(false)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Translate
        {
            get => translate;
            set => translate = value;
        }

        /// <summary>
        /// Gets or sets the associated Flags enumerator.
        /// </summary>
        [Browsable(true)]
        [Category("EnumCheckedListBox")]
        [Description("Gets or sets the associated Flags enumerator.")]
        [TypeConverter(typeof(EnumConverter))]
        [DefaultValue(null)]
        public Type? EnumType
        {
            get => enumType;
            set
            {
                enumType = value;
                FillEnum();
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="EnumType"/> based on the check states.
        /// </summary>
        [Category("EnumCheckedListBox")]
        [Description("Gets or sets the value of the EnumType based on the scheck states.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        [DefaultValue(0)]
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public int Value
        {
            get
            {
                int ret = 0;

                foreach (int i in CheckedIndices)
                    ret |= values[i];

                return ret;
            }
            set
            {
                for (int i = 0; i < Items.Count; i++)
                    SetItemChecked(i, false);

                for (int i = 0; i < values.Count; i++)
                    if ((values[i] & value) > 0)
                        SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// Gets or sets the associated <see cref="ucAllInvertNone"/> control.
        /// </summary>
        [Browsable(true)]
        [Category("EnumCheckedListBox")]
        [Description("Gets or sets the associated ucAllInvertNone control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ucAllInvertNone? AllInvertNoneControl
        {
            get => allInvertNone;
            set
            {
                if (allInvertNone != value)
                {
                    if (allInvertNone != null)
                        allInvertNone.ButtonPressed -= allInvertNone_ButtonPressed;
                    allInvertNone = value;
                    if (allInvertNone != null)
                        allInvertNone.ButtonPressed += allInvertNone_ButtonPressed;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCheckedListBox"/> class.
        /// </summary>
        public EnumCheckedListBox()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Selects all item.
        /// </summary>
        public void SelectAll()
        {
            for (int i = 0; i < Items.Count; i++)
                SetItemChecked(i, true);
        }

        /// <summary>
        /// Deselects all item.
        /// </summary>
        public void SelectNone()
        {
            for (int i = 0; i < Items.Count; i++)
                SetItemChecked(i, false);
        }

        /// <summary>
        /// Inverts all item.
        /// </summary>
        public void SelectInvert()
        {
            for (int i = 0; i < Items.Count; i++)
                SetItemChecked(i, !GetItemChecked(i));
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (allInvertNone != null)
                allInvertNone.ButtonPressed -= allInvertNone_ButtonPressed;
        }

        /// <inheritdoc />
        protected override void OnItemCheck(ItemCheckEventArgs ice)
        {

            if (itemChecking)
                return;

            itemChecking = true;
            try
            {
                SetItemChecked(ice.Index, ice.NewValue == CheckState.Checked);
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                itemChecking = false;
            }
        }

        #endregion

        #region Private Methods

        private void FillEnum()
        {
            if (enumType == null)
                return;

            Items.Clear();
            values.Clear();
            foreach (object i in Enum.GetValues(enumType))
            {
                if (Convert.ToInt32(i, CultureInfo.InvariantCulture) == 0)
                    continue;
                string item = Enum.ToObject(enumType, Convert.ToInt32(i, CultureInfo.InvariantCulture)).ToString()!;
                Items.Add(translate ? Language.Translate(item) : item);
                values.Add(Convert.ToInt32(i, CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Event handlers
#pragma warning disable IDE1006 // Naming Styles

        void allInvertNone_ButtonPressed(object? sender, AllInvertNoneEventArgs e)
        {
            switch (e.ButtonType)
            {
                case InvertButtonTypes.All:
                    SelectAll();
                    break;
                case InvertButtonTypes.Invert:
                    SelectInvert();
                    break;
                case InvertButtonTypes.None:
                    SelectNone();
                    break;
            }
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion
    }
}
