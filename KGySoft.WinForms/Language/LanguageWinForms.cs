using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KGySoft.Libraries.Language;

namespace KGySoft.WinForms
{

    /// <summary>
    /// A class that extends <see cref="Language"/> class with Winfows Forms routines
    /// </summary>
    [Obsolete]
    public static class LanguageWinForms
    {
        /// <summary>
        /// Translates the control along with its children controls.
        /// </summary>
        public static void TranslateControls(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            bool finished;
            TranslateControl(control, out finished);
            if (finished)
                return;

            if (control.HasChildren)
                foreach (Control c in control.Controls)
                    TranslateControls(c);
        }

        /// <summary>
        /// Translate the control without its children. You may use the <see cref="LocalizableAttribute"/> to
        /// make a derived control type untranslatable. To adjust translation of control instances
        /// you may use the <see cref="Language.MarkLocalizable"/> method.
        /// A custom translatable control may implement the <see cref="ICustomTranslated"/> interface.
        /// <remarks>
        /// Cell/row items and their tooltips are not translated, only headers. Use custom translation to translate cell/row values.
        /// </remarks>
        /// </summary>
        /// <param name="control">The control to translate.</param>
        /// <param name="translationFinished">When returns true, no further translation should be performed on child elements.</param>
        /// <returns>Returns true if translation is not disabled for the control, otherwise, false.</returns>
        public static bool TranslateControl(Control control, out bool translationFinished)
        {
            if (!Language.IsObjectLocalizable(control))
            {
                translationFinished = true;
                return false;
            }

            translationFinished = false;

            if (control is ICustomTranslated)
            {
                (control as ICustomTranslated).TranslateControl(out translationFinished);
            }
            else if (control is Form
                || control is Button
                || control is GroupBox
                || control is Label
                || control is CheckBox
                || control is RadioButton
                || control is TabPage
                )
            {
                if (Language.IsPropertyLocalizable(control, "Text"))
                    control.Text = Language.Translate(control.Text);
            }
            else if (control is ListView)
            {
                foreach (ColumnHeader header in ((ListView)control).Columns)
                {
                    if (Language.IsObjectLocalizable(header))
                        header.Text = Language.Translate(header.Text);
                }
                foreach (ListViewGroup group in ((ListView)control).Groups)
                {
                    if (Language.IsObjectLocalizable(group))
                        group.Header = Language.Translate(group.Header);
                }
            }
            else if (control is MenuStrip)
            {
                foreach (ToolStripItem item in ((MenuStrip)control).Items)
                {
                    if (item is ToolStripMenuItem)
                        TranslateMenuItems((ToolStripMenuItem)item);
                    else
                        item.ToolTipText = Language.Translate(item.ToolTipText);
                }
            }
            else if (control is StatusStrip)
            {
                foreach (ToolStripItem item in (control as StatusStrip).Items)
                {
                    if (!Language.IsObjectLocalizable(item))
                        continue;
                    ToolStripStatusLabel statusLabel = item as ToolStripStatusLabel;
                    if (statusLabel != null)
                    {
                        if (Language.IsPropertyLocalizable(statusLabel, "Text"))
                            statusLabel.Text = Language.Translate(statusLabel.Text);
                        statusLabel.ToolTipText = Language.Translate(statusLabel.ToolTipText);
                    }
                }
            }
            else if (control is ToolStrip)
            {
                foreach (ToolStripItem item in (control as ToolStrip).Items)
                {
                    if (!Language.IsObjectLocalizable(item))
                        continue;

                    if (Language.IsPropertyLocalizable(item, "Text"))
                        item.Text = Language.Translate(item.Text);
                    if (item.Text != item.ToolTipText)
                        item.ToolTipText = Language.Translate(item.ToolTipText);

                    if (item is ToolStripDropDownItem)
                    {
                        foreach (ToolStripItem i in (item as ToolStripDropDownItem).DropDownItems)
                        {
                            if (i is ToolStripMenuItem)
                                TranslateMenuItems((ToolStripMenuItem)i);
                            else
                                i.ToolTipText = Language.Translate(i.ToolTipText);
                        }
                    }
                }
            }
            else if (control is DataGridView)
            {
                foreach (DataGridViewColumn column in ((DataGridView)control).Columns)
                {
                    if (!Language.IsObjectLocalizable(column))
                        continue;
                    column.HeaderText = Language.Translate(column.HeaderText);
                    column.ToolTipText = Language.Translate(column.ToolTipText);
                }
            }

            return true;
        }

        private static void TranslateMenuItems(ToolStripMenuItem menuItems)
        {
            if (Language.IsObjectLocalizable(menuItems))
            {
                if (Language.IsPropertyLocalizable(menuItems, "Text"))
                    menuItems.Text = Language.Translate(menuItems.Text);
                menuItems.ToolTipText = Language.Translate(menuItems.ToolTipText);
            }
            if (menuItems.DropDownItems.Count > 0)
            {
                foreach (ToolStripItem item in menuItems.DropDownItems)
                {
                    if (item is ToolStripMenuItem)
                        TranslateMenuItems((ToolStripMenuItem)item);
                    else
                        item.ToolTipText = Language.Translate(menuItems.ToolTipText);
                }
            }
        }
    }
}
