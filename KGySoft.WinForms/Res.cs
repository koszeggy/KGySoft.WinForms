#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Globalization;

using KGySoft.Resources;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Contains the string resources of the project.
    /// </summary>
    internal static class Res
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.WinForms.Messages", typeof(Res).Assembly)
        {
            SafeMode = true,
            UseLanguageSettings = true,
        };

        #endregion

        #region Methods

        #region Internal Methods

        #region Accessors

        /// <summary>Instance property "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstancePropertyDoesNotExist(string? propertyName, Type type) => Get("Accessors_InstancePropertyDoesNotExistFormat", propertyName, type);

        /// <summary>Static property "{0}" not found on type "{1}".</summary>
        internal static string AccessorsStaticPropertyDoesNotExist(string? propertyName, Type type) => Get("Accessors_StaticPropertyDoesNotExistFormat", propertyName, type);

        /// <summary>Instance field "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstanceFieldDoesNotExist(string? fieldName, Type type) => Get("Accessors_InstanceFieldDoesNotExistFormat", fieldName, type);

        /// <summary>Static field "{0}" not found on type "{1}".</summary>
        internal static string AccessorsStaticFieldDoesNotExist(string? fieldName, Type type) => Get("Accessors_StaticFieldDoesNotExistFormat", fieldName, type);

        /// <summary>Instance method "{0}" not found on type "{1}".</summary>
        internal static string AccessorsInstanceMethodDoesNotExist(string? methodName, Type type) => Get("Accessors_InstanceMethodDoesNotExistFormat", methodName, type);

        #endregion

        #endregion

        #region Private Methods

        private static string Get(string id) => resourceManager.GetString(id, LanguageSettings.DisplayLanguage) ?? String.Format(CultureInfo.InvariantCulture, unavailableResource, id);

        private static string Get(string id, params object?[]? args)
        {
            string format = Get(id);
            return args == null ? format : SafeFormat(format, args);
        }

        private static string SafeFormat(string format, object?[] args)
        {
            try
            {
                int i = Array.IndexOf(args, null);
                if (i >= 0)
                {
                    string nullRef = PublicResources.Null;
                    for (; i < args.Length; i++)
                        args[i] ??= nullRef;
                }

                return String.Format(LanguageSettings.FormattingLanguage, format, args);
            }
            catch (FormatException)
            {
                return String.Format(CultureInfo.InvariantCulture, invalidResource, args.Length, format);
            }
        }

        #endregion

        #endregion
    }
}
