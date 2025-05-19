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
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [Obsolete("It is not recommended to use this class anymore.")]
    public partial class ucAllInvertNone : UserControl
    {
        #region Events

        /// <summary>
        /// Occurs when a button is pressed.
        /// </summary>
        [Category("ucAllInvertNone")]
        [Description("Occurs when a button is pressed.")]
        public event EventHandler<AllInvertNoneEventArgs> ButtonPressed;

        #endregion

        #region Constructors

        public ucAllInvertNone()
        {
            InitializeComponent();

            this.buttonNone.Image = Properties.Resources.None;
            this.buttonInvert.Image = Properties.Resources.Options;
            this.buttonAll.Image = Properties.Resources.All;

            buttonAll.Click += new EventHandler(buttonAll_Click);
            buttonInvert.Click += new EventHandler(buttonInvert_Click);
            buttonNone.Click += new EventHandler(buttonNone_Click);
        }

        #endregion

        #region Methods

        void buttonNone_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.None));
        }

        void buttonInvert_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.Invert));
        }

        void buttonAll_Click(object sender, EventArgs e)
        {
            if (ButtonPressed != null)
                ButtonPressed(this, new AllInvertNoneEventArgs(InvertButtonTypes.All));
        }

        #endregion
    }
}
