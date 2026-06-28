#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ExpandablePropertiesConverter.cs
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
using System.ComponentModel;
using System.Windows.Forms;

#endregion

#region Suppressions

#if !NET6_0_OR_GREATER
// ReSharper disable AssignNullToNotNullAttribute - inconsistent annotations on different platforms
// ReSharper disable PossibleNullReferenceException - inconsistent annotations on different platforms
#endif

#endregion

namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Provides a type converter that allows expanding the properties recursively in a <see cref="PropertyGrid"/>.
    /// This class is similar to <see cref="ExpandableObjectConverter">System.ComponentModel.ExpandableObjectConverter</see>, but that one allows expanding the top-level properties only.
    /// </summary>
    /// <remarks>
    /// <para>This can be used to decorate properties by <see cref="TypeConverterAttribute"/>.
    /// Alternatively, you can use the <see cref="RecursivelyEditableTypeDescriptor"/> to wrap any object to make its properties expandable in a <see cref="PropertyGrid"/>.</para>
    /// <note type="tip">To load/save object graphs that are recursively editable in a <see cref="PropertyGrid"/>, you can use
    /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Serialization_Xml_XmlSerializer.htm" target="_blank">XmlSerializer</a> class
    /// from the KGy SOFT Core Libraries package.</note>
    /// </remarks>
    /// <example>
    /// To make a property recursively expandable in a <see cref="PropertyGrid"/>, use the <see cref="TypeConverterAttribute"/>:
    /// <code lang="C#"><![CDATA[
    /// [TypeConverter(typeof(ExpandablePropertiesConverter))]
    /// public MyComplexType MyComplexProperty { get; set; }
    /// ]]></code>
    /// </example>
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
            public override TypeConverter Converter => converter ??= new ExpandablePropertiesConverter();

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

        #region Methods

        #region Static Methods

        internal static PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection properties)
        {
            var result = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor? property in properties)
            {
                if (property == null)
                    continue;
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

        /// <inheritdoc />
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
            => FilterProperties(TypeDescriptor.GetProperties(value, attributes, true));

        /// <inheritdoc />
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context)
            => context?.PropertyDescriptor?.PropertyType.GetProperties().Length > 0;

        #endregion

        #endregion
    }
}
