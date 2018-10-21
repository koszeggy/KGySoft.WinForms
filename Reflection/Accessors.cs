using System.Reflection;
using System.Windows.Forms;
using KGySoft.Reflection;
using MethodInvoker = KGySoft.Reflection.MethodInvoker;

namespace KGySoft.Controls.Reflection
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        #region Field Accessors

        private static FieldAccessor fieldErrorProvider_currentChanged;
        private static FieldAccessor fieldErrorProvider_errorManager;

        #endregion

        #region Method Accessors

        private static MethodInvoker methodErrorProvider_UnwireEvents;

        #endregion

        #endregion

        #region Methods

        #region Field Accessors

        internal static FieldAccessor ErrorProvider_currentChanged => fieldErrorProvider_currentChanged 
            ?? (fieldErrorProvider_currentChanged = FieldAccessor.GetFieldAccessor(typeof(ErrorProvider).GetField("currentChanged", BindingFlags.Instance | BindingFlags.NonPublic)));

        internal static FieldAccessor ErrorProvider_errorManager => fieldErrorProvider_errorManager
            ?? (fieldErrorProvider_errorManager = FieldAccessor.GetFieldAccessor(typeof(ErrorProvider).GetField("errorManager", BindingFlags.Instance | BindingFlags.NonPublic)));

        #endregion

        #region Method Accessors

        internal static void UnwireEvents(this ErrorProvider errorProvider, BindingManagerBase listManager)
        {
            if (methodErrorProvider_UnwireEvents == null)
                methodErrorProvider_UnwireEvents = MethodInvoker.GetMethodInvoker(typeof(ErrorProvider).GetMethod("UnwireEvents", BindingFlags.Instance | BindingFlags.NonPublic));
            methodErrorProvider_UnwireEvents.Invoke(errorProvider, listManager);
        }

        #endregion

        #endregion
    }
}
