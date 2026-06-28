#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RecursivelyEditableTypeDescriptor.cs
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

namespace KGySoft.ComponentModel
{
    /// <summary>
    /// Represents a custom type descriptor that can wrap any object to allow editing its properties recursively in a <see cref="PropertyGrid"/>.
    /// </summary>
    /// <remarks>
    /// <para>This class can be used as a wrapper of any object to make its properties recursively expandable at run time in a <see cref="PropertyGrid"/>:
    /// <code lang="C#">propertyGrid1.SelectedObject = new RecursivelyEditableTypeDescriptor(myObjectToEdit);</code></para>
    /// <para>Alternatively, you can use the <see cref="RecursivelyEditableTypeDescriptor"/> to mark individual properties expandable at design time.</para>
    /// <note type="tip">To load/save object graphs that are recursively editable in a <see cref="PropertyGrid"/>, you can use
    /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Serialization_Xml_XmlSerializer.htm" target="_blank">XmlSerializer</a> class
    /// from the KGy SOFT Core Libraries package.</note>
    /// </remarks>
    public class RecursivelyEditableTypeDescriptor : ICustomTypeDescriptor
    {
        #region Properties

        /// <summary>
        /// Gets the underlying object associated with this instance.
        /// </summary>
        public object Object { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecursivelyEditableTypeDescriptor"/> class for the specified object.
        /// </summary>
        /// <param name="obj">The object, whose properties should be recursively expandable and editable in a <see cref="PropertyGrid"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
        public RecursivelyEditableTypeDescriptor(object obj)
        {
            Object = obj ?? throw new ArgumentNullException(nameof(obj), PublicResources.ArgumentNull);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(Object, true);

        /// <inheritdoc />
        public string? GetClassName() => TypeDescriptor.GetClassName(Object, true);

        /// <inheritdoc />
        public string? GetComponentName() => TypeDescriptor.GetComponentName(Object, true);

        /// <inheritdoc />
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(Object, true);

        /// <inheritdoc />
        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(Object, true);

        /// <inheritdoc />
        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(Object, true);

        /// <inheritdoc />
        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(Object, editorBaseType, true);

        /// <inheritdoc />
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(Object, true);

        /// <inheritdoc />
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(Object, attributes, true);

        /// <inheritdoc />
        public PropertyDescriptorCollection GetProperties() => ExpandablePropertiesConverter.FilterProperties(TypeDescriptor.GetProperties(Object, true));

        /// <inheritdoc />
        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => ExpandablePropertiesConverter.FilterProperties(TypeDescriptor.GetProperties(Object, attributes, true));

        /// <inheritdoc />
        public object GetPropertyOwner(PropertyDescriptor? pd) => Object;

        #endregion
    }
}
