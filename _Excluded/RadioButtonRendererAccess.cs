using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using MethodInvoker = KGySoft.Libraries.Reflection.MethodInvoker;

namespace KGySoft.Controls
{
    internal static class RadioButtonRendererAccess
    {
        private static MethodInvoker methodConvertFromButtonState;

        internal static RadioButtonState ConvertFromButtonState(ButtonState state, bool isHot)
        {
            if (methodConvertFromButtonState == null)
                methodConvertFromButtonState = MethodInvoker.GetMethodInvoker(typeof(RadioButtonRenderer).GetMethod("ConvertFromButtonState", BindingFlags.Static | BindingFlags.NonPublic));

            return (RadioButtonState)methodConvertFromButtonState.Invoke(null, state, isHot);
        }
    }
}
