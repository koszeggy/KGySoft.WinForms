#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents appearance status information of a control that supports fading animations.
    /// </summary>
    public sealed class ControlAppearanceState
    {
        #region Constants

        internal const FadingOptions CustomChange = (FadingOptions)(1 << 30);
        internal const FadingOptions NonStandardChanges = FadingOptions.TextChange | FadingOptions.Appearing | FadingOptions.ColorChange | CustomChange;

        #endregion

        #region Fields

        private readonly int systemPartId;
        private readonly int systemStateId;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets whether the control should be painted in enabled state.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Gets whether the control should be painted in pressed state.
        /// </summary>
        public bool Pressed { get; internal set; }

        /// <summary>
        /// Gets whether the control should be painted in hovered state.
        /// </summary>
        public bool Hovered { get; internal set; }

        /// <summary>
        /// Gets whether the control should be painted as a default button.
        /// </summary>
        public bool IsDefault { get; internal set; }

        /// <summary>
        /// When applicable, gets the check state to be painted for the control.
        /// </summary>
        public CheckState CheckState { get; internal set; }

        /// <summary>
        /// Gets the text that should be painted for the control.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets whether the control should be painted in a visible state.
        /// When the control is invisible, its background (parent area) should be painted.
        /// </summary>
        public bool Visible { get; internal set; }

        /// <summary>
        /// Gets the back color that should be used when the control is painted.
        /// </summary>
        public Color BackColor { get; internal set; }

        /// <summary>
        /// Gets the fore color that should be used when the control is painted.
        /// </summary>
        public Color ForeColor { get; internal set; }

        #endregion

        #region Internal Properties

        internal int SystemPartId
        {
            get { return systemPartId; }
        }

        internal int SystemStateId
        {
            get { return systemStateId; }
        }

        /// <summary>
        /// Gets or sets a custom state that may help to indicate any non-standard change
        /// </summary>
        internal object CustomState { get; set; }

        #endregion

        #endregion

        #region Constructors

        internal ControlAppearanceState(int systemPartId, int systemStateId)
        {
            this.systemPartId = systemPartId;
            this.systemStateId = systemStateId;
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode()
        {
            return systemStateId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ControlAppearanceState other = obj as ControlAppearanceState;
            if (other == null)
                return false;

            return systemStateId == other.systemStateId
                && BackColor == other.BackColor
                && ForeColor == other.ForeColor
                && Text == other.Text
                && Visible == other.Visible
                && systemPartId == other.systemPartId
                && Enabled == other.Enabled
                && Pressed == other.Pressed
                && Hovered == other.Hovered
                && IsDefault == other.IsDefault
                && CheckState == other.CheckState
                && Equals(CustomState, other.CustomState);
        }

        public override string ToString()
        {
            return string.Format("Part: {0}; State: {1}", SystemPartId, SystemStateId);
        }

        #endregion

        #region Internal Methods

        internal bool EqualsWithOptions(ControlAppearanceState other, FadingOptions options)
        {
            if (other == null || systemPartId != other.systemPartId)
                return false;

            if (options == FadingOptions.None)
                return true;

            if ((options & FadingOptions.AnyChange) != FadingOptions.None)
                return Equals(other);

            if ((options & FadingOptions.StandardEffects) != FadingOptions.None &&
                (SystemStateId != other.SystemStateId))
            {
                return false;
            }

            if ((options & FadingOptions.Appearing) != FadingOptions.None &&
                (Visible != other.Visible))
            {
                return false;
            }

            if ((options & FadingOptions.ColorChange) != FadingOptions.None &&
                (Enabled == other.Enabled) &&
                (BackColor != other.BackColor || ForeColor != other.ForeColor))
            {
                return false;
            }

            if ((options & FadingOptions.TextChange) != FadingOptions.None &&
                (Text != other.Text))
            {
                return false;
            }

            if ((options & CustomChange) != FadingOptions.None &&
                (CustomState != other.CustomState))
            {
                return false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
