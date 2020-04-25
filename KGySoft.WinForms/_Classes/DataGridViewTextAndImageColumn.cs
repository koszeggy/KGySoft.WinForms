using System;
using System.Drawing;
using System.Windows.Forms;

namespace KGySoft.WinForms
{
    [Obsolete("Will be deleted")]
    public class DataGridViewTextAndImageColumn : DataGridViewTextBoxColumn
    {
        private Image imageValue;
        private Size imageSize;

        public DataGridViewTextAndImageColumn()
        {
            this.CellTemplate = new DataGridViewTextAndImageCell();
        }

        public override object Clone()
        {
            DataGridViewTextAndImageColumn c = base.Clone() as DataGridViewTextAndImageColumn;
            c.imageValue = this.imageValue;
            c.imageSize = this.imageSize;
            return c;
        }

        public Image Image
        {
            get { return this.imageValue; }
            set
            {
                if (this.Image != value)
                {
                    this.imageValue = value;
                    this.imageSize = value.Size;

                    if (this.InheritedStyle != null)
                    {
                        Padding inheritedPadding = this.InheritedStyle.Padding;
                        this.DefaultCellStyle.Padding = new Padding(imageSize.Width,
                             inheritedPadding.Top, inheritedPadding.Right, inheritedPadding.Bottom);
                    }
                }
            }
        }
        private DataGridViewTextAndImageCell DataGridViewTextAndImageCellTemplate
        {
            get { return this.CellTemplate as DataGridViewTextAndImageCell; }
        }
        internal Size ImageSize
        {
            get { return imageSize; }
        }
    }

    public class DataGridViewTextAndImageCell : DataGridViewTextBoxCell
    {
        private Image imageValue;
        private Size imageSize;

        public override object Clone()
        {
            DataGridViewTextAndImageCell c = base.Clone() as DataGridViewTextAndImageCell;
            c.imageValue = this.imageValue;
            c.imageSize = this.imageSize;
            return c;
        }

        public Image Image
        {
            get
            {
                if (this.OwningColumn == null || this.OwningDataGridViewTextAndImageColumn == null)
                {

                    return imageValue;
                }
                else if (this.imageValue != null)
                {
                    return this.imageValue;
                }
                else
                {
                    return this.OwningDataGridViewTextAndImageColumn.Image;
                }
            }
            set
            {
                if (this.imageValue != value)
                {
                    this.imageValue = value;
                    if (value != null)
                        this.imageSize = value.Size;
                    else
                        this.imageSize = new Size(1, 1);

                    Padding inheritedPadding = this.InheritedStyle.Padding;
                    this.Style.Padding = new Padding(imageSize.Width,
                        inheritedPadding.Top, inheritedPadding.Right,
                        inheritedPadding.Bottom);
                }
            }
        }
        [System.Diagnostics.DebuggerStepThrough()]
        protected override void Paint(Graphics graphics, Rectangle clipBounds,
            Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState,
            object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts
        )
        {
            // Paint the base content
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
               value, formattedValue, errorText, cellStyle,
               advancedBorderStyle, paintParts);

            if (this.Image != null)
            {
                // Draw the image clipped to the cell.
                System.Drawing.Drawing2D.GraphicsContainer container = graphics.BeginContainer();

                graphics.SetClip(cellBounds);

                int y = (cellBounds.Height - this.imageSize.Height) / 2;
                graphics.DrawImageUnscaled(this.Image, cellBounds.Location.X, cellBounds.Location.Y + y);

                graphics.EndContainer(container);
            }
        }

        private DataGridViewTextAndImageColumn OwningDataGridViewTextAndImageColumn
        {
            get { return this.OwningColumn as DataGridViewTextAndImageColumn; }
        }
    }
}