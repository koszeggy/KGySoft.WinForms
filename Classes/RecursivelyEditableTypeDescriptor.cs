using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Controls.Classes
{
    public class RecursivelyEditableTypeDescriptor : ICustomTypeDescriptor
    {
        #region Nested classes

        #region ExpandablePropertyDescriptor class

        class ExpandablePropertyDescriptor : PropertyDescriptor
        {
            #region Fields

            private readonly PropertyDescriptor wrappedDescriptor;
            private readonly object obj;
            private TypeConverter converter;

            #endregion

            #region Properties

            public override Type ComponentType => wrappedDescriptor.ComponentType;
            public override bool IsReadOnly => wrappedDescriptor.IsReadOnly;
            public override Type PropertyType => obj?.GetType() ?? wrappedDescriptor.PropertyType;
            public override TypeConverter Converter => obj == null ? wrappedDescriptor.Converter : converter ?? (converter = new ExpandablePropertiesConverter(obj));

            #endregion

            #region Constructors

            public ExpandablePropertyDescriptor(PropertyDescriptor descriptor, object obj) : base(descriptor)
            {
                wrappedDescriptor = descriptor;
                this.obj = obj;
            }

            #endregion

            #region Methods

            public override bool CanResetValue(object component) => wrappedDescriptor.CanResetValue(component);
            public override object GetValue(object component) => wrappedDescriptor.GetValue(component);
            public override void ResetValue(object component) => wrappedDescriptor.ResetValue(component);
            public override void SetValue(object component, object value) => wrappedDescriptor.SetValue(component, value);
            public override bool ShouldSerializeValue(object component) => wrappedDescriptor.ShouldSerializeValue(component);

            #endregion
        }

        #endregion

        #region ExpandablePropertiesConverter class

        internal class ExpandablePropertiesConverter : TypeConverter
        {
            private readonly object obj;

            internal ExpandablePropertiesConverter(object obj) => this.obj = obj;

            #region Methods

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
                => context.PropertyDescriptor?.PropertyType.GetProperties().Length > 0;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) 
                => FilterProperties(TypeDescriptor.GetProperties(value, attributes, true), obj);

            #endregion
        }

        #endregion

        #endregion

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

        #region Public Methods

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(Object, true);
        public string GetClassName() => TypeDescriptor.GetClassName(Object, true);
        public string GetComponentName() => TypeDescriptor.GetComponentName(Object, true);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(Object, true);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(Object, true);
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(Object, true);
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(Object, editorBaseType, true);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(Object, true);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(Object, attributes, true);
        public PropertyDescriptorCollection GetProperties() => FilterProperties(TypeDescriptor.GetProperties(Object, true), Object);
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => FilterProperties(TypeDescriptor.GetProperties(Object, attributes, true), Object);
        public object GetPropertyOwner(PropertyDescriptor pd) => Object;

        #endregion

        #region Private Methods

        private static PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection properties, object instance)
        {
            var result = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor property in properties)
            {
                if (!(property.Converter is ReferenceConverter || property.Converter?.GetType() == typeof(TypeConverter)))
                {
                    result.Add(property);
                    continue;
                }

                object propertyValue = property.GetValue(instance);
                result.Add(new ExpandablePropertyDescriptor(property, propertyValue));
            }

            return result;
        }

        #endregion

        #endregion
    }
}
