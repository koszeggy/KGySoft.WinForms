using System;
using System.Windows.Forms;
using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms.Test.Forms
{
    internal partial class frmFadingDemo: ControlsTestBaseForm
    {
        public frmFadingDemo()
        {
            InitializeComponent();
        }
    }

    internal enum LabelStates
    {
        Normal,
        Disabled,
        Hovered
    }

    internal class FadingLabelDemo: Label, ISupportsFading<LabelStates>
    {
        private LabelStates state;
        private FadingPainter<LabelStates> fadingPainter;
        private bool isAdjustingPropertyForAppearance;

        public FadingLabelDemo()
        {
            FadingAnimationsEnabled = true;
            FadingAnimationDefaultSpeed = 500;
            fadingPainter = new FadingPainter<LabelStates>(this, LabelStates.Normal);

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

        public LabelStates State
        {
            get { return state; }
        }

        // Try to change it in the Property Grid
        public int FadingAnimationDefaultSpeed { get; set; }

        public int GetFadingAnimationSpeed(LabelStates stateFrom, LabelStates stateTo)
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

        public void PaintState(LabelStates state, PaintEventArgs e)
        {
            // This is how to make a workaround for a property (here Enabled) that would raise Paint again when temporaly switched back
            bool origEnabled = Enabled;
            isAdjustingPropertyForAppearance = true;

            try
            {
                Enabled = state != LabelStates.Disabled;
                if (state == LabelStates.Hovered)
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
            state = Enabled ? LabelStates.Normal : LabelStates.Disabled;
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Invalidating manually to raise a Paint
            state = LabelStates.Hovered;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Invalidating manually to raise a Paint
            state = LabelStates.Normal;
            base.OnMouseLeave(e);
            Invalidate();
        }
    }
}