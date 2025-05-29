#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportsFading.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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

        /// <summary>
        /// Gets whether the <see cref="IWin32Window.Handle"/> property is available.
        /// </summary>
        bool IsHandleCreated { get; }

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
