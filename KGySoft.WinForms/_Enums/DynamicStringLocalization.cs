#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DynamicStringLocalization.cs
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

using System.Resources;

using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents the possible modes of dynamic string localization for forms and user controls
    /// derived from <see cref="BaseForm"/> and <see cref="BaseUserControl"/>.
    /// <br/>See also the <strong>Remarks</strong> section of the <see cref="BaseForm.DynamicStringLocalization"/> property for details.
    /// </summary>
    /// <remarks>
    /// <para></para>
    /// </remarks>
    public enum DynamicStringLocalization
    {
        /// <summary>
        /// Specifies no automatic string localization for the controls of the current <see cref="BaseForm"/> or <see cref="BaseUserControl"/>.
        /// If <see cref="BaseUserControl.DynamicStringLocalization"/> is disabled for a <see cref="BaseUserControl"/>, a parent <see cref="BaseUserControl"/>
        /// or <see cref="BaseForm"/> can still apply an automatic localization, if it has a non-disabled <see cref="DynamicStringLocalization"/> mode.
        /// </summary>
        Disabled,

        /// <summary>
        /// Specifies that the <see cref="BaseForm"/> or <see cref="BaseUserControl"/> will automatically apply string resources to its controls when the control is loaded,
        /// or when <see cref="LanguageSettings.DisplayLanguage">LanguageSettings.DisplayLanguage</see> is set to a different language than the current one.
        /// The localization occurs for localizable string properties of the controls if their <c>ControlName.PropertyName</c> entry exists in the invariant resource file.
        /// The name of the resource should follow the pattern <c>MyNamespace.MyFormOrUserControl.StringResources</c>.
        /// The resource can be either compiled to the project or placed in the <c>Resources</c> folder of the build output directory as a .resx file.
        /// This option allows generating new resource files in .resx format when requesting localization for a language that has no resources yet.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BaseForm.DynamicStringLocalization"/> property for more details.
        /// </summary>
        LocalScope,

        /// <summary>
        /// Similar to <see cref="LocalScope"/>, except that a single resource file is used for all forms and user controls in the same assembly,
        /// using the pattern <c>MyAssemblyName.StringResources</c>.
        /// If you use this option, you must ensure that controls with potentially different texts have distinct names.
        /// </summary>
        AssemblyScope,

        /// <summary>
        /// Specifies that though the <see cref="BaseForm"/> or <see cref="BaseUserControl"/> traverses its controls when it is loaded,
        /// it does not apply any localization automatically. You need to handle the <see cref="LocalizationHelper.LocalizationRequested"/> event to
        /// provide a localization for the enumerated properties of the controls.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BaseForm.DynamicStringLocalization"/> property for more details.
        /// </summary>
        Custom
    }
}