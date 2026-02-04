#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FadingPaintControl.cs
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms.Example.Controls
{
    #region Enumerations

    // These properties are participating in fading animation
    internal struct FadingPaintState
    {
        internal Color ForeColor { get; set; }
        internal Color BackColor { get; set; }
        internal string FadingText { get; set; }
    }

    #endregion

    internal class FadingPaintControl : Control, ISupportsFading<FadingPaintState>
    {
        #region Fields

        private readonly FadingPainter<FadingPaintState> fadingPainter;

        private string nonFadingText;
        private bool isHovered;

        #endregion

        #region Properties

        #region Public Properties
        
        public bool FadingAnimationsEnabled { get; set; }
        public int FadingAnimationDefaultSpeed { get; set; }

#if NETCOREAPP3_0_OR_GREATER
        [AllowNull]
#endif
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public string NonFadingText
        {
            get => nonFadingText;
            set
            {
                nonFadingText = value;
                Invalidate();
            }
        }

        private Color EffectiveForeColor => !Enabled ? SystemColors.GrayText
            : isHovered ? SystemColors.HighlightText
            : ForeColor;

        #endregion

        #region Explicitly Implemented Inteface Properties

        FadingPaintState ISupportsFading<FadingPaintState>.State => GetAppearance();

        #endregion

        #endregion

        #region Constructors

        public FadingPaintControl()
        {
            FadingAnimationsEnabled = true;
            FadingAnimationDefaultSpeed = 500;
            fadingPainter = new FadingPainter<FadingPaintState>(this, GetAppearance());

            // Double buffering should be disabled to be able to use buffered animations. A double buffer will be used internally by FadingPainter.
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);

            Text = @"This control uses fading animations when its hovered status, ForeColor, BackColor or Text changes.";
            nonFadingText = "This text does not participate in fading animations";
        }

        #endregion

        #region Methods

        #region Public Methods

        public int GetFadingAnimationSpeed(FadingPaintState stateFrom, FadingPaintState stateTo) => FadingAnimationDefaultSpeed;

        // Every painting should be performed here. You can turn it into an explicit implementation and add a protected OnPaintState with an event.
        public void PaintState(FadingPaintState state, PaintEventArgs e)
        {
            // Take every property from state that should participate in fading animation.
            e.Graphics.Clear(state.BackColor);
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding;

            Rectangle bounds = ClientRectangle;
            bounds.Height /= 2;

            // This is the fading text. Text and color is taken from state. You can include Font to animate Font change as well.
            TextRenderer.DrawText(e.Graphics, state.FadingText, Font, bounds, state.ForeColor, flags);
            bounds.Y += bounds.Height;

            // Using NonFadingText and EffectiveForeColor directly from the control, so their change is updated immediately.
            TextRenderer.DrawText(e.Graphics, NonFadingText, Font, bounds, EffectiveForeColor, flags);
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                fadingPainter.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do not paint the background here if it participates in fading animation. Include it into PaintState instead.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // do not call base, just call fadingPainter.Paint(e) to invoke PaintState.
            fadingPainter.Paint(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }

        #endregion

        #region Private Methods

        private FadingPaintState GetAppearance() => new()
        {
            BackColor = isHovered ? SystemColors.Highlight : BackColor,
            ForeColor = EffectiveForeColor,
            FadingText = Text
        };

        #endregion

        #endregion
    }
}
