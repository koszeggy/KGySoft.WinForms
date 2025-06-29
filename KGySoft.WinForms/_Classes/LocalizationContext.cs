#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LocalizationContext.cs
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
using System.Globalization;
using System.Windows.Forms;

using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Represents the context of a localization operation that can be useful when handling the <see cref="LocalizationHelper.LocalizationRequested"/> event,
    /// and can determine the automatic localization behavior of controls.
    /// </summary>
    public sealed class LocalizationContext
    {
        #region Fields

        private string? cacheKey;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the name of the root control or object that is being localized.
        /// </summary>
        public string RootName { get; }

        /// <summary>
        /// Gets the type of the root control or object that is being localized.
        /// </summary>
        public Type RootType { get; }

        /// <summary>
        /// Gets the culture of the suggested language for the localization operation.
        /// It is initialized by the UI culture of the current thread, but can be overridden by the handler of the <see cref="LocalizationHelper.LocalizationRequested"/> event.
        /// </summary>
        public CultureInfo LanguageHint { get; }

        /// <summary>
        /// Gets a <see cref="System.Windows.Forms.ToolTip"/> applicable for the root object, if any.
        /// When presents, the <see cref="LocalizationHelper.LocalizeStringProperties"/> method can apply <c>{name}.ToolTipText</c> resource keys for <see cref="Control"/> instances.
        /// </summary>
        public ToolTip? ToolTip { get; }

        /// <summary>
        /// Gets the scope of the localization operation. It is never <see cref="DynamicStringLocalization.Disabled"/> here.
        /// </summary>
        public DynamicStringLocalization LocalizationScope { get; }

        #endregion

        #region Internal Properties

        internal string CacheKey => cacheKey ??= LocalizationScope switch
        {
            DynamicStringLocalization.LocalScope => RootType.AssemblyQualifiedName!,
            DynamicStringLocalization.AssemblyScope => "!" + RootType.Assembly.FullName,
            _ => throw new InvalidOperationException(Res.InternalError($"CacheKey was requested for unexpected scope: {LocalizationScope}"))
        };

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationContext"/> class.
        /// </summary>
        /// <param name="root">The root control or object that is being localized.</param>
        /// <param name="language">The suggested language of the localization operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
        public LocalizationContext(object root, CultureInfo? language = null)
        {
            #region Local Methods

            static DynamicStringLocalization GetScope(object root)
            {
                Control? control = root switch // NOTE: see the switch in ApplyStringResources as well
                {
                    Control c => c,
                    ToolStripItem i => i.Owner,
                    DataGridViewColumn col => col.DataGridView,
                    _ => null
                };

                for (Control? c = control; c != null; c = c.Parent)
                {
                    switch (c)
                    {
                        case BaseUserControl { DynamicStringLocalization: DynamicStringLocalization.LocalScope or DynamicStringLocalization.AssemblyScope } bc:
                            return bc.DynamicStringLocalization;
                        case BaseForm { DynamicStringLocalization: not DynamicStringLocalization.Disabled } frm:
                            return frm.DynamicStringLocalization;
                    }
                }

                return DynamicStringLocalization.Custom;
            }

            #endregion

            if (root == null)
                throw new ArgumentNullException(nameof(root), PublicResources.ArgumentNull);
            Type type = root.GetType();
            string? name = (root as Control)?.Name;
            if (String.IsNullOrEmpty(name))
                name = type.Name;
            RootName = name;
            RootType = type;
            LanguageHint = language ?? LanguageSettings.DisplayLanguage;
            ToolTip = (root as Control)?.TryGetToolTip();
            LocalizationScope = GetScope(root);
        }

        #endregion
    }
}
