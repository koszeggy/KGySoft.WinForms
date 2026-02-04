#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Language.cs
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
using System.Globalization;
using System.Text;

using KGySoft.WinForms;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

#pragma warning disable CS1574 // the documentation contains types that are not available in every target

// ReSharper disable once CheckNamespace - compatibility with the old Language class
namespace KGySoft.Libraries.Language
{
    /// <summary>
    /// A class that makes localization possible in a very simple way, without the need of additional .dll files.
    /// Resources are stored in .resx files, which can be both read and created on-the-fly.
    /// Dynamic expansion of dictionaries are supported, including create new dictionaries for new languages.
    /// </summary>
    /// <remarks>
    /// <note type="warning">This class has been obsoleted. The way it worked was really non-professional:
    /// instead of looking up real keys from resources, it took the original untranslated terms and used them as keys to look them up
    /// in localized resources. To avoid conflicts, it supported so-called "distinction postfixes". Though it worked from
    /// .resx files, the system-provided culture hierarchy was omitted, only neutral (non-specific) cultures were supported.
    /// To overcome all these issues and still use .resx-based dynamically generated localizations you can use the
    /// <see cref="LocalizationHelper"/> class (recommended when you use the <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
    /// property of <see cref="BaseForm"/> or <see cref="BaseUserControl"/> classes), or the
    /// <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_DynamicResourceManager.htm" target="_blank">DynamicResourceManager</a> class instead.</note>
    /// </remarks>
    /// <seealso cref="LocalizationHelper"/>
    /// <seealso cref="LocalizationHelper.GetString(string,LocalizationContext)"/>
    /// <seealso cref="BaseForm.DynamicStringLocalization"/>
    [Obsolete("Use LocalizationHelper, or the DynamicResourceManager class from KGySoft.CoreLibraries instead")]
    public static class Language
    {
        #region Constants

        //private const string UntranslatedPrefix = "!T!: ";

        /// <summary>
        /// Indicates a distinction part of the string that will be removed on translation.
        /// Using DistinctionSeparator with a custom postfix makes possible store different
        /// translations for the same words or texts in different occasions.
        /// </summary>
        public const string DistinctionSeparator = "__";

        #endregion

        #region Public methods

        /// <summary>
        /// Translates the given invariant text to the currently set <see cref="ActiveLanguage"/>.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <remarks><note type="warning">This method does not translate anything anymore, just removes the possibly existing distinction postfix. Use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext)">LocalizationHelper.GetString</see> method or the
        /// <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_DynamicResourceManager.htm" target="_blank">DynamicResourceManager</a> class instead.</note></remarks>
        public static string Translate(string? text)
        {
            if (text == null!)
                return String.Empty;

            if (text.Trim().Length == 0)
                return text;

            return RemoveDistinctionSeparator(text);

            //// no translation needed
            //if (LanguageSettings.DisplayLanguage.Equals(CultureInfo.InvariantCulture) || activeLanguage.Equals(invariantLanguage))
            //    return RemoveDistinctionSeparator(text);

            //// translate redirection
            //if (RedirectTranslate != null)
            //{
            //    RedirectTranslateEventArgs args = new RedirectTranslateEventArgs(text);
            //    RedirectTranslate(null, args);
            //    return args.TextToTranslate;
            //}

            //// translation
            //string result;
            //Debug.Assert(!text.StartsWith(UntranslatedPrefix, StringComparison.Ordinal), "Translation of already translated text: " + text);
            //if (!dictionary.TryGetValue(text, out result))
            //{
            //    // translation not found
            //    result = StoreNewItem(text);
            //}
            //else if (!showUntranslatedPrefix && result.StartsWith(UntranslatedPrefix, StringComparison.Ordinal))
            //{
            //    result = result.Remove(0, UntranslatedPrefix.Length);
            //}

            //return result;
        }

        /// <summary>
        /// Translates the invariant text containing placeholders using the culture
        /// of currently set <see cref="ActiveLanguage"/>
        /// </summary>
        /// <param name="text">Invariant text with placeholders.</param>
        /// <param name="args">Arguments for placeholders.</param>
        /// <returns></returns>
        /// <remarks><note type="warning">This method does not translate anything anymore, just removes the possibly existing distinction postfix. Use the
        /// <see cref="LocalizationHelper.GetString(string,LocalizationContext,object[])">LocalizationHelper.GetString</see> method or the
        /// <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_DynamicResourceManager.htm" target="_blank">DynamicResourceManager</a> class instead.</note></remarks>
        public static string Translate(string text, params object[]? args) => args is null || args.Length == 0
            ? Translate(text)
            : String.Format(LanguageSettings.FormattingLanguage, Translate(text), args);

        /// <summary>
        /// Translates the invariant text containing placeholders to the currently
        /// set <see cref="ActiveLanguage"/> using the given culture.
        /// </summary>
        /// <param name="formattingCulture">Culture for formatting arguments.</param>
        /// <param name="text">Invariant text with placeholders.</param>
        /// <param name="args">Arguments for placeholders.</param>
        /// <returns></returns>
        /// <remarks><note type="warning">This method does not translate anything anymore, just removes the possibly existing distinction postfix. Use the
        /// <see cref="LocalizationHelper.GetString(CultureInfo,string,LocalizationContext,object[])">LocalizationHelper.GetString</see> method or the
        /// <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_DynamicResourceManager.htm" target="_blank">DynamicResourceManager</a> class instead.</note></remarks>
        public static string Translate(CultureInfo formattingCulture, string text, params object[]? args) => args is null || args.Length == 0
            ? Translate(text)
            : String.Format(formattingCulture, Translate(text), args);

        /// <summary>
        /// Saves the dictionary if <see cref="ActiveLanguage"/> is not the invariant culture.
        /// </summary>
        /// <remarks><note type="warning">This method doesn't do anything anymore. Use the
        /// <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_DynamicResourceManager.htm" target="_blank">DynamicResourceManager</a> class instead.</note></remarks>
        public static void SaveDictionary()
        {
            //SaveDictionary(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Makes translation enabled or disabled for given objects.
        /// </summary>
        /// <param name="objects">The objects that are subjects of translation marking.</param>
        /// <param name="translationEnabled">True if translation is enabled for the object, otherwise, false.</param>
        /// <remarks>
        /// <note>
        /// "Unmarking" objects is not necessary because this method does not keep any reference of the marked objects.
        /// In other words, using this method does not disturb garbage collection and causes no memory leak.
        /// </note>
        /// </remarks>
        public static void MarkLocalizable(bool translationEnabled, params object[] objects)
        {
            foreach (object obj in objects)
            {
                TypeDescriptor.AddAttributes(obj, translationEnabled ? LocalizableAttribute.Yes : LocalizableAttribute.No);
            }
        }

        /// <summary>
        /// Gets whether an object is localizable. By default, an object is localizable.
        /// This can be changed either by making a type not localizable with <see cref="LocalizableAttribute"/>
        /// or by <see cref="MarkLocalizable"/> method, which works also at runtime.
        /// </summary>
        public static bool IsObjectLocalizable(object obj)
        {
            AttributeCollection attrs = TypeDescriptor.GetAttributes(obj);
            // cannot use indexer because default of LocalizableAttribute is false, while here localization is considered true by default
            for (int i = 0; i < attrs.Count; i++)
            {
                if (attrs[i] is LocalizableAttribute localizable)
                    return localizable.IsLocalizable;
            }

            return true;
        }

        /// <summary>
        /// Gets whether a property is localizable. By default, a property is not localizable unless it is marked
        /// by <see cref="LocalizableAttribute"/>.
        /// </summary>
        /// <remarks>
        /// A property is not considered localizable by default, because here only properties with LocalizableAttribute should be checked.
        /// <note>Checking whether a property is localizable in a .NET framework class could be needed only in case of virtual properties.</note>
        /// </remarks>
        public static bool IsPropertyLocalizable(object obj, string propertyName) => TypeDescriptor.GetProperties(obj)[propertyName]?.IsLocalizable == true;

        /// <summary>
        /// Formats captions:
        /// <para>- Capitalizes first letter</para>
        /// <para>- Inserts spaces before capitals except in case of multiple capitals</para>
        /// </summary>
        /// <example>
        /// For example:
        /// <para>"columnHeader" -> "Column Header"</para>
        /// <para>"KGySOFTLibraries" -> "KGy SOFT Libraries"</para>
        /// </example>
        public static string FormatCaption(string caption)
        {
            if (string.IsNullOrEmpty(caption))
                return string.Empty;

            StringBuilder ret = new StringBuilder(caption);
            int i = 1;
            while (i < ret.Length)
            {
                char prev = ret[i - 1];
                char act = ret[i];

                if (Char.IsUpper(act) && Char.IsLower(prev))
                {
                    ret.Insert(i, ' ');
                    i++;
                }
                i++;
            }
            // Capitalize first char
            if (!Char.IsUpper(ret[0]))
            {
                ret.Insert(0, Char.ToUpperInvariant(ret[0]));
                ret.Remove(1, 1);
            }
            //// Eliminate doubled spaces
            //ret.Replace("  ", " ");
            return ret.ToString();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Removes distinction separator from the end of invariant text.
        /// </summary>
        private static string RemoveDistinctionSeparator(string text)
        {
            int sep = text.LastIndexOf(DistinctionSeparator, StringComparison.Ordinal);

            return sep >= 0 ? text.Substring(0, sep) : text;
        }

        #endregion
    }
}
