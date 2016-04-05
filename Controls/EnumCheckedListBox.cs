extern alias lang;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Data;

using Language = lang::KGySoft.Libraries.Language.Language;

namespace KGySoft.Controls
{
	/// <summary>
	/// A CheckListBox that gains its values from a Flags enum (<see cref="EnumCheckedListBox.EnumType"/>).
	/// The state of the checkboxes can be get or set via <see cref="EnumCheckedListBox.Value"/> property.
	/// </summary>
	public partial class EnumCheckedListBox: CheckedListBox
	{
		#region Fields

		private bool translate = true;
		private ucAllInvertNone allInvertNone;
		private Type enumType = null;
		private List<int> values = new List<int>();
		private bool itemChecking = false;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the state of a check box has been changed.
		/// </summary>
		[Category("EnumCheckedListBox")]
		[Description("Occurs when the state of a check box has been changed.")]
		public event EventHandler CheckedChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets whether the items should be translated.
		/// </summary>
		[Category("EnumCheckedListBox")]
		[Description("Gets or sets whether the items should be translated.")]
		[DefaultValue(true)]
		public bool Translate
		{
			get { return translate; }
			set { translate = value; }
		}

		/// <summary>
		/// Gets or sets the associated Flags enumerator.
		/// </summary>
		[Browsable(true)]
		[Category("EnumCheckedListBox")]
		[Description("Gets or sets the associated Flags enumerator.")]
		[TypeConverter(typeof(EnumConverter))]
		public Type EnumType
		{
			get { return enumType; }
			set
			{
				enumType = value;
				FillEnum();
			}
		}

		/// <summary>
		/// Gets or sets the value of the <see cref="EnumType"/> based on the scheck states.
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

				foreach (int i in this.CheckedIndices)
					ret |= values[i];

				return ret;
			}
			set
			{
				for (int i = 0; i < Items.Count; i++)
					this.SetItemChecked(i, false);

				for (int i = 0; i < values.Count; i++)
					if ((values[i] & value) > 0)
						this.SetItemChecked(i, true);
			}
		}

		/// <summary>
		/// Gets or sets the associated <see cref="ucAllInvertNone"/> control.
		/// </summary>
		[Browsable(true)]
		[Category("EnumCheckedListBox")]
		[Description("Gets or sets the associated ucAllInvertNone control.")]
		public ucAllInvertNone AllInvertNoneControl
		{
			get { return allInvertNone; }
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

		#region Construction

		public EnumCheckedListBox()
		{
			InitializeComponent();
		}

        //public EnumCheckedListBox(IContainer container)
        //{
        //    container.Add(this);

        //    InitializeComponent();
        //}

		#endregion

		#region Public methods

        /// <summary>
        /// Selects all item.
        /// </summary>
		public void SelectAll()
		{
			for (int i = 0; i < Items.Count; i++)
				this.SetItemChecked(i, true);
		}

        /// <summary>
        /// Deselects all item.
        /// </summary>
		public void SelectNone()
		{
			for (int i = 0; i < Items.Count; i++)
				this.SetItemChecked(i, false);
		}

        /// <summary>
        /// Inverts all item.
        /// </summary>
		public void SelectInvert()
		{
			for (int i = 0; i < Items.Count; i++)
				this.SetItemChecked(i, !GetItemChecked(i));
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			if (allInvertNone != null)
				allInvertNone.ButtonPressed -= allInvertNone_ButtonPressed;
		}

		#endregion

		#region Overridden methods

		protected override void OnItemCheck(ItemCheckEventArgs ice)
		{

			if (itemChecking)
				return;

			itemChecking = true;
			try
			{
				this.SetItemChecked(ice.Index, ice.NewValue == CheckState.Checked);
				if (CheckedChanged != null)
					CheckedChanged(this, EventArgs.Empty);
			}
			finally
			{
				itemChecking = false;
			}
		}

		#endregion

		#region Private implementation

		void allInvertNone_ButtonPressed(object sender, AllInvertNoneEventArgs e)
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

		private void FillEnum()
		{
			if (enumType == null)
				return;

			this.Items.Clear();
			values.Clear();
			foreach (object i in Enum.GetValues(enumType))
			{
				if (Convert.ToInt32(i) == 0)
					continue;
				string item = Enum.ToObject(enumType, Convert.ToInt32(i)).ToString();
				this.Items.Add(translate ? Language.Translate(item) : item);
				values.Add(Convert.ToInt32(i));
			}
		}

		#endregion
	}
}

