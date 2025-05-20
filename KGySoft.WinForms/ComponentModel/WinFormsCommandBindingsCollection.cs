#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WinFormsCommandBindingsCollection.cs
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
using System.Collections.Generic;
using System.Windows.Forms;

using KGySoft.Reflection;

#endregion

// ReSharper disable once CheckNamespace
namespace KGySoft.ComponentModel
{
    /// <summary>
    /// A specialized <see cref="CommandBindingsCollection"/> that can be used for commands with <see cref="Control"/> sources.
    /// By using this collection the <see cref="ICommandState"/> properties (e.g. <see cref="ICommandState.Enabled"/> but also any other added property)
    /// of the added bindings will be synced with the command sources.
    /// </summary>
    public class WinFormsCommandBindingsCollection : CommandBindingsCollection
    {
        #region CrossThreadEnabledStateUpdater class

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

                Func<bool> setState = () => Reflector.TrySetProperty(commandSource, stateName, value);
                return commandSource switch
                {
                    Control control => control.InvokeRequired ? control.Invoke(setState) : setState.Invoke(),
                    ToolStripItem item => item.Owner?.InvokeRequired == true ? item.Owner.Invoke(setState) : setState.Invoke(),
                    _ => false
                };
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
        /// It is relevant when the command source is a <see cref="Control"/> or a <see cref="ToolStripItem"/>, and the command state changes
        /// (such as <see cref="ICommandState.Enabled"/>) can be updated from a different thread than the one the command binding was created on.
        /// <br/>Default value: <see langrowd="false"/>.
        /// </summary>
        /// <remarks>
        /// <note>This property should be set before adding a binding to the collection. Already added bindings will not be affected by this property.</note>
        /// </remarks>
        public bool SupportCrossThreadStateUpdates { get; set; }

        #endregion

        #region Methods

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