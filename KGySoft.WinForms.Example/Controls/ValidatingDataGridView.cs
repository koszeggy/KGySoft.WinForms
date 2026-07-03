#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ValidatingDataGridView.cs
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
using System.Drawing;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Drawing;

#endregion

namespace KGySoft.WinForms.Example.Controls
{
    // Just a DataGridView with IValidatingObject support.
    // Not a public AdvancedDataGridView, because it lacks many features other advanced controls in KGySoft.WinForms have, such as auto-scaling, etc.
    // A more advanced version can be found in the KGySoft.Drawing.Tools repo, but it still would need some polishing before making it a public control available in KGySoft.WinForms:
    // https://github.com/koszeggy/KGySoft.Drawing.Tools/blob/master/KGySoft.Drawing.ImagingTools/View/Controls/AdvancedDataGridView.cs
    internal class ValidatingDataGridView : DataGridView
    {
        #region Constants

        private const int WM_DPICHANGED_BEFOREPARENT = 0x02E2;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly Size iconReferenceSize = new Size(16, 16);

        #endregion

        #region Instance Fields

        private Bitmap? errorIcon;
        private Bitmap? warningIcon;
        private Bitmap? infoIcon;

        #endregion

        #endregion

        #region Properties

        private Bitmap ErrorIcon => errorIcon ??= ToScaledBitmap(Icons.SystemError);
        private Bitmap WarningIcon => warningIcon ??= ToScaledBitmap(Icons.SystemWarning);
        private Bitmap InfoIcon => infoIcon ??= ToScaledBitmap(Icons.SystemInformation);

        #endregion

        #region Methods

        #region Protected Methods

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DPICHANGED_BEFOREPARENT:
                    base.WndProc(ref m);
                    ReleaseIcons();
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ErrorIcon);
            if ((e.PaintParts & DataGridViewPaintParts.ErrorIcon) != DataGridViewPaintParts.None)
                DrawValidationIcon(e);

            e.Handled = true;
        }

        protected override void OnRowErrorTextNeeded(DataGridViewRowErrorTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || Rows[e.RowIndex].DataBoundItem is not IValidatingObject validatingObject)
                return;

            e.ErrorText = validatingObject.ValidationResults.Message;
        }

        protected override void OnCellErrorTextNeeded(DataGridViewCellErrorTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || Rows[e.RowIndex].DataBoundItem is not IValidatingObject validatingObject)
                return;

            ValidationResultsCollection validationResults = validatingObject.ValidationResults;
            e.ErrorText = validationResults.TryGetFirstWithHighestSeverity(Columns[e.ColumnIndex].DataPropertyName)?.Message!;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ReleaseIcons();

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private Bitmap ToScaledBitmap(Icon icon)
        {
            using (icon)
            {
                using var resizedIcon = icon.Resize(iconReferenceSize.Scale(this.GetScale()));
                return resizedIcon.ExtractBitmap(0)!;
            }
        }

        private void ReleaseIcons()
        {
            errorIcon?.Dispose();
            warningIcon?.Dispose();
            infoIcon?.Dispose();
            errorIcon = null;
            warningIcon = null;
            infoIcon = null;
        }

        private void DrawValidationIcon(DataGridViewCellPaintingEventArgs e)
        {
            Rectangle bounds = e.CellBounds;
            bounds.Height -= 1;
            bounds.Width -= 1;

            Bitmap? icon = GetCellIcon(e);
            if (icon == null)
                return;

            Size size = icon.Size;
            Rectangle iconRect = new Rectangle(bounds.Left + (bounds.Width - size.Width - 4),
                    bounds.Top + ((bounds.Height >> 1) - (size.Height >> 1)),
                    size.Width, size.Height);

            Rectangle iconBounds = bounds.IntersectSafe(iconRect);
            if (iconBounds.IsEmpty)
                return;

            bool clip = iconRect != iconBounds;
            if (clip)
                e.Graphics!.IntersectClip(iconBounds);
            e.Graphics!.DrawImage(icon, iconRect);
            if (clip)
                e.Graphics.ResetClip();
        }

        private Bitmap? GetCellIcon(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return null;

            // falling back to default error logic
            if (Rows[e.RowIndex].DataBoundItem is not IValidatingObject validatingObject)
                return String.IsNullOrEmpty(e.ErrorText) ? null : ErrorIcon;

            if (!OSHelper.IsWindows)
                EnsureValidationText(e, validatingObject);

            ValidationResultsCollection validationResults = validatingObject.ValidationResults;
            if (validationResults.Count == 0)
                return null;

            // row header
            if (e.ColumnIndex < 0)
            {
                return validationResults.HasErrors ? ErrorIcon
                    : validationResults.HasWarnings ? WarningIcon
                    : validationResults.HasInfos ? InfoIcon
                    : null;
            }

            // cell
            ValidationResultsCollection propertyValidation = validationResults[Columns[e.ColumnIndex].DataPropertyName];
            return propertyValidation.HasErrors ? ErrorIcon
                : propertyValidation.HasWarnings ? WarningIcon
                : propertyValidation.HasInfos ? InfoIcon
                : null;
        }

        private void EnsureValidationText(DataGridViewCellPaintingEventArgs e, IValidatingObject validatingObject)
        {
            DataGridViewRow row = Rows[e.RowIndex];
            DataGridViewCell cell = e.ColumnIndex < 0 ? row.HeaderCell : row.Cells[e.ColumnIndex];
            ValidationResultsCollection validationResults = validatingObject.ValidationResults;

            cell.ErrorText = e.ColumnIndex < 0
                ? validationResults.Message
                : validationResults.TryGetFirstWithHighestSeverity(Columns[e.ColumnIndex].DataPropertyName)?.Message;
        }

        #endregion

        #endregion
    }
}
