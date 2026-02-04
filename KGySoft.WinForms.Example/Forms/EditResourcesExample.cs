#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EditResourcesExample.cs
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
using System.Drawing;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.WinForms.Example.ViewModel;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class EditResourcesExample : DialogBaseForm
    {
        #region Constants

        private const string resTextFormat = "EditResources.Text.Format";
        private const string resSaveErrorFormat = "SaveError.Format";

        #endregion

        #region Fields

        private readonly EditResourcesViewModel viewModel = null!;
        private readonly LocalizationContext customLocalizationContext; // needed only if we want to retrieve custom resource entries from the form's string resources

        private bool isRtlChanging;
        private Point location;

        #endregion

        #region Constructors

        #region Public Constructors

        public EditResourcesExample()
        {
            InitializeComponent();
            ApplyRightToLeft();
            cmbResourceFiles.ValueMember = "Key";
            cmbResourceFiles.DisplayMember = "Value";
            customLocalizationContext = new LocalizationContext(this);
        }

        #endregion

        #region Internal Constructors

        internal EditResourcesExample(EditResourcesViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
        }

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            bool isLoaded = IsLoaded;
            base.OnLoad(e);
            if (isLoaded)
            {
                if (!isRtlChanging)
                    return;

                isRtlChanging = false;
                Location = location;
                return;
            }

            ApplyViewModel();
        }

        protected override void ApplyStringResources()
        {
            ApplyRightToLeft();

            // This auto-applies the localized text properties of the controls
            base.ApplyStringResources();

            // This is how we can apply custom resources with formatting: textFormatKey gets a string value with two placeholders for the language names.
            Text = LocalizationHelper.GetString(null, resTextFormat, customLocalizationContext, viewModel.EditedLanguage.NativeName, viewModel.EditedLanguage.EnglishName);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Changing RightToLeft in .NET Framework and .NET Core 3.0-.NET 6.0 causes the dialog close. We let it happen because the parent may also change,
            // and if we cancel the closing here, then the dialog may turn a non-modal form. Reopening is handled in the parent form.
            if (isRtlChanging)
            {
                if (DialogResult != DialogResult.Retry)
                    isRtlChanging = false;
                else
                    location = Location;
            }

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void ApplyViewModel()
        {
            viewModel.ApplyLocalizationCallback = ApplyStringResources;

            // Command bindings
            CommandBindings.Add(viewModel.ApplyResourcesCommand, viewModel.ApplyResourcesCommandState)
                .AddSource(ApplyButton, nameof(ApplyButton.Click));

            ICommandBinding saveBinding = CommandBindings.Add(viewModel.SaveResourcesCommand)
                    .AddSource(OKButton, nameof(OKButton.Click));

            saveBinding.Error += (_, e) =>
            {
                // If the save operation fails, we show the error, also with localization, and prevent closing the dialog.
                Dialogs.AutoRightToLeftLayout = true; // would be enough to set only once, in the application startup
                Dialogs.ErrorMessage(LocalizationHelper.GetString(null, resSaveErrorFormat, customLocalizationContext, e.Error.Message)!);
                e.Handled = true;
                DialogResult = DialogResult.None; // prevent closing the dialog
            };

            CommandBindings.Add(viewModel.CancelEditCommand)
                .AddSource(CancelButton, nameof(CancelButton.Click));

            // Property bindings
            // VM.ResourceFiles -> cmbResourceFiles.DataSource
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ResourceFiles), nameof(cmbResourceFiles.DataSource), cmbResourceFiles);

            // VM.SelectedResource <-> cmbResourceFiles.SelectedValue
            CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.SelectedResource), cmbResourceFiles, nameof(cmbResourceFiles.SelectedValue));

            // VM.ResourceEntries -> bindingSource.DataSource
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ResourceEntries), nameof(bindingSource.DataSource), bindingSource);
        }

        private void ApplyRightToLeft()
        {
            var rtl = LanguageSettings.DisplayLanguage.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            if (RightToLeft == rtl)
                return;

            if (!OSHelper.IsMono && IsHandleCreated)
                isRtlChanging = true;

            RightToLeft = rtl;

            // Modal forms on Windows: when changing RTL, the DialogResult is set to Cancel in older framework targets, causing the dialog to close.
            // To make it work the same way on all platforms, we set it to Retry, signaling the check in OnClosing that the dialog should be reopened.
            // Without the reopening, the dialog would turn into a non-modal form, allowing the user to interact with the caller form.
            if (Modal && !OSHelper.IsMono && OSHelper.IsWindows)
                DialogResult = DialogResult.Retry;
        }

        #endregion

        #endregion
    }
}
