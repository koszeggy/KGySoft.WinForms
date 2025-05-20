#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RecursivelyEditableTypeDescriptor.cs
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
    /// Represents a custom type descriptor that allows editing the properties recursively.
    /// </summary>
    public class RecursivelyEditableTypeDescriptor : ICustomTypeDescriptor
    {
        #region Properties

        public object Object { get; }

        #endregion

        #region Constructors

        public RecursivelyEditableTypeDescriptor(object obj)
        {
            Object = obj ?? throw new ArgumentNullException(nameof(obj));
        }

        #endregion

        #region Methods

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(Object, true);
        public string? GetClassName() => TypeDescriptor.GetClassName(Object, true);
        public string? GetComponentName() => TypeDescriptor.GetComponentName(Object, true);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(Object, true);
        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(Object, true);
        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(Object, true);
        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(Object, editorBaseType, true);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(Object, true);
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(Object, attributes, true);
        public PropertyDescriptorCollection GetProperties() => ExpandablePropertiesConverter.FilterProperties(TypeDescriptor.GetProperties(Object, true), Object.GetType());
        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => ExpandablePropertiesConverter.FilterProperties(TypeDescriptor.GetProperties(Object, attributes, true), Object.GetType());
        public object GetPropertyOwner(PropertyDescriptor? pd) => Object;

        #endregion
    }
}
