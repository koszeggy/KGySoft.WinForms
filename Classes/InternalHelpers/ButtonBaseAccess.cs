using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

using KGySoft.Libraries.Reflection;

using MethodInvoker = KGySoft.Libraries.Reflection.MethodInvoker;

namespace KGySoft.Controls
{
    /// <summary>
    /// Acessor of ButtonBase class members
    /// </summary>
    internal static class ButtonBaseAccess
    {
        private static PropertyAccessor propertyShowToolTip;
        private static MethodInvoker methodAnimate;
        private static FieldAccessor fieldEventPaint;

        /// <summary>
        /// Sets ButtonBase.ShowToolTip
        /// </summary>
        internal static void ShowToolTip(this ButtonBase instance, bool value)
        {
            if (propertyShowToolTip == null)
                propertyShowToolTip = PropertyAccessor.GetPropertyAccessor(typeof(ButtonBase).GetProperty("ShowToolTip", BindingFlags.Instance | BindingFlags.NonPublic));

            propertyShowToolTip.Set(instance, value);
        }

        /// <summary>
        /// Executes ButtonBase.Animate
        /// </summary>
        internal static void Animate(this ButtonBase instance)
        {
            if (methodAnimate == null)
                methodAnimate = MethodInvoker.GetMethodInvoker(typeof(ButtonBase).GetMethod("Animate", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null));

            methodAnimate.Invoke(instance);
        }

        /// <summary>
        /// Gets Control.EventPaint
        /// </summary>
        internal static object EventPaint
        {
            get
            {
                if (fieldEventPaint == null)
                    fieldEventPaint = FieldAccessor.GetFieldAccessor(typeof(Control).GetField("EventPaint", BindingFlags.Static | BindingFlags.NonPublic));

                return fieldEventPaint.Get(null);
            }
        }
    }
}
