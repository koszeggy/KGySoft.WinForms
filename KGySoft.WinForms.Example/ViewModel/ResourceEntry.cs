#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ResourceEntry.cs
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

#endregion

namespace KGySoft.WinForms.Example.ViewModel
{
    public class ResourceEntry : ObservableObjectBase
    {
        #region Properties

        public string Key { get; }
        public string OriginalText { get; }
        public string? TranslatedText { get => Get<string>(); set => Set(value); }

        #endregion

        #region Constructors

        internal ResourceEntry(string key, string originalText, string? translatedText)
        {
            Key = key;
            OriginalText = originalText;
            TranslatedText = translatedText;
            SetModified(false);
        }

        #endregion
    }
}