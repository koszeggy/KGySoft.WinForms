namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a read-only capable control
    /// </summary>
    public interface IReadOnlyCapable
    {
        /// <summary>
        /// Gets or sets read-only status of the control.
        /// </summary>
        bool ReadOnly { get; set; }
    }
}
