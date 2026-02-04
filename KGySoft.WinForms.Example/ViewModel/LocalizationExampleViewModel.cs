#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LocalizationExampleViewModel.cs
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using KGySoft.Collections;
using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.WinForms.Example.ViewModel
{
    internal sealed class LocalizationExampleViewModel : ObservableObjectBase
    {
        #region Constants

        private const string resourcesDir = "Resources";

        #endregion

        #region Fields

        private static StringKeyedDictionary<CultureInfo>? culturesCache;

        #endregion

        #region Properties

        #region Static Properties

        private static StringKeyedDictionary<CultureInfo> CulturesCache
            => culturesCache ??= CultureInfo.GetCultures(CultureTypes.AllCultures).ToStringKeyedDictionary(ci => ci.Name);

        #endregion

        #region Instance Properties

        internal Action? ApplyLocalizationCallback { get; set; }
        internal IList<KeyValuePair<CultureInfo, string>> Languages { get => Get<IList<KeyValuePair<CultureInfo, string>>>(); set => Set(value); }
        internal bool ExistingLanguagesOnly { get => Get(true); set => Set(value); }
        internal CultureInfo SelectedLanguage { get => Get<CultureInfo>(); set => Set(value); }
        internal bool UseCustomLocalization { get => Get<bool>(); set => Set(value); }
        
        internal ICommand ApplyCommand => Get(() => new SimpleCommand(OnApplyCommand));
        internal ICommandState ApplyCommandState => Get(() => new CommandState { Enabled = false });

        #endregion

        #endregion

        #region Constructors

        public LocalizationExampleViewModel()
        {
            ResetLanguages();
            SelectedLanguage = GetClosestNeutralCulture(LanguageSettings.DisplayLanguage);
            LocalizationHelper.LocalizationRequested += LocalizationHelper_LocalizationRequested;
        }

        #endregion

        #region Methods

        #region Static Methods

        internal static CultureInfo GetClosestNeutralCulture(CultureInfo culture)
        {
            while (!culture.IsNeutralCulture)
                culture = culture.Parent;

            return culture;
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        protected override void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(SelectedLanguage):
                    ApplyCommandState.Enabled = !SelectedLanguage.Equals(GetClosestNeutralCulture(LanguageSettings.DisplayLanguage));
                    break;

                case nameof(ExistingLanguagesOnly):
                    ResetLanguages();
                    break;

                case nameof(UseCustomLocalization):
                    ApplyLocalizationCallback?.Invoke();
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            LocalizationHelper.LocalizationRequested -= LocalizationHelper_LocalizationRequested;
            if (disposing)
                ApplyLocalizationCallback = null;
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void LocalizationHelper_LocalizationRequested(object? sender, LocalizationRequestedEventArgs e)
        {
            if (UseCustomLocalization)
                e.Value = e.Key; // we simply return the resource key as the value for demonstration purposes
        }

        private void ResetLanguages()
        {
            var selected = SelectedLanguage;
            IEnumerable<CultureInfo> languages;
            if (ExistingLanguagesOnly)
            {
                // creating the possibly unsaved .resx files
                LocalizationHelper.SavePendingScopedResources();

                var result = new HashSet<CultureInfo>();
                CultureInfo invariantLanguage = CultureInfo.GetCultureInfo("en"); // The language of the invariant culture specified by the NeutralResourcesLanguage attribute
                result.Add(invariantLanguage);
                result.Add(GetClosestNeutralCulture(LanguageSettings.DisplayLanguage));
                string dir = Path.GetFullPath(Path.Combine(Files.GetExecutingPath(), resourcesDir));
                string baseName = LocalizationHelper.GetResourceBaseName(Assembly.GetExecutingAssembly());
                if (Directory.Exists(dir))
                {
                    int startIndex = dir.Length + baseName.Length + 2;
                    string[] files = Directory.GetFiles(dir, $"{baseName}.*.resx", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        StringSegment resName = file.AsSegment(startIndex, file.Length - startIndex - 5);
                        if (CulturesCache.TryGetValue(resName, out CultureInfo? ci) && !ci.Equals(CultureInfo.InvariantCulture))
                            result.Add(ci);
                    }
                }

                if (!result.Contains(selected))
                    selected = invariantLanguage;

                languages = result;
            }
            else
                languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures);

            Languages = languages
                .Select(ci => new KeyValuePair<CultureInfo, string>(ci, $"{ci.NativeName} ({ci.EnglishName})"))
                .OrderBy(l => l.Value)
                .ToList();
            SelectedLanguage = selected;
        }

        private void OnApplyCommand()
        {
            ApplyCommandState.Enabled = false;
            LanguageSettings.DisplayLanguage = SelectedLanguage;
        }

        #endregion

        #endregion

        #endregion
    }
}
