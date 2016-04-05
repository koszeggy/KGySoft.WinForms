using System;
using System.Collections.Generic;
using System.Text;

namespace KGySoft.Controls
{
	/// <summary>
	/// Arguments for handling an <see cref="ucCustomSelector.AutoFind"/> event.
	/// </summary>
	public class AutoFindEventArgs: EventArgs
	{
		#region Properties

	    /// <summary>
	    /// Get the text that was typed into the text field.
	    /// </summary>
	    public string SearchPattern { get; private set; }

	    /// <summary>
	    /// Gets or sets the value that is associated with the found or selected item.
	    /// Set this property to associate a value with the found text. By setting this property
	    /// text of the selector will be calculated by <see cref="ucCustomSelector.GetTextByValue"/> or by its derived method.
	    /// By default, value of this property is the object that represents the not selected value.
	    /// If in the used scenario <see cref="ucCustomSelector.Value"/> has no special meaning,
	    /// then you may set this property to <see cref="ControlTools.UndefinedValue"/> so <see cref="ucCustomSelector.Text"/>
	    /// will not be changed - you have to do it manually.
	    /// To fallback to default logic set <see cref="DefaultAutoFind"/> to <c>true</c>.
	    /// </summary>
	    public object Value { get; set; }

	    /// <summary>
	    /// Gets or sets whether <see cref="ucCustomSelector.DefaultAutoFind"/> should be called.
	    /// Set this property to <c>true</c> to fallback to default logic instead of accepting <see cref="Value"/>.
	    /// </summary>
        public bool DefaultAutoFind { get; set; }

	    #endregion

		#region Constructor

		internal AutoFindEventArgs(string searchPattern, object notSelectedValue)
		{
			this.Value = notSelectedValue;
			this.SearchPattern = searchPattern;
		}

		#endregion
	}
}
