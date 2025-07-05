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
using System.Reflection;
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
        /// Can be an <see cref="Assembly"/> name if this <see cref="LocalizationContext"/> was created using the <see cref="FromAssemblyScope"/> method.
        /// </summary>
        public string RootName { get; }

        /// <summary>
        /// Gets the type of the root control or object that is being localized.
        /// Can be <see langword="null"/> if this <see cref="LocalizationContext"/> was created using the <see cref="FromAssemblyScope"/> method.
        /// </summary>
        public Type? RootType { get; }

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
            DynamicStringLocalization.LocalScope => RootType!.AssemblyQualifiedName!,
            DynamicStringLocalization.AssemblyScope => "!" + RootType!.Assembly.FullName,
            _ => throw new InvalidOperationException(Res.InternalError($"CacheKey was requested for unexpected scope: {LocalizationScope}"))
        };

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

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

        #region Private Constructors

        private LocalizationContext(DynamicStringLocalization scope, string cacheKey, string name, Type? type, CultureInfo? language)
        {
            LocalizationScope = scope;
            this.cacheKey = cacheKey;
            RootName = name;
            RootType = type;
            LanguageHint = language ?? LanguageSettings.DisplayLanguage;
        }

        #endregion


        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Creates a <see cref="LocalizationContext"/> from the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to create the context from.</param>
        /// <param name="language">A suggested language for the localization operation. If <see langword="null"/>, the current display language is used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A new <see cref="LocalizationContext"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static LocalizationContext FromAssemblyScope(Assembly assembly, CultureInfo? language = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), PublicResources.ArgumentNull);
            return new LocalizationContext(DynamicStringLocalization.AssemblyScope, "!" + assembly.FullName, assembly.GetName().Name!, null, language);
        }

        /// <summary>
        /// Creates a <see cref="LocalizationContext"/> from the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to create the context from.</param>
        /// <param name="language">A suggested language for the localization operation. If <see langword="null"/>, the current display language is used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A new <see cref="LocalizationContext"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> does not have an <see cref="Type.AssemblyQualifiedName"/>.</exception>
        public static LocalizationContext FromLocalScope(Type type, CultureInfo? language = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), PublicResources.ArgumentNull);
            string aqn = type.AssemblyQualifiedName ?? throw new ArgumentException(PublicResources.PropertyNull(nameof(Type.AssemblyQualifiedName)), nameof(type));
            return new LocalizationContext(DynamicStringLocalization.LocalScope, aqn, type.Name, type, language);
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="LocalizationContext"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is LocalizationContext other
            && CacheKey == other.CacheKey
            && RootType == other.RootType
            && RootName == other.RootName
            && LanguageHint.Equals(other.LanguageHint);

        /// <summary>
        /// Returns a hash code for the current <see cref="LocalizationContext"/> instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => CacheKey.GetHashCode();

        /// <summary>
        /// Returns a string that represents the current <see cref="LocalizationContext"/> instance.
        /// </summary>
        /// <returns>A string that represents the current instance.</returns>
        public override string ToString() => $"{RootName} ({LocalizationScope}) [{LanguageHint}]";

        #endregion

        #endregion
    }
}
