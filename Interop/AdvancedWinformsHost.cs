using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Forms;

using KGySoft.Libraries;
using KGySoft.Controls.WinApi;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;

namespace KGySoft.Controls.Interop
{
    // Note: When .NET version >= 4.0, System.Xaml reference has to be added due to this class
    /// <summary>
    /// Fixes some issues in the original <see cref="WindowsFormsHost"/> control when used from another AppDomain:
    /// <list type="bullet">
    /// <item><term>Working cursor keys</term><description>In original <see cref="WindowsFormsHost"/> cursor keys are not working correctly inside of the hosted control
    /// (right cursor jumps out from the focused control, for example.). This control tracks the focused inner Windows Forms control and sends the cursor keys to it.</description></item>
    /// <item><term>Working TAB key</term><description>TAB jumps to the next focusable control. Incompatibility: Shift+TAB is not supported and <see cref="TextBoxBase.AcceptsTab"/> is not detected.</description></item>
    /// <item><term>Memory leak fix</term><description>When used in another AppDomain, releases references that is not performed by the .NET framework.</description></item>
    /// </list>
    /// </summary>
    public class AdvancedWinformsHost: WindowsFormsHost, IKeyboardInputSink
    {
        #region Fields

        private Control activeControl;
        private INativeHandleContract handleContract;

        #endregion

        #region Constructor, Dispose

        /// <summary>
        /// Disposes the base control and performs additional disposal steps to avoid memory leaks:
        /// nullifies <see cref="WindowsFormsHost.Child"/>, unregisters the <see cref="HwndHost.KeyboardInputSite"/>,
        /// revokes lifetime token from the handle contract while disconnecting it if <see cref="GetHandleContract"/> was called,
        /// and tries to unhook all of the WPF-related references that cause memory leak (not everything can be done without causeng windows errors).
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            activeControl = null;
            base.Dispose(disposing);
            Action nullify = () => Child = null;
            if (Dispatcher == null || Dispatcher.CheckAccess())
                nullify.Invoke();
            else
                Dispatcher.Invoke(nullify);

            IKeyboardInputSite kis = ((IKeyboardInputSink)this).KeyboardInputSite;
            if (kis != null)
            {
                kis.Unregister();
            }

            if (handleContract != null)
            {
                List<int> tokens = (List<int>)Reflector.GetField(handleContract, "m_lifetimeTokens");
                if (tokens != null)
                {
                    foreach (int i in tokens.ToArray())
                    {
                        handleContract.RevokeLifetimeToken(i);
                    }
                }
                RemotingServices.Disconnect((MarshalByRefObject)handleContract);
                handleContract = null;
            }

            if (Dispatcher != null)
            {
                // Removing Dispatcher._reserved0 (as MediaContext) from MediaSystem._mediaContexts
                object fieldDispatcher__reserved0_MediaContext = Reflector.GetField(Dispatcher, "_reserved0");
                Type typeMediaSystem = Reflector.ResolveType("System.Windows.Media.MediaSystem");
                Reflector.RunMethod(typeMediaSystem, "Shutdown", fieldDispatcher__reserved0_MediaContext);

                //// nullifying Dispatcher._reserved0
                //Reflector.SetInstanceFieldByName(Dispatcher, "_reserved0", null);

                //// nullifying Dispatcher._reservedInputManager
                //Reflector.SetInstanceFieldByName(Dispatcher, "_reservedInputManager", null);

                ////// nullifying Dispatcher._queue
                ////Reflector.SetInstanceFieldByName(Dispatcher, "_queue", null);

                //// nullifying Dispatcher.ShutdownFinished
                //Reflector.SetInstanceFieldByName(Dispatcher, "ShutdownFinished", null);
            }
        }

        #endregion

        #region IKeyboardInputSink Members

        bool IKeyboardInputSink.TranslateAccelerator(ref MSG msg, ModifierKeys modifiers)
        {
            if (msg.message == Constants.WM_KEYDOWN)
            {
                Keys key = (Keys)msg.wParam;

                if (!key.In(Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Tab))
                    return false;

                if (activeControl == null || !activeControl.Focused)
                {
                    activeControl = null;
                    FindActiveControl(Child);
                }
                if (activeControl == null)
                    return false;

                if (key == Keys.Tab)
                {
                    if (!Child.Enabled)
                        return false;
                    Control newActiveControl = activeControl.GetNextControl(activeControl, true);
                    if (newActiveControl != null)
                    {
                        activeControl = newActiveControl;
                        return true;
                    }

                    HashSet<Control> visited = new HashSet<Control>();
                    newActiveControl = FindNextSelectableControl(activeControl);
                    while (!visited.Contains(newActiveControl))
                    {
                        bool focused = newActiveControl.Focus();
                        if (focused)
                        {
                            activeControl = newActiveControl;
                            return true;
                        }
                        visited.Add(newActiveControl);
                        newActiveControl = FindNextSelectableControl(newActiveControl);
                    }
                    return false;
                }

                User32.SendMessage(activeControl.Handle, msg.message, msg.wParam, msg.lParam);
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        private void FindActiveControl(Control control)
        {
            if (control == null)
                return;
            if (control.Focused)
            {
                activeControl = control;
                return;
            }
            if (!control.HasChildren)
                return;

            foreach (Control c in control.Controls)
            {
                FindActiveControl(c);
                if (activeControl != null)
                    return;
            }
        }

        /// <summary>
        /// Finds the next selectable control after <paramref name="start"/>.
        /// </summary>
        /// <param name="start">Start control, can be invalid for selection.</param>
        private Control FindNextSelectableControl(Control start)
        {
            Control curr = null;
            List<Control> visited = new List<Control>();

            do
            {
                if (curr != null)
                    visited.Add(curr);
                else
                    curr = start;

                // 1. Find next control (next curr)
                // a.) curr is Child -> this is the topmost control: the next one will be its first child or itself
                if (curr == Child)
                {
                    curr = curr.HasChildren ? GetFirstTabChild(curr.Controls) : curr;
                }
                // b.) curr is a valid parent -> the next one will be its first child
                else if (!curr.IsDisposed && curr.Enabled && curr.Visible && curr.HasChildren)
                {
                    curr = GetFirstTabChild(curr.Controls);
                }
                // c.) find next control in Tab Order -> Minimum Search
                else
                {
                    Int64 min = Int64.MaxValue;
                    Control minChild = null;
                    while (min == Int64.MaxValue && curr != Child)
                    {
                        foreach (Control c in curr.Parent.Controls)
                        {
                            if (c.TabIndex < min && c.TabIndex > curr.TabIndex)
                            {
                                min = c.TabIndex;
                                minChild = c;
                            }
                        }
                        if (min == Int64.MaxValue) // curr was the last control -> move up one level
                            curr = curr.Parent;
                        else
                            curr = minChild;
                    }
                }

                // 2. check whether curr is ok
                if (!curr.IsDisposed && curr.Enabled && curr.Visible
                    && curr.CanFocus && curr.TabStop)
                {
                    return curr;
                }
            } while (!visited.Contains(curr));

            // at this point we have visited every controls and not found anything -> return with self Child
            return Child;
        }

        private static Control GetFirstTabChild(Control.ControlCollection children)
        {
            int minTab = Int32.MaxValue;
            Control result = null;
            foreach (Control child in children)
            {
                if (child.TabIndex < minTab)
                {
                    minTab = child.TabIndex;
                    result = child;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets a <see cref="INativeHandleContract"/> instance that can be used to embed this control in another <see cref="AppDomain"/>.
        /// </summary>
        public INativeHandleContract GetHandleContract()
        {
            if (handleContract == null)
            {
                handleContract = FrameworkElementAdapters.ViewToContractAdapter(this);
            }
            return handleContract;
        }

        #endregion

    }
}
