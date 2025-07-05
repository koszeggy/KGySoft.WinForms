#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: frmLocalizationExample.cs
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
using System.Resources;
using System.Windows.Forms;

using KGySoft.WinForms.Example.ViewModel;

#endregion

// Indicates that the assembly uses English as the default language for resources.
// If the selected display language is English, the resources will be loaded from the invariant resources, without generating new ones.
// Normally you should place this attribute somewhere in AssemblyInfo.cs or in the main file of your project.
[assembly: NeutralResourcesLanguage("en")]

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmLocalizationExample : ControlsTestBaseForm
    {
        #region Fields

        private readonly LocalizationExampleViewModel viewModel = new();

        #endregion

        #region Constructors

        #region Static Constructor

        static frmLocalizationExample()
        {
            // Normally initializing the display language is somewhere in the main file of your project, preferably from some configuration file.
            LanguageSettings.DisplayLanguage = LocalizationExampleViewModel.GetClosestNeutralCulture(LanguageSettings.DisplayLanguage);
        }

        #endregion

        #region Instance Constructors

        public frmLocalizationExample()
        {
            InitializeComponent();
            ApplyRightToLeft();
            cmbLanguages.ComboBox.ValueMember = "Key";
            cmbLanguages.ComboBox.DisplayMember = "Value";
            LanguageSettings.DisplayLanguageChanged += LanguageSettings_DisplayLanguageChanged;
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
                return;
            ApplyViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            LanguageSettings.DisplayLanguageChanged -= LanguageSettings_DisplayLanguageChanged;
            if (disposing)
            {
                components?.Dispose();
                viewModel.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void ApplyRightToLeft() => RightToLeft = LanguageSettings.DisplayLanguage.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;

        private void ApplyViewModel()
        {
            viewModel.ApplyLocalizationCallback = ApplyStringResources;

            // Command bindings
            CommandBindings.Add(viewModel.ApplyCommand, viewModel.ApplyCommandState)
                .AddSource(btnApply, nameof(btnApply.Click));

            CommandBindings.Add(OnEditResourcesCommand)
                .AddSource(btnEdit, nameof(btnEdit.Click));

            // Property bindings
            // VM.ExistingLanguagesOnly <-> chbFilter.Checked
            CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.ExistingLanguagesOnly), chbFilter, nameof(chbFilter.Checked));

            // VM.Languages -> cmbLanguages.ComboBox.DataSource
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.Languages), nameof(cmbLanguages.ComboBox.DataSource), cmbLanguages.ComboBox);

            // VM.CurrentLanguage <-> cmbLanguages.ComboBox.SelectedValue
            CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.SelectedLanguage), cmbLanguages.ComboBox, nameof(cmbLanguages.ComboBox.SelectedValue));

            // chbCustom.Checked -> VM.UseCustomLocalization
            CommandBindings.AddPropertyBinding(chbCustom, nameof(chbCustom.Checked), nameof(viewModel.UseCustomLocalization), viewModel);
        }

        private void OnEditResourcesCommand()
        {
            using var vmEdit = new EditResourcesViewModel(viewModel);
            using var frmEditResources = new frmEditResources(vmEdit);
            do
            {
                // Workaround for RTL changing: without this, the dialog may turn non-modal when its RightToLeft property changes
                frmEditResources.ShowDialog(this);
            } while (frmEditResources.DialogResult == DialogResult.Retry);
            if (viewModel.SelectedLanguage.Equals(LanguageSettings.DisplayLanguage) && vmEdit.IsDirty)
                ApplyStringResources();
        }

        #endregion

        #region Event handlers

        private void LanguageSettings_DisplayLanguageChanged(object? sender, EventArgs e)
        {
            ApplyRightToLeft();
            ApplyStringResources();
        }

        private void localizableControlDemo_DynamicStringLocalizationChanged(object? sender, EventArgs e) => ApplyStringResources();

        #endregion

        #endregion
    }
}