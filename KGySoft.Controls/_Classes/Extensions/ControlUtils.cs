#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using KGySoft.Libraries;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Extension methods for <see cref="Control"/> class.
    /// </summary>
    public static class ControlTools
    {
        #region Constants

        public const int NotSelectedValue = 0;
        public const int AllSelectedValue = NotSelectedValue - 1;
        public const int NoneSelectedValue = NotSelectedValue - 2;
        public const int UndefinedValue = Int32.MaxValue;
        public const string NotSelectedText = " (Not selected)";
        public const string AllSelectedText = " (All)";
        public const string NoneSelectedText = " (None)";
        public const string UndefinedText = " (Undefined)";
        private static MethodAccessor methodPaintBackground;
        private static MethodAccessor methodPaint;

        #endregion

        #region Fields

        private static PropertyAccessor propertyControl_ShowKeyboardCues;
        private static PropertyAccessor propertyControl_DoubleBuffered;
        private static MethodAccessor methodControl_SetStyle;

        #endregion

        #region Properties

        private static PropertyAccessor PropertyControl_ShowKeyboardCues
        {
            get
            {
                if (propertyControl_ShowKeyboardCues != null)
                    return propertyControl_ShowKeyboardCues;

                propertyControl_ShowKeyboardCues = PropertyAccessor.GetAccessor(typeof(Control).GetProperty("ShowKeyboardCues", BindingFlags.Instance | BindingFlags.NonPublic));
                return propertyControl_ShowKeyboardCues;
            }
        }

        private static PropertyAccessor PropertyControl_DoubleBuffered
        {
            get
            {
                if (propertyControl_DoubleBuffered != null)
                    return propertyControl_DoubleBuffered;

                propertyControl_DoubleBuffered = PropertyAccessor.GetAccessor(typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic));
                return propertyControl_DoubleBuffered;
            }
        }

        private static MethodAccessor MethodControl_SetStyle
        {
            get
            {
                if (methodControl_SetStyle != null)
                    return methodControl_SetStyle;

                methodControl_SetStyle = MethodAccessor.GetAccessor(typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic));
                return methodControl_SetStyle;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Stores values of all controls derived from <see cref="ucBase"/> to mark modified controls.
        /// </summary>
        public static void StoreValues(this Control control)
        {
            if (control is ucBase)
                (control as ucBase).SaveValue();

            if (control.HasChildren)
                foreach (Control c in control.Controls)
                    StoreValues(c);
        }

        /// <summary>
        /// Clears stored values of all controls derived from <see cref="ucBase"/> to unmark modified controls.
        /// </summary>
        public static void ClearStoredValues(this Control control)
        {
            if (control is ucBase)
                (control as ucBase).ClearSavedValue();

            if (control.HasChildren)
                foreach (Control c in control.Controls)
                    ClearStoredValues(c);
        }

        /// <summary>
        /// Sets enabled state of the passed <paramref name="control"/> by
        /// setting its <see cref="Control.Enabled"/> property along with its non-container children recursively.
        /// By this way pages of a <see cref="TabControl"/> will remain selectable, a <see cref="SplitContainer"/> remains resizable, etc.
        /// </summary>
        /// <param name="control">The root control.</param>
        /// <param name="enabled">The enabled state to set.</param>
        public static void SetControlEnabled(this Control control, bool enabled)
        {
            if (control is Panel || control is GroupBox ||
                //containerControl is FlowLayoutPanel || // (FlowLayoutPanel is Panel)
                control is TabControl || //containerControl is TabPage (TabPage is Panel)
                control is UserControl || control is Form ||
                control is SplitContainer) // (ucCaptionedBase is UserControl)
            {
                foreach (Control c in control.Controls)
                    SetControlEnabled(c, enabled);
            }
            else
                control.Enabled = enabled;
        }

        /// <summary>
        /// Recursively sets read-only state for the children of the passed <paramref name="control"/>.
        /// Affects <see cref="TextBoxBase"/>, <see cref="AdvancedComboBox"/> and <see cref="ucBase"/> instances.
        /// </summary>
        /// <param name="control">The root control.</param>
        /// <param name="readOnly">The read-only state to set.</param>
        public static void SetControlReadonly(this Control control, bool readOnly)
        {
            if (control is IReadOnlyCapable)
            {
                ((IReadOnlyCapable)control).ReadOnly = readOnly;
                return;
            }

            if (control is TextBoxBase)
            {
                ((TextBoxBase)control).ReadOnly = readOnly;
                return;
            }

            if (control.HasChildren)
                foreach (Control c in control.Controls)
                    SetControlReadonly(c, readOnly);
        }

        /// <summary>
        /// Gets formatting flags for a custom drawn control.
        /// </summary>
        /// <param name="c">The control to be drawn.</param>
        /// <returns>Format flags for drawing the text of the control.</returns>
        public static TextFormatFlags GetFormatFlags(this Control c)
        {
            TextFormatFlags flags = TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.TextBoxControl;
            //| TextFormatFlags.PreserveGraphicsTranslateTransform; // To prevent erasing text when rendered with TextRenderer

            bool showEllipsis = false;
            bool useMnemonic = false;
            bool wordBreak = false;
            ContentAlignment? contentAlignment = null;
            HorizontalAlignment? horizontalAlignment = null;

            TextBoxBase textBox = c as TextBoxBase;
            if (textBox != null)
            {
                flags |= TextFormatFlags.ExpandTabs;
                wordBreak = textBox.Multiline;

                TextBox tb = textBox as TextBox;
                if (tb != null)
                    horizontalAlignment = tb.TextAlign;
                else
                {
                    MaskedTextBox mtb = textBox as MaskedTextBox;
                    if (mtb != null)
                        horizontalAlignment = mtb.TextAlign;
                }
            }
            else
            {
                ButtonBase button = c as ButtonBase;
                if (button != null)
                {
                    contentAlignment = button.TextAlign;
                    wordBreak = true;
                    showEllipsis = button.AutoEllipsis;
                    useMnemonic = button.UseMnemonic;
                }
                else
                {
                    Label label = c as Label;
                    if (label != null)
                    {
                        contentAlignment = label.TextAlign;
                        wordBreak = true;
                        showEllipsis = label.AutoEllipsis;
                        useMnemonic = label.UseMnemonic;
                    }
                }
            }

            bool isRtl = c.RightToLeft == RightToLeft.Yes;

            if (contentAlignment.HasValue)
            {
                if (isRtl)
                    contentAlignment = contentAlignment.Value.RtlTranslateContent(c);

                if (contentAlignment.Value.AnyBottom())
                    flags |= TextFormatFlags.Bottom;
                else if (contentAlignment.Value.AnyMiddle())
                    flags |= TextFormatFlags.VerticalCenter;
                else
                    flags |= TextFormatFlags.Top;

                if (contentAlignment.Value.AnyRight())
                    flags |= TextFormatFlags.Right;
                else if (contentAlignment.Value.AnyCenter())
                    flags |= TextFormatFlags.HorizontalCenter;
                else
                    flags |= TextFormatFlags.Left;
            }
            else if (horizontalAlignment.HasValue)
            {
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        flags |= TextFormatFlags.HorizontalCenter;
                        break;
                    case HorizontalAlignment.Left:
                        flags |= isRtl ? TextFormatFlags.Right : TextFormatFlags.Left;
                        break;
                    case HorizontalAlignment.Right:
                        flags |= isRtl ? TextFormatFlags.Left : TextFormatFlags.Right;
                        break;
                }
            }
            else
            {
                flags |= isRtl ? TextFormatFlags.Right : TextFormatFlags.Left;
            }

            if (wordBreak)
                flags |= TextFormatFlags.WordBreak;
            else
                flags |= TextFormatFlags.SingleLine;

            if (showEllipsis)
                flags |= TextFormatFlags.WordEllipsis | TextFormatFlags.EndEllipsis;
            if (isRtl)
                flags |= TextFormatFlags.RightToLeft;
            if (!useMnemonic)
                flags |= TextFormatFlags.NoPrefix;
            ISupportButtonAdapter adapter = c as ISupportButtonAdapter;
            if (adapter != null && !adapter.ShowKeyboardCues || !(bool)PropertyControl_ShowKeyboardCues.Get(c))
                flags |= TextFormatFlags.HidePrefix;

            return flags;
        }

        /// <summary>
        /// Sets the double buffering state of a control
        /// </summary>
        /// <param name="control">The control to set.</param>
        /// <param name="useDoubleBuffering"><see langword="true"/>, if <paramref name="control"/> should use double buffering; otherwise, <see langword="false"/>.</param>
        public static void SetDoubleBuffered(this Control control, bool useDoubleBuffering)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            PropertyControl_DoubleBuffered.Set(control, useDoubleBuffering);
        }

        /// <summary>
        /// Sets a specified <see cref="ControlStyles"/> flag to either true or false.
        /// </summary>
        /// <param name="control">The control to set.</param>
        /// <param name="flags">The <see cref="ControlStyles"/> bits to set.</param>
        /// <param name="value">true to apply the specified style to the control; otherwise, false.</param>
        public static void SetStyle(this Control control, ControlStyles flags, bool value)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            MethodControl_SetStyle.Invoke(control, flags, value);
        }

        #endregion

        #region Internal Methods

        internal static void PaintBackground(this Control c, PaintEventArgs e, Rectangle rectangle, Color backColor, Point scrollOffset)
        {
            if (methodPaintBackground == null)
                methodPaintBackground = MethodAccessor.GetAccessor(typeof(Control).GetMethod("PaintBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(PaintEventArgs), typeof(Rectangle), typeof(Color), typeof(Point) }, null));

            methodPaintBackground.Invoke(c, e, rectangle, backColor, scrollOffset);
        }

        internal static void PaintTransparentBackground(this Control c, PaintEventArgs e)
        {
            Control parent = c.Parent;
            if (parent == null)
                return;

            Rectangle rectangle = c.ClientRectangle;
            if (Application.RenderWithVisualStyles)
                ButtonRenderer.DrawParentBackground(e.Graphics, rectangle, c);
            else
            {
                GraphicsContainer cstate = e.Graphics.BeginContainer();
                try
                {
                    e.Graphics.TranslateTransform(-c.Left, -c.Top);
                    rectangle.Offset(c.Left, c.Top);
                    PaintEventArgs pe = new PaintEventArgs(e.Graphics, rectangle);
                    PaintBackground(parent, pe, rectangle, parent.BackColor, Point.Empty);
                    InvokePaint(parent, pe);
                }
                finally
                {
                    e.Graphics.EndContainer(cstate);
                }
            }
        }

        #endregion

        #region Private Methods

        private static void InvokePaint(Control c, PaintEventArgs e)
        {
            if (methodPaint == null)
                methodPaint = MethodAccessor.GetAccessor(typeof(Control).GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic));

            methodPaint.Invoke(c, e);
        }

        #endregion

        #endregion
    }
}
