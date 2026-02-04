#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EditResourcesViewModel.cs
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using KGySoft.ComponentModel;
using KGySoft.Reflection;
using KGySoft.Resources;

#endregion

namespace KGySoft.WinForms.Example.ViewModel
{
    internal class EditResourcesViewModel : ObservableObjectBase
    {
        #region Nested classes

        private sealed class ResourceSetEntry // not a ValueTuple so IsModified can be set without resetting the value in resources dictionary
        {
            #region Fields

            internal SortableBindingList<ResourceEntry> ResourceSet = null!;
            internal bool IsModified;

            #endregion
        }

        #endregion

        #region Constants

        private const string compiledResourcesPostfix = ".StringResources.resources";

        #endregion

        #region Fields

        private readonly LocalizationExampleViewModel parent;
        private readonly Dictionary<LocalizationContext, ResourceSetEntry> resources = new();

        #endregion

        #region Properties

        internal bool IsDirty { get; private set; }
        internal CultureInfo EditedLanguage { get; }
        internal Action? ApplyLocalizationCallback { get; set; }
        internal KeyValuePair<LocalizationContext, string>[] ResourceFiles { get; }
        internal LocalizationContext SelectedResource { get => Get<LocalizationContext>(); set => Set(value); }
        internal SortableBindingList<ResourceEntry> ResourceEntries { get => Get<SortableBindingList<ResourceEntry>>(); set => Set(value); }

        internal ICommand ApplyResourcesCommand => Get(() => new SimpleCommand(OnApplyResourcesCommand));
        internal ICommand SaveResourcesCommand => Get(() => new SimpleCommand(OnSaveResourcesCommand));
        internal ICommand CancelEditCommand => Get(() => new SimpleCommand(OnCancelEditCommand));

        internal ICommandState ApplyResourcesCommandState => Get(() => new CommandState());

        #endregion

        #region Constructors

        internal EditResourcesViewModel(LocalizationExampleViewModel parent)
        {
            this.parent = parent;
            EditedLanguage = parent.SelectedLanguage;
            ResourceFiles = GetResourceFiles();
            SelectedResource = ResourceFiles[0].Key;
            bool isModified = ApplyResourcesCommandState.Enabled = !Equals(EditedLanguage, LanguageSettings.DisplayLanguage);
            SetModified(isModified); // initialized to true if the edited language is not the display language
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override bool AffectsModifiedState(string propertyName) => false; // setting IsModified manually

        protected override void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(IsModified):
                    ApplyResourcesCommandState.Enabled = IsModified;
                    break;

                case nameof(SelectedResource):
                    LocalizationContext context = SelectedResource;
                    if (resources.TryGetValue(context, out ResourceSetEntry? value))
                        ResourceEntries = value.ResourceSet;
                    else
                    {
                        IExpandoResourceSet invariantSet = LocalizationHelper.GetResourceSet(context, CultureInfo.InvariantCulture)!;
                        IExpandoResourceSet translatedSet = LocalizationHelper.GetResourceSet(context)!;
                        var entries = new List<ResourceEntry>();
                        foreach (DictionaryEntry entry in invariantSet)
                        {
                            // entry.Value is string for compiled resources, and ResXDataNode for .resx files (because SafeMode is true by default), but ToString() works for both
                            // NOTE: if an attacker replaces a string to a malicious resource in the .resx file, the ToString just returns the raw string from the XML without loading any referenced types.
                            entries.Add(new ResourceEntry((string)entry.Key, entry.Value!.ToString()!, translatedSet.GetString((string)entry.Key)));
                        }

                        var result = new ResourceSetEntry { ResourceSet = new SortableBindingList<ResourceEntry>(entries) };
                        result.ResourceSet.ListChanged += (_, args) => // lambda is alright, dispose will perform the unsubscription implicitly
                        {
                            if (args.ListChangedType == ListChangedType.ItemChanged)
                            {
                                result.IsModified = true;
                                SetModified(true);
                            }
                        };

                        resources[SelectedResource] = result;
                        ResourceEntries = result.ResourceSet;
                    }

                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            foreach (ResourceSetEntry value in resources.Values)
                value.ResourceSet.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This example project uses compiled invariant resources so we can use Assembly.GetManifestResourceNames to detect the localizable resources.
        /// If you use exclusively .resx files, you can scan the Resources folder for .resx files instead.
        /// </summary>
        private KeyValuePair<LocalizationContext, string>[] GetResourceFiles()
        {
            var result = new List<KeyValuePair<LocalizationContext, string>>();
            foreach (string manifestResourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                // skipping everything but .StringResources (e.g. designer-generated resources for forms and user controls)
                if (!manifestResourceName.EndsWith(compiledResourcesPostfix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string name = manifestResourceName.Substring(0, manifestResourceName.Length - compiledResourcesPostfix.Length);
                Assembly? assembly = Reflector.ResolveAssembly(name, ResolveAssemblyOptions.None);
                Type? type = assembly is null ? Reflector.ResolveType(name, ResolveTypeOptions.ThrowError) : null;
                string displayName = type?.Name ?? name;
                var context = assembly != null
                        ? LocalizationContext.FromAssemblyScope(assembly, EditedLanguage)
                        : LocalizationContext.FromLocalScope(type!, EditedLanguage);
                result.Add(new KeyValuePair<LocalizationContext, string>(context, displayName));
            }

            return result.ToArray();
        }

        private void ApplyResources()
        {
            if (!IsModified)
                return;

            foreach (KeyValuePair<LocalizationContext, ResourceSetEntry> resource in resources)
            {
                // skipping unmodified resources
                if (!resource.Value.IsModified)
                    continue;

                // applying the modified resources to the live resource set (only in memory)
                SortableBindingList<ResourceEntry> viewModelSet = resource.Value.ResourceSet;
                IExpandoResourceSet liveSet = LocalizationHelper.GetResourceSet(resource.Key, EditedLanguage)!;
                foreach (ResourceEntry entry in viewModelSet)
                {
                    if (!entry.IsModified)
                        continue; // skipping unmodified entries

                    // updating the resource value in the live resource set
                    liveSet.SetObject(entry.Key, entry.TranslatedText);
                    entry.SetModified(false);
                }

                resource.Value.IsModified = false;
                IsDirty = true;
            }

            SetModified(false);
        }

        private void OnApplyResourcesCommand()
        {
            ApplyResources();

            // Switching the display language if needed, and applying the localization changes
            bool isLanguageChanging = !EditedLanguage.Equals(LanguageSettings.DisplayLanguage);
            if (isLanguageChanging)
                LanguageSettings.DisplayLanguage = EditedLanguage;
            else
            {
                // as the display language is not changing, resetting the localization manually
                ApplyLocalizationCallback?.Invoke();
                parent.ApplyLocalizationCallback?.Invoke();
            }
        }

        private void OnSaveResourcesCommand()
        {
            // Applying the resources happens in memory only
            ApplyResources();

            // Saving the actual resource files happens here. Possible exceptions are handled in the view by the binding.Error event,
            // so it can show a message box with the error message, and it can prevent closing the dialog.
            // NOTE: This saves scoped resources only. If you use centralized DynamicResourceManager instances,
            // you can use LanguageSettings.SavePendingResources instead, which may create the localized .resx files also for the dependent KGy SOFT assemblies,
            // as they also use centralized DynamicResourceManagers (if you opt in dynamic resources in LanguageSettings.DynamicResourceManagersSource).
            if (IsDirty)
                LocalizationHelper.SavePendingScopedResources();
        }

        private void OnCancelEditCommand()
        {
            if (!IsDirty)
                return;

            // If there were changes applied in memory, releasing the resource sets, so the original resources will be reloaded on demand
            LocalizationHelper.ReleaseAllScopedResources();
        }

        #endregion

        #endregion
    }
}
