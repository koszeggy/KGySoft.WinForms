using System;
using System.Windows.Forms;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Example.Controls
{
    internal enum LabelStatus
    {
        Normal,
        Disabled,
        Hovered
    }

    internal class FadingLabelDemo: Label, ISupportsFading<LabelStatus>
    {
        private LabelStatus state;
        private FadingPainter<LabelStatus> fadingPainter;
        private bool isAdjustingPropertyForAppearance;

        public FadingLabelDemo()
        {
            FadingAnimationsEnabled = true;
            FadingAnimationDefaultSpeed = 500;
            fadingPainter = new FadingPainter<LabelStatus>(this, LabelStatus.Normal);

            // Double buffering should be disabled
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadingPainter.Dispose();
            }

            base.Dispose(disposing);
        }

        // Try to change it in the Property Grid
        public bool FadingAnimationsEnabled { get; set; }

        public LabelStatus State
        {
            get { return state; }
        }

        // Try to change it in the Property Grid
        public int FadingAnimationDefaultSpeed { get; set; }

        public int GetFadingAnimationSpeed(LabelStatus stateFrom, LabelStatus stateTo)
        {
            return FadingAnimationDefaultSpeed;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do not paint background here
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (isAdjustingPropertyForAppearance)
                return;

            fadingPainter.Paint(e);
        }

        public void PaintState(LabelStatus state, PaintEventArgs e)
        {
            // This is how to make a workaround for a property (here Enabled) that would raise Paint again when temporaly switched back
            bool origEnabled = Enabled;
            isAdjustingPropertyForAppearance = true;

            try
            {
                Enabled = state != LabelStatus.Disabled;
                if (state == LabelStatus.Hovered)
                    e.Graphics.Clear(ControlPaint.LightLight(BackColor));
                else
                    base.OnPaintBackground(e);

                base.OnPaint(e);
            }
            finally
            {
                Enabled = origEnabled;
                isAdjustingPropertyForAppearance = false;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            // Changing enabled will raise a Paint
            state = Enabled ? LabelStatus.Normal : LabelStatus.Disabled;
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Invalidating manually to raise a Paint
            state = LabelStatus.Hovered;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Invalidating manually to raise a Paint
            state = LabelStatus.Normal;
            base.OnMouseLeave(e);
            Invalidate();
        }
    }
}