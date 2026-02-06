#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ControlExtensions.cs
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using KGySoft.Collections;
using KGySoft.Reflection;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Reflection;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms
#pragma warning disable CS8604 // Possible null reference argument - inconsistent nullability annotations on different platforms
#endif

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Extension methods for <see cref="Control"/> class.
    /// </summary>
    public static class ControlExtensions
    {
        #region Constants

        /// <summary>
        /// Represents a special value indicating that no item is selected in a selection control.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const int NotSelectedValue = 0;

        /// <summary>
        /// Represents a special value indicating that all items are selected in a selection control.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const int AllSelectedValue = NotSelectedValue - 1;

        /// <summary>
        /// Represents a special value indicating that no items are selected in a selection control.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const int NoneSelectedValue = NotSelectedValue - 2;

        /// <summary>
        /// Represents a special value indicating that an undefined custom value is selected in a selection control.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const int UndefinedValue = Int32.MaxValue;

        /// <summary>
        /// Gets the text representing the <see cref="NotSelectedValue"/>.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const string NotSelectedText = " (Not selected)";

        /// <summary>
        /// Gets the text representing the <see cref="AllSelectedValue"/>.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const string AllSelectedText = " (All)";

        /// <summary>
        /// Gets the text representing the <see cref="NoneSelectedValue"/>.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const string NoneSelectedText = " (None)";

        /// <summary>
        /// Gets the text representing the <see cref="UndefinedValue"/>.
        /// </summary>
        [Obsolete("SelectionPlusItems-related functionality")]public const string UndefinedText = " (Undefined)";

        #endregion

        #region Fields

        private static IThreadSafeCacheAccessor<Type, FieldAccessor?>? toolTipCache;

        #endregion

        #region Properties

        private static IThreadSafeCacheAccessor<Type, FieldAccessor?> ToolTipCache
        {
            get
            {
                if (toolTipCache == null)
                {
                    var options = new LockFreeCacheOptions()
                    {
                        InitialCapacity = 4,
                        ThresholdCapacity = 32,
                        MergeInterval = TimeSpan.FromMilliseconds(100)
                    };

                    var cache = ThreadSafeCacheFactory.Create<Type, FieldAccessor?>(GetToolTipField, options);
                    Interlocked.CompareExchange(ref toolTipCache, cache, null);
                }

                return toolTipCache;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Stores values of all controls derived from <see cref="ucBase"/> to mark modified controls.
        /// </summary>
        [Obsolete("ucBase-related functionality.")]
        public static void StoreValues(this Control control)
        {
            if (control is ucBase @base)
                @base.SaveValue();
            if (control.HasChildren)
                foreach (Control c in control.Controls)
                    StoreValues(c);
        }

        /// <summary>
        /// Clears stored values of all controls derived from <see cref="ucBase"/> to unmark modified controls.
        /// </summary>
        [Obsolete("ucBase-related functionality.")]
        public static void ClearStoredValues(this Control control)
        {
            if (control is ucBase @base)
                @base.ClearSavedValue();
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
            if (control is IReadOnlyCapable readOnlyCapable)
            {
                readOnlyCapable.ReadOnly = readOnly;
                return;
            }

            if (control is TextBoxBase textBoxBase)
            {
                textBoxBase.ReadOnly = readOnly;
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
            TextFormatFlags flags = TextFormatFlags.TextBoxControl;
            //| TextFormatFlags.PreserveGraphicsTranslateTransform; // To prevent erasing text when rendered with TextRenderer

            bool showEllipsis = false;
            bool useMnemonic = false;
            bool wordBreak = false;
            bool singleLine = false;
            ContentAlignment? contentAlignment = null;
            HorizontalAlignment? horizontalAlignment = null;
            bool isRtl = c.RightToLeft == RightToLeft.Yes;

            switch (c)
            {
                case TextBoxBase textBox:
                    flags |= TextFormatFlags.ExpandTabs;
                    singleLine = !textBox.Multiline;
                    wordBreak = !singleLine && textBox.WordWrap;
                    if (singleLine)
                        flags |= TextFormatFlags.NoPadding;
                    horizontalAlignment = textBox switch
                    {
                        TextBox tb => tb.TextAlign,
                        MaskedTextBox mtb => mtb.TextAlign,
                        _ => horizontalAlignment
                    };

                    break;
                case ButtonBase button:
                    contentAlignment = button.TextAlign;
                    wordBreak = true;
                    showEllipsis = button.AutoEllipsis;
                    useMnemonic = button.UseMnemonic;
                    break;
                case Label label:
                    contentAlignment = label.TextAlign;
                    wordBreak = true;
                    showEllipsis = label.AutoEllipsis;
                    useMnemonic = label.UseMnemonic;
                    break;
                case ComboBox:
                    singleLine = true;
                    flags |= TextFormatFlags.NoPadding;
                    break;
                case DateTimePicker dtp:
                    flags |= TextFormatFlags.NoPadding;
                    contentAlignment = ContentAlignment.MiddleLeft;
                    singleLine = true;
                    isRtl &= dtp.RightToLeftLayout;
                    break;
            }

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
                flags |= isRtl ? TextFormatFlags.Right : TextFormatFlags.Left;

            if (wordBreak)
                flags |= TextFormatFlags.WordBreak;
            else if (singleLine)
                flags |= TextFormatFlags.SingleLine;

            if (showEllipsis)
                flags |= TextFormatFlags.WordEllipsis | TextFormatFlags.EndEllipsis;
            if (isRtl)
                flags |= TextFormatFlags.RightToLeft;
            if (!useMnemonic)
                flags |= TextFormatFlags.NoPrefix;
            if (c.TopLevelControl?.IsHandleCreated != true || c is ISupportButtonAdapter adapter && !adapter.ShowKeyboardCues || !c.ShowKeyboardCues())
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
                throw new ArgumentNullException(nameof(control), PublicResources.ArgumentNull);
            Accessors.SetDoubleBuffered(control, useDoubleBuffering);
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
                throw new ArgumentNullException(nameof(control), PublicResources.ArgumentNull);
            Accessors.SetStyle(control, flags, value);
        }

        #endregion

        #region Internal Methods

        internal static void PaintTransparentBackground(this Control c, PaintEventArgs e)
        {
            Control? parent = c.Parent;
            if (parent == null)
                return;

            Rectangle rectangle = c.ClientRectangle;
            if (VisualStyleHelper.RenderWithVisualStyles)
                ButtonRenderer.DrawParentBackground(e.Graphics, rectangle, c);
            else
            {
                GraphicsContainer state = e.Graphics.BeginContainer();
                try
                {
                    e.Graphics.TranslateTransform(-c.Left, -c.Top);
                    rectangle.Offset(c.Left, c.Top);
                    PaintEventArgs pe = new PaintEventArgs(e.Graphics, rectangle);
                    parent.PaintBackground(pe, rectangle, parent.BackColor);
                    parent.OnPaint(pe);
                }
                finally
                {
                    e.Graphics.EndContainer(state);
                }
            }
        }

        /// <summary>
        /// Tries to find the first <see cref="ToolTip"/> component associated with the control or its parent controls.
        /// Ignores private self ToolTips of custom controls (e.g. ToolStrip, DataGridView), but returns the first ToolTip of a parent Form or UserControl that is expected to use for child controls anyway.
        /// </summary>
        internal static ToolTip? TryGetToolTip(this Control ctrl)
        {
            for (Control? c = ctrl; c != null; c = c.Parent)
            {
                // checking forms and user controls only; otherwise, using the control only to traverse the hierarchy
                if (c is not (Form or UserControl))
                    continue;

                FieldAccessor? toolTipField = ToolTipCache[c.GetType()];
                if (toolTipField != null)
                    return toolTipField.Get(c) as ToolTip;
            }

            return null;
        }

        #endregion

        #region Private Methods

        private static FieldAccessor? GetToolTipField(Type type)
        {
            Debug.Assert(typeof(UserControl).IsAssignableFrom(type) || typeof(Form).IsAssignableFrom(type));

            // looking for the first toolTip field in the type hierarchy not deeper than the Form/UserControl type, i.e. custom fields of derived forms and user controls only
            for (Type t = type; t != typeof(Form) && t != typeof(UserControl); t = t.BaseType!)
            {
                FieldInfo? fi = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).FirstOrDefault(f => typeof(ToolTip).IsAssignableFrom(f.FieldType));
                if (fi != null)
                    return FieldAccessor.GetAccessor(fi);
            }

            return null;
        }

        #endregion

        #endregion
    }
}
