using System;
using System.ComponentModel;

namespace KGySoft.Controls
{
    /// <summary>
    /// Provides arguments for the <see cref="AdvancedErrorProvider.SetMessage">AdvancedErrorProvider.SetMessage</see> event.
    /// </summary>
    /// <seealso cref="AdvancedErrorProvider" />
    public class SetMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current item of the underlying data source.
        /// Can be <see langword="null"/> if the message is required for a binding error.
        /// </summary>
        public object Current { get; }

        /// <summary>
        /// Gets the name of the property for which the message is requested.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets or sets the message to be displayed. If the message is retrieved due to a binding error or the <see cref="Current"/> item
        /// implements the <see cref="IDataErrorInfo"/> interface, then this property may already contain a value.
        /// </summary>
        public string Message { get; set; }

        internal SetMessageEventArgs(object current, string propertyName, string message)
        {
            Current = current;
            PropertyName = propertyName;
            Message = message;
        }
    }
}