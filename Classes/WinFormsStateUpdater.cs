using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KGySoft.ComponentModel;
using KGySoft.Reflection;

namespace KGySoft.Controls.Classes
{
    /// <summary>
    /// Provides special handling for ToolTipText: tries to find the associated <see cref="ToolTip"/> component.
    /// </summary>
    internal class WinFormsPropertyCommandStateUpdater : ICommandStateUpdater
    {
        private const string ToolTipTextProperty = "ToolTipText";
        private static readonly WinFormsPropertyCommandStateUpdater instance = new WinFormsPropertyCommandStateUpdater();

        private WinFormsPropertyCommandStateUpdater()
        {
        }

        internal static ICommandStateUpdater Updater => instance;

        public bool TryUpdateState(object commandSource, string stateName, object value)
        {
            if (stateName != ToolTipTextProperty || !(value is string text) || !(commandSource is Control control))
                return false;

            GetToolTip(control).SetToolTip(control, text);
            return true;
        }

        private static ToolTip GetToolTip(Control ctrl)
        {
            for (Control c = ctrl; c != null; c = c.Parent)
            {
                var containerFields = c.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => typeof(IContainer).IsAssignableFrom(f.FieldType));
                foreach (var containerField in containerFields)
                {
                    var container = (IContainer)Reflector.GetField(c, containerField);
                    var toolTip = container?.Components?.OfType<ToolTip>().FirstOrDefault();
                    if (toolTip != null)
                        return toolTip;
                }
            }

            return null;
        }

        void IDisposable.Dispose()
        {
        }
    }
}
