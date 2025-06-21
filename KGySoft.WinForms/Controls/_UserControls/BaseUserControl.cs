#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseUserControl.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.ComponentModel;
using System.Windows.Forms;

using KGySoft.ComponentModel;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A base class for user controls that provides some additional functionality.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseUserControl"/> class provides the following additional features:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the user control is disposed. To do that for the events of derived controls as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item><see cref="CommandBindings"/> property. See the <a href="https://kgysoft.net/corelibraries#command-binding" target="_blank">online documentation</a> for details.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// </list>
    /// </remarks>
    public class BaseUserControl : UserControl
    {
        #region Fields

        private readonly CommandBindingsCollection commandBindings = new WinFormsCommandBindingsCollection();
        private readonly InvokeMarshaller invoker;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the command bindings of this form. The <see cref="O:KGySoft.ComponentModel.CommandBindingsCollection.Add">Add</see> methods also add
        /// the <see cref="PropertyCommandStateUpdater"/> to the created bindings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets whether the user control is in design mode. Unlike the <see cref="Component.DesignMode"/> property,
        /// this property works even during initialization.
        /// </summary>
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserControl"/> class.
        /// </summary>
        protected BaseUserControl()
        {
            invoker = new InvokeMarshaller(this);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                commandBindings.Dispose();
                Events.Dispose();
            }
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> on the thread that the control was created on.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// <para>This method is similar as using <see cref="Control.InvokeRequired"/> and <see cref="Control.Invoke(Delegate)"/> together,
        /// but it works even when the handle is not created yet, in which case <see cref="Control.InvokeRequired"/> returns <see langword="false"/>.</para>
        /// <para>The callback is invoked only if <see cref="Control.Disposing"/> and <see cref="Control.IsDisposed"/> properties return <see langword="false"/>.</para>
        /// </remarks>
        protected void InvokeOnUIThread(Action callback) => invoker.Invoke(callback);

        #endregion
    }
}