#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucAllInvertNone.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a selector control that contains three buttons: All, Invert, and None.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("It is not recommended to use this class anymore.")]
    public partial class ucAllInvertNone : UserControl
    {
        #region Events

        /// <summary>
        /// Occurs when a button is pressed.
        /// </summary>
        [Category("ucAllInvertNone")]
        [Description("Occurs when a button is pressed.")]
        public event EventHandler<AllInvertNoneEventArgs>? ButtonPressed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucAllInvertNone"/> class.
        /// </summary>
        public ucAllInvertNone()
        {
            InitializeComponent();

            buttonNone.Image = Properties.Resources.None;
            buttonInvert.Image = Properties.Resources.Options;
            buttonAll.Image = Properties.Resources.All;

            buttonAll.Click += buttonAll_Click;
            buttonInvert.Click += buttonInvert_Click;
            buttonNone.Click += buttonNone_Click;
        }

        #endregion

        #region Methods

        void buttonNone_Click(object? sender, EventArgs e)
        {
            ButtonPressed?.Invoke(this, new AllInvertNoneEventArgs(InvertButtonTypes.None));
        }

        void buttonInvert_Click(object? sender, EventArgs e)
        {
            ButtonPressed?.Invoke(this, new AllInvertNoneEventArgs(InvertButtonTypes.Invert));
        }

        void buttonAll_Click(object? sender, EventArgs e)
        {
            ButtonPressed?.Invoke(this, new AllInvertNoneEventArgs(InvertButtonTypes.All));
        }

        #endregion
    }
}
