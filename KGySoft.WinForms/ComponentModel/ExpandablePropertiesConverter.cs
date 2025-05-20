#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ExpandablePropertiesConverter.cs
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

#endregion

// ReSharper disable once CheckNamespace
namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Similar to <see cref="ExpandableObjectConverter"/> but recursively.
    /// </summary>
    public class ExpandablePropertiesConverter : TypeConverter
    {
        #region Nested classes

        #region ExpandablePropertyDescriptor class

        private class ExpandablePropertyDescriptor : PropertyDescriptor
        {
            #region Fields

            private readonly PropertyDescriptor wrappedDescriptor;

            private TypeConverter? converter;

            #endregion

            #region Properties

            public override Type ComponentType => wrappedDescriptor.ComponentType;
            public override bool IsReadOnly => wrappedDescriptor.IsReadOnly;
            public override Type PropertyType => wrappedDescriptor.PropertyType;
            public override TypeConverter Converter => converter ??= new ExpandablePropertiesConverter(wrappedDescriptor.PropertyType);

            #endregion

            #region Constructors

            internal ExpandablePropertyDescriptor(PropertyDescriptor descriptor) : base(descriptor)
            {
                wrappedDescriptor = descriptor;
            }

            #endregion

            #region Methods

            public override bool CanResetValue(object component) => wrappedDescriptor.CanResetValue(component);
            public override object? GetValue(object? component) => wrappedDescriptor.GetValue(component);
            public override void ResetValue(object component) => wrappedDescriptor.ResetValue(component);
            public override void SetValue(object? component, object? value) => wrappedDescriptor.SetValue(component, value);
            public override bool ShouldSerializeValue(object component) => wrappedDescriptor.ShouldSerializeValue(component);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Type type;

        #endregion

        #region Constructors

        public ExpandablePropertiesConverter(Type type) => this.type = type ?? throw new ArgumentNullException(nameof(type));

        #endregion

        #region Methods

        #region Static Methods

        internal static PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection properties, Type type)
        {
            var result = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor property in properties)
            {
                if (!(property.Converter is ReferenceConverter || property.Converter.GetType() == typeof(TypeConverter)))
                {
                    result.Add(property);
                    continue;
                }

                result.Add(new ExpandablePropertyDescriptor(property));
            }

            return result;
        }

        #endregion

        #region Instance Methods

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
            => FilterProperties(TypeDescriptor.GetProperties(value, attributes, true), type);

        public override bool GetPropertiesSupported(ITypeDescriptorContext? context)
            => context?.PropertyDescriptor?.PropertyType.GetProperties().Length > 0;

        #endregion

        #endregion
    }
}
