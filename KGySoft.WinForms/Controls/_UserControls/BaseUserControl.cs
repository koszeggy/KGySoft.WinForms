#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BaseUserControl.cs
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
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// A base class for user controls that provides some additional functionality.
    /// </summary>
    /// <remarks>
    /// The <see cref="BaseUserControl"/> class provides the following additional features:
    /// <list type="bullet">
    /// <item>Removes all event subscriptions when the user control is disposed. To do that for the events of derived controls as well,
    /// use the <see cref="Component.Events"/> property in your derived event <see langword="add"/>/<see langword="remove"/> accessors.</item>
    /// <item><see cref="CommandBindings"/> property. See the <a href="https://kgysoft.net/corelibraries#command-binding" target="_blank">online documentation</a> for details.</item>
    /// <item>An <see cref="IsDesignMode"/> property that works even during initialization, when <see cref="Component.DesignMode"/> would return <see langword="false"/>.</item>
    /// <item><see cref="InvokeOnUIThread">InvokeOnUIThread</see> method.</item>
    /// </list>
    /// </remarks>
    public class BaseUserControl : UserControl
    {
        #region Fields

        private readonly CommandBindingsCollection commandBindings = new WinFormsCommandBindingsCollection();
        private readonly InvokeMarshaller invoker;

        private bool isLoaded;
        private DynamicStringLocalization localizationMode;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="DynamicStringLocalization"/> property changes.
        /// </summary>
        [Category("BaseUserControl")]
        [Description("Occurs when the DynamicStringLocalization property changes.")]
        public event EventHandler? DynamicStringLocalizationChanged
        {
            add => Events.AddHandler(nameof(DynamicStringLocalization), value);
            remove => Events.RemoveHandler(nameof(DynamicStringLocalization), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the command bindings of this form. The <see cref="O:KGySoft.ComponentModel.CommandBindingsCollection.Add">Add</see> methods also add
        /// the <see cref="PropertyCommandStateUpdater"/> to the created bindings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        /// <summary>
        /// Gets or sets the dynamic string localization strategy of the user control. It allows using potentially auto-generated string resources from .resx files.
        /// <br/>See the <strong>Remarks</strong> section for the <see cref="BaseForm.DynamicStringLocalization"/> property for details.
        /// </summary>
        [Category("BaseUserControl")]
        [DefaultValue(DynamicStringLocalization.Disabled)]
        [Description("Specifies the dynamic string localization strategy of the control. LocalScope and AssemblyScope allow using potentially auto-generated .resx files "
            + "and ensure that localization is automatically re-applied when LanguageSettings.DisplayLanguage is changed. They need an existing invariant resource set to work. "
            + "The Custom setting allows handling the LocalizationHelper.LocalizationRequested event to provide localization for the controls programmatically.")]
        public DynamicStringLocalization DynamicStringLocalization
        {
            get => localizationMode;
            set
            {
                if (localizationMode == value)
                    return;
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                localizationMode = value;
                OnDynamicStringLocalizationChanged(EventArgs.Empty);
                LanguageSettings.DisplayLanguageChanged -= LanguageSettings_DisplayLanguageChanged;
                if (IsDesignMode || value == DynamicStringLocalization.Disabled)
                    return;

                LanguageSettings.DisplayLanguageChanged += LanguageSettings_DisplayLanguageChanged;
                if (isLoaded)
                    ApplyStringResources();
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets whether the user control is in design mode. Unlike the <see cref="Component.DesignMode"/> property,
        /// this property works even during initialization.
        /// </summary>
        [Browsable(false)]
        protected bool IsDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        /// <summary>
        /// Gets whether the user control has already been loaded. This property is <see langword="true"/> after the <see cref="UserControl.Load"/> event is raised for the first time,
        /// and remains <see langword="true"/> even if the  handle is recreated (e.g. because <see cref="Control.RightToLeft"/> changes).
        /// Can be useful if we overload the <see cref="UserControl.OnLoad"/> method and want to avoid executing some initialization more than once.
        /// </summary>
        [Browsable(false)]
        protected bool IsLoaded => isLoaded;

        #endregion

        #region Private Properties

        private bool HasLocalizedParent
        {
            get
            {
                Control? parent = Parent;
                while (parent != null)
                {
                    if (parent is BaseUserControl { DynamicStringLocalization: not DynamicStringLocalization.Disabled } or BaseForm { DynamicStringLocalization: not DynamicStringLocalization.Disabled })
                        return true;
                    parent = parent.Parent;
                }

                return false;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUserControl"/> class.
        /// </summary>
        protected BaseUserControl()
        {
            invoker = new InvokeMarshaller(this);
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            bool loaded = isLoaded;
            isLoaded = true;
            base.OnLoad(e);

            // isLoaded can be true if handle was recreated
            if (!loaded)
                ApplyResources();
        }

        /// <summary>
        /// Raises the <see cref="DynamicStringLocalizationChanged"/> event.
        /// </summary>
        /// <param name="e">Contains the arguments of the event.</param>
        protected virtual void OnDynamicStringLocalizationChanged(EventArgs e)
            => Events.GetHandler<EventHandler>(nameof(DynamicStringLocalization))?.Invoke(this, e);

        /// <summary>
        /// Applies the resources of the user control. The default implementation just calls the <see cref="ApplyStringResources">ApplyStringResources</see> method.
        /// Called when the user control is loaded for the first time. In a derived control, this method can be overridden to apply additional (non-string) resources,
        /// and it can be called whenever the resources should be re-applied, e.g. when the display language changes.
        /// </summary>
        protected virtual void ApplyResources()
        {
            if (!IsDesignMode)
                ApplyStringResources();
        }

        /// <summary>
        /// Applies the string resources of the user control. If the <see cref="DynamicStringLocalization"/> property is not set to <see cref="DynamicStringLocalization.Disabled"/>,
        /// and this user control has no parent form or user control that has a non-disabled <see cref="DynamicStringLocalization"/> mode,
        /// the default implementation just calls the <see cref="LocalizationHelper.ApplyStringResources">LocalizationHelper.ApplyStringResources</see> method.
        /// In a derived control, this method can be overridden to apply a custom string localization, and it can be called whenever the form's string resources
        /// should be re-applied, e.g. when the display language changes.
        /// </summary>
        protected virtual void ApplyStringResources()
        {
            if (localizationMode != DynamicStringLocalization.Disabled && !IsDesignMode && !HasLocalizedParent)
                LocalizationHelper.ApplyStringResources(this);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            LanguageSettings.DisplayLanguageChanged -= LanguageSettings_DisplayLanguageChanged;
            if (disposing)
            {
                commandBindings.Dispose();
                Events.Dispose();
            }
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> on the thread that the control was created on.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <remarks>
        /// <para>This method is similar as using <see cref="Control.InvokeRequired"/> and <see cref="Control.Invoke(Delegate)"/> together,
        /// but it works even when the handle is not created yet, in which case <see cref="Control.InvokeRequired"/> returns <see langword="false"/>.</para>
        /// <para>The callback is invoked only if <see cref="Control.Disposing"/> and <see cref="Control.IsDisposed"/> properties return <see langword="false"/>.</para>
        /// </remarks>
        protected void InvokeOnUIThread(Action callback) => invoker.Invoke(callback);

        #endregion

        #region Event Handlers

        private void LanguageSettings_DisplayLanguageChanged(object? sender, EventArgs e) => ApplyStringResources();

        #endregion

        #endregion
    }
}