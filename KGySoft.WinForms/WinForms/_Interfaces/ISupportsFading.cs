#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportsFading.cs
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

using System.Windows.Forms;

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents a control that supports buffered fading animations based on a custom state type.
    /// </summary>
    /// <typeparam name="TState">A type that represents the state of the object. It should be meaningfully equatable.</typeparam>
    /// <remarks>
    /// A <see cref="FadingPainter{TState}"/> instance should be created along with the control.
    /// Double buffering should be disabled, otherwise animations will not work. To avoid
    /// flickering do not draw anything in the overridden <see cref="Control.OnPaintBackground">OnPaintBackground</see> method.
    /// Override <see cref="Control.OnPaint">OnPaint</see>, and call <see cref="FadingPainter{TState}.Paint">FadingPainter&lt;TState&gt;.Paint</see>
    /// from there, which uses double buffering internally. That will invoke the <see cref="PaintState">PaintState</see> method implementation
    /// where the control should be painted with the specified state.
    /// </remarks>
    public interface ISupportsFading<TState> : IWin32Window
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether fading animations are enabled for the control.
        /// Animations work on Windows Vista and above when visual styles are enabled.
        /// </summary>
        bool FadingAnimationsEnabled { get; set; }

        /// <summary>
        /// Gets the current state of the object. When it changes, the control should be invalidated.
        /// When the control is repainted but there is no state difference, no animation will be performed.
        /// </summary>
        TState State { get; }

        /// <summary>
        /// Gets or sets the default fading animation speed, in milliseconds. 0 means immediate change.
        /// </summary>
        int FadingAnimationDefaultSpeed { get; set; }

        /// <summary>
        /// Gets whether the <see cref="IWin32Window.Handle"/> property is available.
        /// </summary>
        bool IsHandleCreated { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the speed of the animation between two specified states, in milliseconds.
        /// </summary>
        /// <param name="stateFrom">The previous state.</param>
        /// <param name="stateTo">The new state.</param>
        /// <returns>The fading animation speed in milliseconds. Zero means immediate change. Less than zero
        /// means that <see cref="FadingAnimationDefaultSpeed"/> should be used.</returns>
        int GetFadingAnimationSpeed(TState stateFrom, TState stateTo);

        /// <summary>
        /// The implementer method should perform all painting operations here, using the provided <paramref name="state"/>.
        /// </summary>
        /// <param name="state">The current state of the object. It should contain the properties, whose changes are reflected in the fading animation.</param>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        void PaintState(TState state, PaintEventArgs e);

        #endregion
    }
}
