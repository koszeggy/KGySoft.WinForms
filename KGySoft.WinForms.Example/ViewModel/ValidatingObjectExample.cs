#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ValidatingObjectExample.cs
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
using System.Linq;

using KGySoft.ComponentModel;

#endregion

namespace KGySoft.WinForms.Example.ViewModel
{
    internal class ValidatingObjectExample : ValidatingObjectBase
    {
        #region Properties

        public string UserName { get => Get(String.Empty); set => Set(value); }
        public DateTime DateOfBirth { get => Get<DateTime>(() => new DateTime(2000, 1, 1)); set => Set(value); }
        public string Password { get => Get(String.Empty); set => Set(value); }
        public decimal AccountBalance { get => Get<decimal>(); set => Set(value); }

        #endregion

        #region Methods

        protected override ValidationResultsCollection DoValidation()
        {
            var result = new ValidationResultsCollection();

            if (String.IsNullOrEmpty(UserName))
                result.AddError(nameof(UserName), "The user name must not be empty");
            else if (UserName.Any(c => c < 32 || c > 127) == true)
                result.AddError(nameof(UserName), "The user name contains invalid characters");

            int years = DateTime.Today.Year - DateOfBirth.Year;
            if (DateTime.Today < DateOfBirth)
                result.AddError(nameof(DateOfBirth), "Invalid date of birth");
            else if (years < 13)
                result.AddError(nameof(DateOfBirth), "User is is too young to create an account");
            else if (years < 18)
                result.AddWarning(nameof(DateOfBirth), $"User is {years} years old in this calendar year, account is restricted");
            else if (years > 99)
                result.AddInfo(nameof(DateOfBirth), $"{years} years old, really? Wow");

            if (String.IsNullOrEmpty(Password))
                result.AddError(nameof(Password), "The password must not be empty");
            else if (Password.Length < 8)
                result.AddError(nameof(Password), "The password is too short");
            else if (Password.Distinct().Count() < 10)
                result.AddWarning(nameof(Password), "Weak password");

            if (!result.HasErrors)
            {
                if (AccountBalance < 0m)
                    result.AddWarning(nameof(AccountBalance), "The balance is negative, the account is restricted.");
                else if (AccountBalance >= 100_000m && !result[nameof(DateOfBirth)].HasWarnings)
                    result.AddInfo(nameof(AccountBalance), "Premium services are enabled.");
            }

            return result;
        }

        #endregion
    }
}
