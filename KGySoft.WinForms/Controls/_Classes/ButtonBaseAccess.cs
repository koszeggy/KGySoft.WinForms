using System;
using System.Reflection;
using System.Windows.Forms;
using KGySoft.Reflection;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Acessor of ButtonBase class members
    /// </summary>
    internal static class ButtonBaseAccess
    {
        private const string fieldNameEventPaint =
#if NETFRAMEWORK
            "EventPaint";
#else
            "s_paintEvent";
#endif

        private static PropertyAccessor propertyShowToolTip;
        private static MethodAccessor methodAnimate;
        private static FieldAccessor fieldEventPaint;

        /// <summary>
        /// Sets ButtonBase.ShowToolTip
        /// </summary>
        internal static void ShowToolTip(this ButtonBase instance, bool value)
        {
            if (propertyShowToolTip == null)
                propertyShowToolTip = PropertyAccessor.GetAccessor(typeof(ButtonBase).GetProperty("ShowToolTip", BindingFlags.Instance | BindingFlags.NonPublic));

            propertyShowToolTip.Set(instance, value);
        }

        /// <summary>
        /// Executes ButtonBase.Animate
        /// </summary>
        internal static void Animate(this ButtonBase instance)
        {
            if (methodAnimate == null)
                methodAnimate = MethodAccessor.GetAccessor(typeof(ButtonBase).GetMethod("Animate", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null));

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
                    fieldEventPaint = FieldAccessor.GetAccessor(typeof(Control).GetField(fieldNameEventPaint, BindingFlags.Static | BindingFlags.NonPublic));

                return fieldEventPaint.Get(null);
            }
        }
    }
}
