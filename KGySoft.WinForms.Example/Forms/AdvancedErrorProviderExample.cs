#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedErrorProviderExample.cs
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

using KGySoft.ComponentModel;
using KGySoft.Drawing;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Example.ViewModel;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    public partial class AdvancedErrorProviderExample : BaseForm
    {
        #region Fields

        private readonly AdvancedErrorProviderExampleViewModel viewModel = new();

        #endregion

        #region Constructors

        public AdvancedErrorProviderExample()
        {
            InitializeComponent();

            errorProvider.Icon = Icons.SystemError;
            warningProvider.Icon = Icons.SystemWarning;
            infoProvider.Icon = Icons.SystemInformation;

            ResetBinding();
        }

        #endregion

        #region Methods

        #region Private Methods

        private void ResetBinding()
        {
            bindingSource.SuspendBinding();
            bindingSource.DataSource = viewModel.ValidatingExampleCollection;
            bindingSource.ResumeBinding();
        }

        #endregion

        #region Event handlers

        private void warningProvider_SetMessage(object sender, SetMessageEventArgs e)
        {
            ValidationResultsCollection? propertyValidations = (e.Current as IValidatingObject)?.ValidationResults[e.PropertyName];
            e.Message = propertyValidations is { HasErrors: false, HasWarnings: true }
                ? propertyValidations.Warnings.Message
                : null;
            e.Cancel = propertyValidations is null; // can occur if the handler is setting the binding error and the AdvancedBindingProvider.ShowBindingErrors is true
        }

        private void infoProvider_SetMessage(object sender, SetMessageEventArgs e)
        {
            ValidationResultsCollection? propertyValidations = (e.Current as IValidatingObject)?.ValidationResults[e.PropertyName];
            e.Message = propertyValidations is { HasErrors: false, HasWarnings: false, HasInfos: true }
                ? propertyValidations.Infos.Message
                : null;
            e.Cancel = propertyValidations is null; // can occur if the handler is setting the binding error and the AdvancedBindingProvider.ShowBindingErrors is true
        }

        #endregion

        #endregion
    }
}
