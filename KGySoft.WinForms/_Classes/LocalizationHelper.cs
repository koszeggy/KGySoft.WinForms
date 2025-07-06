#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LocalizationHelper.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Resources;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// Provides members for localization of Windows Forms controls and other objects.
    /// The <see cref="ApplyStringResources">ApplyStringResources</see> method is automatically called by <see cref="BaseForm"/> and <see cref="BaseUserControl"/> instances
    /// if their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see> property is set to a non-default value.
    /// The <see cref="LocalizationRequested"/> event can be handled to redirect the localization requests to a custom resource manager,
    /// or to set the value for a given key programmatically.
    /// </summary>
    public static class LocalizationHelper
    {
        #region Constants

        private const string toolTipTextPropertyName = "ToolTipText";
        private const string resourcesPostfix = ".StringResources";

        #endregion

        #region Fields

        private static EventHandler<LocalizationRequestedEventArgs>? localizationRequestedHandler;
        private static LockingDictionary<string, DynamicResourceManager>? resourceManagersCache;
        private static IThreadSafeCacheAccessor<Type, PropertyAccessor[]?>? localizableStringPropertiesCache;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the localization of a string is requested. Can be used to redirect the localization requests to a custom resource manager,
        /// or to set the value for a given key programmatically.
        /// </summary>
        /// <remarks>
        /// <para>Can be invoked indirectly by <see cref="BaseForm"/> and <see cref="BaseUserControl"/> instances when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see> property is set to a non-default value.
        /// If using <see cref="DynamicStringLocalization.LocalScope"/> or <see cref="DynamicStringLocalization.AssemblyScope"/> values, new .resx files may be created in the <c>Resources</c> subfolder of the executing application during runtime,
        /// when the handlers of this event do not set the <see cref="LocalizationRequestedEventArgs.Value"/> property.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BaseForm.DynamicStringLocalization"/> for details.</para>
        /// <note>Typically, it is not recommended to add multiple handlers to this event. Still, subscribers can check if <see cref="LocalizationRequestedEventArgs.Value"/>
        /// is not <see langword="null"/>, which means that a subscriber has already set the value for the given key.</note>
        /// </remarks>
        public static event EventHandler<LocalizationRequestedEventArgs>? LocalizationRequested
        {
            add => value.AddSafe(ref localizationRequestedHandler);
            remove => value.RemoveSafe(ref localizationRequestedHandler);
        }

        #endregion

        #region Properties

        // NOTE: not a ThreadSafeDictionary because we want the capacity management that Cache provides,
        // but not a IThreadSafeCacheAccessor either because we need to access Values as well
        private static IDictionary<string, DynamicResourceManager> ResourceManagersCache
        {
            get
            {
                if (resourceManagersCache == null)
                {
                    var cache = new Cache<string, DynamicResourceManager>(CreateResourceManager, 16)
                    {
                        DisposeDroppedValues = true
                    }.AsThreadSafe();
                    Interlocked.CompareExchange(ref resourceManagersCache, cache, null);
                }

                return resourceManagersCache;
            }
        }

        private static IThreadSafeCacheAccessor<Type, PropertyAccessor[]?> LocalizableStringPropertiesCache
        {
            get
            {
                if (localizableStringPropertiesCache == null)
                {
                    var options = new LockFreeCacheOptions()
                    {
                        InitialCapacity = 16,
                        ThresholdCapacity = 64,
                        MergeInterval = TimeSpan.FromMilliseconds(100)
                    };

                    var cache = ThreadSafeCacheFactory.Create<Type, PropertyAccessor[]?>(GetLocalizableStringProperties, options);
                    Interlocked.CompareExchange(ref localizableStringPropertiesCache, cache, null);
                }

                return localizableStringPropertiesCache;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Applies string resources to the specified control and its children recursively.
        /// Invokes the <see cref="LocalizationRequested"/> event for each localizable property of the control and its children.
        /// </summary>
        /// <param name="control">The root control to apply the string resources to.</param>
        /// <param name="context">The localization context to use for the operation. If <see langword="null"/>, a context is automatically created. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void ApplyStringResources(Control control, LocalizationContext? context = null)
        {
            #region Local Methods

            static void ApplyToolStripResources(ToolStripItemCollection items, LocalizationContext context)
            {
                foreach (ToolStripItem item in items)
                {
                    // to self
                    LocalizeStringProperties(item, item.Name ?? String.Empty, context);

                    // to children
                    if (item is ToolStripDropDownItem dropDownItem)
                        ApplyToolStripResources(dropDownItem.DropDownItems, context);
                }
            }

#if !NETCOREAPP3_1_OR_GREATER
            static void ApplyMenuResources(Menu.MenuItemCollection items, LocalizationContext context)
            {
                foreach (MenuItem item in items)
                {
                    // to self
                    LocalizeStringProperties(item, item.Name ?? String.Empty, context);

                    // to children
                    if (item.MenuItems.Count > 0)
                        ApplyMenuResources(item.MenuItems, context);
                }
            }
#endif

            #endregion

            context ??= new LocalizationContext(control);
            string name = control.Name;
            if (String.IsNullOrEmpty(name))
                name = control.GetType().Name;

            // custom localization
            if (control is ICustomLocalizable customLocalizable)
            {
                // TODO: detect recursion and throw an exception if it happens
                if (customLocalizable.ApplyStringResources(context))
                    return;
            }

            // applying localization to self properties...
            LocalizeStringProperties(control, name, context);

            // ...to context menu...
            if (control.ContextMenuStrip is ContextMenuStrip cms)
                ApplyToolStripResources(cms.Items, context);
#if !NETCOREAPP3_1_OR_GREATER
            else if (control.ContextMenu is ContextMenu contextMenu)
                ApplyMenuResources(contextMenu.MenuItems, context);

            // ... to main menu...
            if (control is Form { Menu: not null } form)
                ApplyMenuResources(form.Menu.MenuItems, context);
#endif

            // ... and to children
            switch (control) // NOTE: Apply LocalizationContext.ctor.GetScope for the same non-control sub-element types as well
            {
                case ToolStrip toolStrip:
                    ApplyToolStripResources(toolStrip.Items, context);
                    break;

                case DataGridView dataGridView:
                    foreach (DataGridViewColumn item in dataGridView.Columns)
                        LocalizeStringProperties(item, item.Name, context);
                    break;

                case ListView listView:
                    foreach (ColumnHeader header in listView.Columns)
                        LocalizeStringProperties(header, header.Name ?? String.Empty, context);
                    foreach (ListViewGroup group in listView.Groups)
                        // we could also access the default group by reflection, but its name is always null, so it doesn't make sense to auto-localize it
                        LocalizeStringProperties(group, group.Name ?? String.Empty, context);
                    break;

#if !NETCOREAPP3_1_OR_GREATER
                case ToolBar toolBar:
                    foreach (ToolBarButton item in toolBar.Buttons)
                    {
                        LocalizeStringProperties(item, item.Name ?? String.Empty, context);
                        if (item.DropDownMenu != null)
                            ApplyMenuResources(item.DropDownMenu.MenuItems, context);
                    }
                    break;

                case DataGrid dataGrid:
                    foreach (DataGridTableStyle tableStyle in dataGrid.TableStyles)
                    {
                        foreach (DataGridColumnStyle item in tableStyle.GridColumnStyles)
                            LocalizeStringProperties(item, item.MappingName, context);
                    }
                    break;
#endif

                default:
                    foreach (Control child in control.Controls)
                    {
                        var childContext = context;
                        if (child is BaseUserControl baseUserControl && (baseUserControl.DynamicStringLocalization == DynamicStringLocalization.LocalScope
                            || baseUserControl.DynamicStringLocalization is DynamicStringLocalization.AssemblyScope && context.LocalizationScope != DynamicStringLocalization.AssemblyScope))
                        {
                            childContext = new LocalizationContext(child, context.LanguageHint);
                        }

                        ApplyStringResources(child, childContext);
                    }

                    break;
            }
        }

        /// <summary>
        /// Localizes the localizable string properties of the specified <paramref name="target"/> object,
        /// using the specified <paramref name="name"/> as a prefix for the resource keys.
        /// </summary>
        /// <param name="target">The target object whose localizable string properties should be localized.</param>
        /// <param name="name">The name of the target object. Used as a prefix for the resource keys.</param>
        /// <param name="context">A localization context to use for the operation. If <see langword="null"/>, a context is automatically created based on the target object. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        public static void LocalizeStringProperties(object target, string name, LocalizationContext? context = null)
        {
            // Unlike ComponentResourceManager we go by localizable properties instead of resource sets.
            // If the resource is retrieved from a scoped ResourceManager, OnLocalizationRequested retrieves the corresponding resource set on demand.
            // But if LocalizationRequested is handled, it's not even guaranteed that the resource is retrieved from a resource set.
            PropertyAccessor[]? properties = LocalizableStringPropertiesCache[target.GetType()];
            if (properties == null)
                return;

            context ??= new LocalizationContext(target);
            if (context.LocalizationScope == DynamicStringLocalization.Custom && localizationRequestedHandler is null)
                return; // shortcut: no localization would be applied

            bool hasToolTipText = false;
            var args = new LocalizationRequestedEventArgs(context, target);
            foreach (PropertyAccessor property in properties)
            {
                string propertyName = property.MemberInfo.Name;
                if (propertyName == toolTipTextPropertyName)
                    hasToolTipText = true;
                args.Reset(name + "." + propertyName);
                OnLocalizationRequested(args);

                if (args.Value == null)
                    continue;

                property.Set(target, args.Value);
            }

            // applying tool tip
            if (!hasToolTipText && context.ToolTip != null && target is IToolTipTargetProvider or Control)
            {
                args.Reset(name + "." + toolTipTextPropertyName);
                OnLocalizationRequested(args);
                Control? control = (target as IToolTipTargetProvider)?.GetToolTipTarget() ?? (Control?)target;

                if (args.Value != null && control != null)
                    context.ToolTip.SetToolTip(control, args.Value);
            }
        }

        /// <summary>
        /// Gets a localized string for the specified <paramref name="key"/> using the specified <paramref name="context"/>.
        /// It invokes the <see cref="LocalizationRequested"/> event to retrieve the string resource. If the event is not handled,
        /// and a resource set is available for the specified context, it retrieves the string from that resource set.
        /// </summary>
        /// <param name="key">The key of the requested string resource.</param>
        /// <param name="context">The localization context to use for the operation. If <see langword="null"/>, no context is used.</param>
        /// <returns>The localized string for the specified <paramref name="key"/> if found; otherwise, <see langword="null"/>.</returns>
        public static string? GetString(string key, LocalizationContext? context)
        {
            var args = new LocalizationRequestedEventArgs(context, key);
            OnLocalizationRequested(args);
            return args.Value;
        }

        /// <summary>
        /// Gets a localized string for the specified <paramref name="key"/> using the specified <paramref name="context"/> and formatting arguments.
        /// It invokes the <see cref="LocalizationRequested"/> event to retrieve the string resource format. If the event is not handled,
        /// and a resource set is available for the specified context, it retrieves the string from that resource set.
        /// </summary>
        /// <param name="key">The key of the requested string resource. When <paramref name="args"/> has values, the key is expected to be a format string.</param>
        /// <param name="context">The localization context to use for the operation. If <see langword="null"/>, no context is used.</param>
        /// <param name="args">The formatting arguments to be applied to the localized string format.</param>
        /// <returns>The localized and formatted string for the specified <paramref name="key"/> if found; otherwise, <see langword="null"/>.</returns>
        public static string? GetString(string key, LocalizationContext? context, params object?[]? args)
        {
            #region Local Methods

            static string SafeFormat(string format, object?[] args)
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

            #endregion

            string? format = GetString(key, context);
            if (format == null)
                return null;
            if (args == null)
                return format;

            try
            {
                return SafeFormat(format, args);
            }
            catch (FormatException)
            {
                return Res.LocalizationInvalidResource(key, args.Length, format);
            }
        }

        /// <summary>
        /// Gets a resource set for the specified <paramref name="context"/> if available. The result can be freely edited.
        /// </summary>
        /// <param name="context">The localization context to use for the operation. To retrieve a resource set,
        /// the <see cref="LocalizationContext.LocalizationScope"/> must <see cref="DynamicStringLocalization.LocalScope"/> or <see cref="DynamicStringLocalization.AssemblyScope"/>.</param>
        /// <param name="culture">The culture for which the resource set is requested. If <see langword="null"/>, the <see cref="LocalizationContext.LanguageHint"/> is used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IExpandoResourceSet"/> instance if a resource set is available for the specified context; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
        public static IExpandoResourceSet? GetResourceSet(LocalizationContext context, CultureInfo? culture = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), PublicResources.ArgumentNull);

            if (context.LocalizationScope is not (DynamicStringLocalization.LocalScope or DynamicStringLocalization.AssemblyScope))
                return null;

            return ResourceManagersCache[context.CacheKey].GetExpandoResourceSet(culture ?? context.LanguageHint, ResourceSetRetrieval.CreateIfNotExists, true);
        }

        /// <summary>
        /// Saves all pending scoped resources to the corresponding resource files.
        /// Can be useful after calling <see cref="GetResourceSet">GetResourceSet</see> if the result was edited.
        /// </summary>
        /// <remarks>
        /// <note>This method affects resources managed by the <see cref="LocalizationHelper"/> class.
        /// This includes resources of <see cref="BaseForm"/> and <see cref="BaseUserControl"/> instances when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is <see cref="DynamicStringLocalization.LocalScope"/> or <see cref="DynamicStringLocalization.AssemblyScope"/>.
        /// If you use <see cref="DynamicResourceManager"/> managers directly whose <see cref="DynamicResourceManager.UseLanguageSettings"/>
        /// property is <see langword="true"/>, use the <see cref="LanguageSettings.SavePendingResources">LanguageSettings.SavePendingResources</see> method instead.</note>
        /// </remarks>
        public static void SavePendingScopedResources()
        {
            LockingDictionary<string, DynamicResourceManager>? cache = resourceManagersCache;
            if (cache == null)
                return;

            cache.Lock();
            try
            {
                foreach (DynamicResourceManager resourceManager in cache.Values)
                {
                    if (resourceManager.IsDisposed)
                        return;
                    resourceManager.SaveAllResources(false, resourceManager.CompatibleFormat);
                }
            }
            finally
            {
                cache.Unlock();
            }
        }

        /// <summary>
        /// Releases the loaded resource sets of all scoped resources. Resource sets will be reloaded on the next request.
        /// Can be useful after calling <see cref="GetResourceSet">GetResourceSet</see> if the result was edited and the changes should be discarded.
        /// </summary>
        /// <remarks>
        /// <para>This method may have no effect, if an auto save operation occurred since the last <see cref="GetResourceSet">GetResourceSet</see> call.
        /// Auto save occurs when <see cref="LanguageSettings.DisplayLanguage">LanguageSettings.DisplayLanguage</see> changes or when
        /// localization is requested for many different <see cref="LocalizationContext"/>s in a row, and the resources of some contexts are dropped from the internal cache.</para>
        /// <note>This method affects resources managed by the <see cref="LocalizationHelper"/> class.
        /// This includes resources of <see cref="BaseForm"/> and <see cref="BaseUserControl"/> instances when their <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is <see cref="DynamicStringLocalization.LocalScope"/> or <see cref="DynamicStringLocalization.AssemblyScope"/>.
        /// If you use <see cref="DynamicResourceManager"/> managers directly whose <see cref="DynamicResourceManager.UseLanguageSettings"/>
        /// property is <see langword="true"/>, use the <see cref="LanguageSettings.ReleaseAllResources">LanguageSettings.ReleaseAllResources</see> method instead.</note>
        /// </remarks>
        public static void ReleaseAllScopedResources()
        {
            LockingDictionary<string, DynamicResourceManager>? cache = resourceManagersCache;
            if (cache == null)
                return;

            cache.Lock();
            try
            {
                foreach (DynamicResourceManager resourceManager in cache.Values)
                {
                    if (resourceManager.IsDisposed)
                        return;
                    resourceManager.ReleaseAllResources();
                }
            }
            finally
            {
                cache.Unlock();
            }
        }

        /// <summary>
        /// Gets the base name of the resource file for the specified <paramref name="type"/>.
        /// That is, the name of the resource file without the culture name and the .resx extension.
        /// </summary>
        /// <param name="type">The type for which the resource base name is requested.
        /// This is usually a form or user control type, whose <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.LocalScope"/>.</param>
        /// <returns>The base name of the resource file for the specified <paramref name="type"/>.</returns>
        /// <remarks>
        /// <para>You can use this method to retrieve the base name of resource files associated with a <see cref="LocalizationContext"/>
        /// with <see cref="DynamicStringLocalization.LocalScope"/>.
        /// </para>
        /// </remarks>
        public static string GetResourceBaseName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), PublicResources.ArgumentNull);
            return type.FullName + resourcesPostfix;
        }

        /// <summary>
        /// Gets the base name of the resource file for the specified <paramref name="assembly"/>.
        /// That is, the name of the resource file without the culture name and the .resx extension.
        /// </summary>
        /// <param name="assembly">The assembly for which the resource base name is requested.
        /// This is usually the assembly of a project containing forms or user controls, whose <see cref="BaseForm.DynamicStringLocalization">DynamicStringLocalization</see>
        /// property is set to <see cref="DynamicStringLocalization.AssemblyScope"/>.</param>
        /// <returns>The base name of the resource file for the specified <paramref name="assembly"/>.</returns>
        /// <remarks>
        /// <para>You can use this method to retrieve the base name of resource files associated with a <see cref="LocalizationContext"/>
        /// with <see cref="DynamicStringLocalization.AssemblyScope"/>.
        /// </para>
        /// </remarks>
        public static string GetResourceBaseName(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), PublicResources.ArgumentNull);
            return assembly.GetName().Name + resourcesPostfix;
        }

        #endregion

        #region Private Methods

        private static DynamicResourceManager CreateResourceManager(string key)
        {
            string baseName;
            Assembly assembly;

            if (key[0] == '!') // assembly scope
            {
                assembly = Reflector.ResolveAssembly(key.Substring(1), ResolveAssemblyOptions.ThrowError)!;
                baseName = assembly.GetName().Name + resourcesPostfix;
            }
            else // local scope
            {
                Type type = Reflector.ResolveType(key, ResolveTypeOptions.ThrowError)!;
                assembly = type.Assembly;
                baseName = type.FullName + resourcesPostfix;
            }

            return new DynamicResourceManager(baseName, assembly)
            {
                SafeMode = true,
                ThrowException = false,
                CompatibleFormat = true,
                AutoSave = AutoSaveOptions.LanguageChange | AutoSaveOptions.DomainUnload | AutoSaveOptions.Dispose
            };
        }

        private static PropertyAccessor[]? GetLocalizableStringProperties(Type type)
        {
            // Getting localizable and browsable string properties only.
            var result = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string)
                    && (Attribute.GetCustomAttribute(p, typeof(LocalizableAttribute)) is LocalizableAttribute la && la.IsLocalizable
                        && Attribute.GetCustomAttribute(p, typeof(BrowsableAttribute)) is null or BrowsableAttribute { Browsable: true }
                        || p.DeclaringType == typeof(ListViewGroup) && p.Name == nameof(ListViewGroup.Header)
                    )).ToArray();
            return result.Length == 0 ? null : result.Select(PropertyAccessor.GetAccessor).ToArray();
        }

        private static void OnLocalizationRequested(LocalizationRequestedEventArgs e)
        {
            localizationRequestedHandler?.Invoke(null, e);
            if (e.Value != null)
                return;

            LocalizationContext? context = e.Context?.LocalizationScope is DynamicStringLocalization.LocalScope or DynamicStringLocalization.AssemblyScope ? e.Context : null;
            if (context == null)
                return;

            e.Value = ResourceManagersCache[context.CacheKey].GetString(e.Key, e.Context?.LanguageHint);
        }

        #endregion

        #endregion
    }
}
