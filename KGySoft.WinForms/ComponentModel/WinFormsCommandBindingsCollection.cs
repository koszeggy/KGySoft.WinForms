#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WinFormsCommandBindingsCollection.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

using KGySoft.Reflection;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.ComponentModel
{
    /// <summary>
    /// A specialized <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_ComponentModel_CommandBindingsCollection.htm">CommandBindingsCollection</a>
    /// that can be used for commands with <see cref="Control"/> sources. By using this collection, the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_ComponentModel_ICommandState.htm">ICommandState</a> properties
    /// (e.g. <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_ComponentModel_ICommandState_Enabled.htm">Enabled</a>,
    /// but also any other dynamically added property) of the added bindings will be synced with the command sources.
    /// It also supports cross-thread updates, if its <see cref="SupportCrossThreadStateUpdates"/> property is set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// <note><see cref="BaseForm"/> and <see cref="BaseUserControl"/> types already have a <see cref="BaseForm.CommandBindings"/> property, whose
    /// type is <see cref="WinFormsCommandBindingsCollection"/>.</note>
    /// </remarks>
    public class WinFormsCommandBindingsCollection : CommandBindingsCollection
    {
        #region CrossThreadEnabledStateUpdater class

#if NET6_0_OR_GREATER
        [SuppressMessage("Style", "IDE0004:Remove unnecessary cast", Justification = "Needed in pre-.NET 6")]
#endif
        private sealed class CrossThreadEnabledStateUpdater : ICommandStateUpdater
        {
            #region Fields

            internal static readonly CrossThreadEnabledStateUpdater Instance = new CrossThreadEnabledStateUpdater();

            #endregion

            #region Methods

            public bool TryUpdateState(object commandSource, string stateName, object? value)
            {
                if (stateName == nameof(ICommandState.Enabled))
                {
                    switch (commandSource)
                    {
                        case Control control:
                            bool enabled = value is true;
                            if (control.InvokeRequired)
                                control.Invoke(new Action(() => control.Enabled = enabled));
                            else
                                control.Enabled = enabled;
                            return true;

                        case ToolStripItem item:
                            enabled = value is true;
                            if (item.Owner?.InvokeRequired == true)
                                item.Owner.Invoke(new Action(() => item.Enabled = enabled));
                            else
                                item.Enabled = enabled;
                            return true;

                        default:
                            return false;
                    }
                }

                return commandSource switch
                {
                    Control control => control.InvokeRequired ? (bool)control.Invoke(SetState) : SetState(),
                    ToolStripItem item => item.Owner?.InvokeRequired == true ? (bool)item.Owner.Invoke(SetState) : SetState(),
                    _ => false
                };

                #region Local Methods

                bool SetState() => Reflector.TrySetProperty(commandSource, stateName, value);

                #endregion
            }

            public void Dispose()
            {
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether cross-thread state updates are supported.
        /// It is relevant when the command source is a <see cref="Control"/> or a <see cref="ToolStripItem"/>, and the command states
        /// (e.g. <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_ComponentModel_ICommandState_Enabled.htm">Enabled</a>)
        /// may change from a different thread than the one the command binding was created on.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <note>This property should be set before adding a binding to the collection. Already added bindings will not be affected by this property.</note>
        /// </remarks>
        public bool SupportCrossThreadStateUpdates { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ICommandBinding Add(ICommand command, IDictionary<string, object?>? initialState = null, bool disposeCommand = false)
        {
            ICommandBinding result = base.Add(command, initialState, disposeCommand);

            // thread-safe updates for Control and ToolStripItem sources
            if (SupportCrossThreadStateUpdates)
                result.AddStateUpdater(CrossThreadEnabledStateUpdater.Instance);

            result
                // updater for non-Control/ToolStripItem sources, or when cross-thread updates are not enabled
                .AddStateUpdater(PropertyCommandStateUpdater.Updater)
                // updater for ToolTipText state when there is no ToolTipText property on the source
                .AddStateUpdater(ToolTipTextCommandStateUpdater.Updater);

            return result;
        }

        #endregion
    }
}