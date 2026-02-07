using System;
using System.ComponentModel;

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides extension methods for the <see cref="EventHandlerList"/> type.
    /// </summary>
    public static class EventHandlerExtensions
    {
        #region Methods

        /// <summary>
        /// Gets the event handler of the specified type from the <see cref="EventHandlerList"/> instance.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
        /// <param name="handlers">The <see cref="EventHandlerList"/> instance to het the handler delegate from.</param>
        /// <param name="key">The same key that is used for the <see cref="EventHandlerList.AddHandler"/> and <see cref="EventHandlerList.RemoveHandler"/> methods
        /// when the event is subscribed or unsubscribed.</param>
        /// <returns>The event handler delegate of the specified type, or <see langword="null"/> if no such handler is found or the specified <typeparamref name="TDelegate"/> type does not match.</returns>
        public static TDelegate? GetHandler<TDelegate>(this EventHandlerList? handlers, object key) where TDelegate : Delegate => handlers?[key] as TDelegate;

        #endregion
    }
}
