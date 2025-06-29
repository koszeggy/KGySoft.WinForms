#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ToolTipTextCommandStateUpdater.cs
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
using System.Windows.Forms;

using KGySoft.WinForms;

#endregion

// ReSharper disable once CheckNamespace
namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Provides special handling for ToolTipText: tries to find the associated <see cref="ToolTip"/> component.
    /// </summary>
    internal class ToolTipTextCommandStateUpdater : ICommandStateUpdater
    {
        #region Constants

        private const string ToolTipTextProperty = "ToolTipText";

        #endregion

        #region Fields

        private static readonly ToolTipTextCommandStateUpdater instance = new ToolTipTextCommandStateUpdater();

        #endregion

        #region Properties

        internal static ICommandStateUpdater Updater => instance;

        #endregion

        #region Constructors

        private ToolTipTextCommandStateUpdater()
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        public bool TryUpdateState(object commandSource, string stateName, object? value)
        {
            if (stateName != ToolTipTextProperty || value is not string text || commandSource is not Control control)
                return false;

            control.TryGetToolTip()?.SetToolTip(control, text);
            return true;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IDisposable.Dispose()
        {
        }

        #endregion

        #endregion
    }
}
