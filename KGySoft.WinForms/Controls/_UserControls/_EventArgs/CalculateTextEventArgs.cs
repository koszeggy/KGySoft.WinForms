using System;

namespace KGySoft.WinForms.Controls
{
	/// <summary>
	/// Arguments for handling an <see cref="ucCustomSelector.CalculateText"/> event.
	/// </summary>
	public class CalculateTextEventArgs: EventArgs
	{
		#region Fields

		private string text;
		private object value;

		#endregion

		#region Properties

		/// <summary>
		/// Get or sets the text that is associated by <see cref="Value"/>.
		/// </summary>
		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		/// <summary>
		/// Gets the value that is associated with the found or selected item.
		/// </summary>
		public object Value
		{
			get { return value; }
		}

		#endregion

		#region Constructor

		internal CalculateTextEventArgs(object value, string text)
		{
			this.value = value;
			this.text = text ?? String.Empty;
		}

		#endregion
	}
}
