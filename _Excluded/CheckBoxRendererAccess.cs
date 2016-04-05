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
    internal static class CheckBoxRendererAccess
    {
        private static MethodInvoker methodConvertFromButtonState;

        internal static CheckBoxState ConvertFromButtonState(ButtonState state, bool isMixed, bool isHot)
        {
            if (methodConvertFromButtonState == null)
                methodConvertFromButtonState = MethodInvoker.GetMethodInvoker(typeof(CheckBoxRenderer).GetMethod("ConvertFromButtonState", BindingFlags.Static | BindingFlags.NonPublic));

            return (CheckBoxState)methodConvertFromButtonState.Invoke(null, state, isMixed, isHot);
        }
    }
}
