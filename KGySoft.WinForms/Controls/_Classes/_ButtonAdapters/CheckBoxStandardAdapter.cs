#region Used namespaces

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal sealed class CheckBoxStandardAdapter: CheckBoxBaseAdapter
    {
        #region Constructors

        internal CheckBoxStandardAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
        {
            if (IsButton)
            {
                return ButtonAdapter.GetPreferredSizeCore(g, proposedSize, state);
            }

            return Layout(g, state).GetPreferredSizeCore(g, proposedSize);
        }

        internal override void PaintDown(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintDown(e);
            }
            else
            {
                PaintUp(e);
            }
        }

        internal override void PaintOver(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintOver(e);
            }
            else
            {
                PaintUp(e);
            }
        }

        internal override void PaintUp(PaintStateEventArgs e)
        {
            if (IsButton)
            {
                ButtonAdapter.PaintUp(e);
            }
            else
            {
                Graphics g = e.Graphics;
                ControlAppearanceState state = e.State;
                ColorData colors = ColorData.Calculate(g, state.BackColor, state.ForeColor);
                LayoutData layout = Layout(g, state).Layout(g);
                PaintButtonBackground(e, ButtonInstance.ClientRectangle, colors.buttonFace);
                int focusWidth = layout.focus.X & 1;
                if (!Application.RenderWithVisualStyles)
                {
                    focusWidth = 1 - focusWidth;
                }
                if (!layout.options.dotNetOneButtonCompat)
                {
                    layout.textBounds.Offset(-1, -1);
                }
                layout.imageBounds.Offset(-1, -1);
                layout.focus.Offset(-(focusWidth + 1), -2);
                layout.focus.Width = (layout.textBounds.Width + layout.imageBounds.Width) - 1;
                layout.focus.Intersect(layout.textBounds);
                if ((!layout.options.textAlign.AnyLeft() && layout.options.useCompatibleTextRendering) && layout.options.font.Italic)
                {
                    layout.focus.Width += 2;
                }
                PaintImage(e, layout);
                DrawCheckBox(e, layout);
                PaintField(e, layout, colors, true);
            }
        }

        #endregion

        #region Protected Methods

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonStandardAdapter(ButtonInstance);
        }

        protected override LayoutOptions Layout(Graphics graphics, ControlAppearanceState state)
        {
            LayoutOptions options = CommonLayout(state);
            options.checkPaddingSize = 1;
            options.dotNetOneButtonCompat = !Application.RenderWithVisualStyles;
            if (Application.RenderWithVisualStyles)
            {
                //using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                //{
                    options.checkSize = CheckBoxRenderer.GetGlyphSize(graphics, (CheckBoxState)state.SystemStateId).Width;
                //}
            }
            else
            {
                //options.checkSize = ScaleHelper.IsThreadPerMonitorV2Aware
                //    ? ButtonInstance.LogicalToDeviceUnits(options.checkSize)
                //    : (int)(options.checkSize * GetDpiScaleRatio());
                options.checkSize = ButtonInstance.PerMonitorScale(options.checkSize);
            }

            return options;
        }

        #endregion

        #region Private Methods

        private void DrawCheckBox(PaintStateEventArgs e, LayoutData layout)
        {
            ControlAppearanceState state = e.State;
            if (Application.RenderWithVisualStyles)
            {
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(layout.checkBounds.Left, layout.checkBounds.Top), (CheckBoxState)state.SystemStateId);
            }
            else if (state.CheckState == CheckState.Indeterminate)
            {
                ControlPaint.DrawMixedCheckBox(e.Graphics, layout.checkBounds, GetButtonState(state));
            }
            else
            {
                ControlPaint.DrawCheckBox(e.Graphics, layout.checkBounds, GetButtonState(state));
            }
        }

        #endregion

        #endregion
    }
}
