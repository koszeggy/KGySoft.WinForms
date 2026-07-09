#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedPanel.cs
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Controls.Design;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a panel with advanced <see cref="BorderStyle"/> options.
    /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm">online help</a> of the <see cref="AdvancedLabel"/> class for an image example.</div>
    /// </summary>
    /// <example>
    /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="AdvancedLabel"/> class for an image example, as it uses the same range of possible border styles as <see cref="AdvancedPanel"/>.</note>
    /// </example>
    [ToolboxBitmap(typeof(AdvancedPanel), "Resources.Toolbox.AdvancedPanel.png")]
    [Designer(typeof(AdvancedPanelDesigner))]
    public class AdvancedPanel : Panel, ISafePaintBackground
    {
        #region Fields

        private AdvancedBorderStyle borderStyle = AdvancedBorderStyle.Raised;
        private int borderWidth = 1;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the border style of the <see cref="AdvancedPanel"/>.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/winforms/html/T_KGySoft_WinForms_Controls_AdvancedLabel.htm">online help</a> of the <see cref="AdvancedLabel"/> class for an image example.</div>
        /// </summary>
        /// <example>
        /// <note type="tip">See the <strong>Examples</strong> section of the <see cref="AdvancedLabel"/> class for an image example, as it uses the same range of possible border styles as <see cref="AdvancedPanel"/>.</note>
        /// </example>
        /// <seealso cref="AdvancedBorderStyle"/>
        [Category("AdvancedPanel")]
        [Description("Gets or sets the border style of the AdvancedPanel.")]
        [DefaultValue(typeof(AdvancedBorderStyle), "Raised")]
        public new AdvancedBorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                if (borderStyle == value)
                    return;

                borderStyle = value;
                int previousWidth = borderWidth;
                borderWidth = value switch
                {
                    AdvancedBorderStyle.None => 0,
                    AdvancedBorderStyle.FixedSingle or AdvancedBorderStyle.Raised or AdvancedBorderStyle.Sunken => 1,
                    AdvancedBorderStyle.Flat or AdvancedBorderStyle.RaisedHigh or AdvancedBorderStyle.SunkenLow
                        or AdvancedBorderStyle.RaisedFrame or AdvancedBorderStyle.SunkenFrame => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value))
                };

                if (OSHelper.IsWindows)
                {
                    InvalidateNC();
                    return;
                }

                // To rearrange docked controls when the border is part of the client area. Unfortunately, it's not working great on Linux.
                if (previousWidth != borderWidth)
                    PerformLayout();
                Invalidate();
            }
        }

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override Padding DefaultPadding => OSHelper.IsWindows ? Padding.Empty : new Padding(borderWidth);

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Constants.WM_NCCALCSIZE when OSHelper.IsWindows:
                    base.WndProc(ref m);
                    if (m.WParam == IntPtr.Zero || m.WParam == new IntPtr(1))
                        NCHelper.CalcSizeNC(m.LParam, borderWidth);
                    return;

                case Constants.WM_NCPAINT when OSHelper.IsWindows:
                    base.WndProc(ref m);
                    NCHelper.DrawBorderNC(m.HWnd, Size, borderStyle);
                    return;

                case Constants.WM_MOUSEWHEEL or Constants.WM_HSCROLL or Constants.WM_VSCROLL when !OSHelper.IsWindows:
                    base.WndProc(ref m);
                    Invalidate();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (OSHelper.IsWindows)
                InvalidateNC();
        }

#if NET && !NET10_0_OR_GREATER
        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // workaround for https://github.com/dotnet/winforms/issues/13784
            base.OnPaintBackground(e);
            e.Graphics.GetHdc();
            e.Graphics.ReleaseHdc();
        }
#endif

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (OSHelper.IsWindows || borderStyle == AdvancedBorderStyle.None)
                return;
            e.Graphics.DrawBorder(borderStyle, ClientRectangle);
        }

        #endregion

        #region Private Methods

        private void InvalidateNC()
        {
            if (IsHandleCreated)
                NCHelper.InvalidateNC(Handle);
        }

        #endregion

        #endregion
    }
}
