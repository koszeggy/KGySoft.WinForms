namespace KGySoft.Controls
{
    /// <summary>
    /// Represents the possible states of the progress bar of a <see cref="TaskDialog"/>.
    /// </summary>
    public enum TaskDialogProgressBarState
    {
        /// <summary>
        /// Represents the normal progress bar state.
        /// </summary>
        Normal,

        /// <summary>
        /// Represents the error (red) progress bar state. In case of a marquee progress bar,
        /// setting this state stops the animation
        /// </summary>
        Error,

        /// <summary>
        /// Represents the paused (yellow) progress bar state. In case of a marquee progress bar,
        /// setting this state stops the animation
        /// </summary>
        Paused,
    }
}
