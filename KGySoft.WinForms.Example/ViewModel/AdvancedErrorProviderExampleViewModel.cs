#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedErrorProviderExampleViewModel.cs
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

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.WinForms.Example.ViewModel
{
    internal class AdvancedErrorProviderExampleViewModel : ObservableObjectBase
    {
        #region Properties

        internal SortableBindingList<ValidatingObjectExample> ValidatingExampleCollection { get; } = new()
        {
            new ValidatingObjectExample
            {
                UserName = "Young Padawan",
                DateOfBirth = DateTime.Today.AddYears(-16),
                AccountBalance = 100m,
                Password = "password1"
            },
            new ValidatingObjectExample
            {
                UserName = "Veteran Joe",
                DateOfBirth = DateTime.Today.AddYears(-65),
                AccountBalance = 123_456m,
                Password = ThreadSafeRandom.Instance.NextString(16)
            },
            new ValidatingObjectExample
            {
                UserName = "spamBot113",
                DateOfBirth = ThreadSafeRandom.Instance.NextDate(),
                AccountBalance = ThreadSafeRandom.Instance.NextDecimal(Decimal.MinValue, Decimal.MaxValue),
                Password = ThreadSafeRandom.Instance.NextString(10, 16, StringCreation.AnyValidChars)
            }
        };

        #endregion
    }
}