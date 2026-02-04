#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LocalizableControlDemo.cs
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

using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms.Example.Controls
{
    public partial class LocalizableControlDemo : BaseUserControl
    {
        #region Constructors

        public LocalizableControlDemo()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (Dock != DockStyle.Top)
                return base.GetPreferredSize(proposedSize);
            var proposedAutoHeight = new Size(proposedSize.Width, 0);
            return new Size(proposedSize.Width, Padding.Vertical
                + lblLocalizableControlCaption.GetPreferredSize(proposedAutoHeight).Height
                + btnLocalizableControl.GetPreferredSize(proposedAutoHeight).Height
                + (Height - ClientSize.Height));
        }

        #endregion
    }
}