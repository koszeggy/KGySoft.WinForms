#region Used namespaces

using System.Windows.Forms;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a control that supports fading animations based on a custom state type.
    /// </summary>
    /// <typeparam name="TState">A type that represents the state of the object. It should be meningfully equatable.</typeparam>
    /// <remarks>
    /// A <see cref="FadingPainter{TState}"/> instance should be created along with the control.
    /// Double buffering should be disabled, otherwise animations will not work. To avoid
    /// flickering do not draw anything in overridden <see cref="Control.OnPaintBackground"/>.
    /// Override <see cref="Control.OnPaint"/> and call <see cref="FadingPainter{TState}.Paint"/>
    /// from there, which uses double buffering internally. That will invoke <see cref="PaintState"/>
    /// where the control should be painted with specified state.
    /// </remarks>
    public interface ISupportsFading<TState>: IWin32Window
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work in Windows Vista and above, with non-classic themes.
        /// </summary>
        bool FadingAnimationsEnabled { get; set; }

        /// <summary>
        /// Gets the current state of the object. When changes the control should be invalidated.
        /// When the control is needed to be repainted but there is no state difference, no animation will be performed.
        /// </summary>
        TState State { get; }

        /// <summary>
        /// Gets or sets default fading animation speed in milliseconds. 0 means immediate change.
        /// </summary>
        int FadingAnimationDefaultSpeed { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets speed of the animation between two specified states in milliseconds.
        /// </summary>
        /// <param name="stateFrom">Old state.</param>
        /// <param name="stateTo">New state.</param>
        /// <returns>Fading animation speed in milliseconds. Zero means immediate change. Less than zero
        /// means <see cref="FadingAnimationDefaultSpeed"/> should be used.</returns>
        int GetFadingAnimationSpeed(TState stateFrom, TState stateTo);

        /// <summary>
        /// Implementer should perform any painting operation here with provided state.
        /// </summary>
        void PaintState(TState state, PaintEventArgs e);

        #endregion
    }
}
