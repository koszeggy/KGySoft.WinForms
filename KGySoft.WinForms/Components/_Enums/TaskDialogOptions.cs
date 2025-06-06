#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogOptions.cs
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

#endregion

namespace KGySoft.WinForms.Components
{
    /// <summary>
    /// Represents possible options of a <see cref="TaskDialog"/>
    /// </summary>
    [Flags]
    public enum TaskDialogOptions
    {
        /// <summary>
        /// Represents no options
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the <see cref="TaskDialog"/> should resolve hyperlinks.
        /// </summary>
        HyperlinksEnabled = 1,

        // Handled automatically, and can be changed while running
        //UseMainIcon = 0x0002,
        //UseFooterIcon = 0x0004,

        /// <summary>
        /// Indicates that the <see cref="TaskDialog"/> should be able to be closed using Alt-F4, Escape and the title bar’s
        /// close button even if no cancel button is specified in <see cref="TaskDialog.StandardButtons"/>.
        /// </summary>
        AllowCancel = 1 << 3,

        /// <summary>
        /// Indicates that the <see cref="TaskDialog.Buttons"/> should be displayed as command links
        /// (using a standard task dialog glyph) instead of simple push buttons.
        /// If both <see cref="UseCommandLinks"/> and <see cref="UseCommandLinksNoIcon"/> are set, glyphs will be visible.
        /// </summary>
        UseCommandLinks = 1 << 4,

        /// <summary>
        /// Determines whether the <see cref="TaskDialog.Buttons"/> should be displayed as command links
        /// (without a glyph) instead of simple push buttons.
        /// If both <see cref="UseCommandLinks"/> and <see cref="UseCommandLinksNoIcon"/> are set, glyphs will be visible.
        /// </summary>
        UseCommandLinksNoIcon = 1 << 5,

        /// <summary>
        /// Indicates that the details text of a <see cref="TaskDialog"/> should be displayed at the footer area instead of
        /// immediately after the message. This flag is ignored when <see cref="TaskDialog.DetailsText"/> is empty
        /// </summary>
        ExpandFooterArea = 1 << 6,

        /// <summary>
        /// Indicates that details should be expanded when the <see cref="TaskDialog"/> appears.
        /// This flag is ignored when <see cref="TaskDialog.DetailsText"/> is empty.
        /// </summary>
        DetailsExpanded = 1 << 7,

        // Maintaned via a property because can be changed while running
        //CheckBoxChecked = 0x0100,
        //ShowProgressBar = 0x0200,
        //ShowMarqueeProgressBar = 0x0400,
        //UseCallbackTimer = 0x0800,

        /// <summary>
        /// Indicates that the <see cref="TaskDialog"/> should be centered to the owner window rather than the monitor.
        /// </summary>
        PositionRelativeToWindow = 1 << 12,

        /// <summary>
        /// Indicates that the <see cref="TaskDialog"/> should appear in right-to-left layout.
        /// </summary>
        RightToLeftLayout = 1 << 13,

        //Handled automatically:
        //NoDefaultRadioButton = 0x4000

        /// <summary>
        /// Indicates that the <see cref="TaskDialog"/> can be minimized.
        /// </summary>
        AllowMinimize = 1 << 15,

        // NOTE: Below there are non-native features. These flags should be masked out when assigning to TASKDIALOG_FLAGS

        ///// <summary>
        ///// Indicates that <see cref="TaskDialog.DetailsText"/> should be selectable so it can
        ///// easily copied to the clipboard.
        ///// <note>This flag causes to use <see cref="TaskDialog"/> in compatibility mode.</note>
        ///// </summary>
        //DetailsSelectable = 1 << 16

        /// <summary>
        /// Indicates that texts of <see cref="TaskDialog.StandardButtons"/> are localized from the library resources
        /// rather than using Windows resources.
        /// <note>This flag causes to use <see cref="TaskDialog"/> in compatibility mode.</note>
        /// </summary>
        TranslateStandardButtons = 1 << 17
    }
}
