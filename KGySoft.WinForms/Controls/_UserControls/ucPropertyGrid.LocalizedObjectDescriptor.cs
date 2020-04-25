#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using KGySoft.Libraries.Language;

#endregion

namespace KGySoft.WinForms.Controls
{
    partial class ucPropertyGrid
    {
        #region LocalizedObjectDescriptor class

        /// <summary>
        /// Implements a <see cref="ICustomTypeDescriptor"/> for any objects to obtain translated property names and descriptions.
        /// </summary>
        private sealed class LocalizedObjectDescriptor: ICustomTypeDescriptor
        {
            #region Nested classes

            #region LocalizedPropertyDescriptor class

            /// <summary>
            /// A <see cref="PropertyDescriptor"/> wrapper that translates <see cref="MemberDescriptor.DisplayName"/>,
            /// <see cref="MemberDescriptor.Category"/> and <see cref="MemberDescriptor.Description"/> of the original descriptor.
            /// Provides an alternative <see cref="TypeConverter"/> to translate openable internal properties and property values
            /// of combo box properties.
            /// </summary>
            private sealed class LocalizedPropertyDescriptor: PropertyDescriptor
            {
                #region Fields

                readonly PropertyDescriptor wrappedDescriptor;

                private TypeConverter converter;

                #endregion

                #region Properties

                #region Public Properties

                public override Type ComponentType
                {
                    get { return wrappedDescriptor.ComponentType; }
                }

                public override bool IsReadOnly
                {
                    get { return wrappedDescriptor.IsReadOnly; }
                }

                public override Type PropertyType
                {
                    get { return wrappedDescriptor.PropertyType; }
                }

                public override string DisplayName
                {
                    get
                    {
                        if (String.IsNullOrEmpty(wrappedDescriptor.DisplayName))
                            return wrappedDescriptor.DisplayName;
                        return Language.Translate(wrappedDescriptor.DisplayName + "__Property:DisplayName");
                    }
                }

                public override string Category
                {
                    get
                    {
                        if (String.IsNullOrEmpty(wrappedDescriptor.Category))
                            return wrappedDescriptor.Category;
                        return Language.Translate(wrappedDescriptor.Category + "__Property:Category");
                    }
                }

                public override string Description
                {
                    get
                    {
                        if (String.IsNullOrEmpty(wrappedDescriptor.Description))
                            return wrappedDescriptor.Description;
                        return Language.Translate(wrappedDescriptor.Description + "__Property:Description");
                    }
                }

                public override TypeConverter Converter
                {
                    get
                    {
                        if (converter == null)
                        {
                            converter = new LocalizedTypeConverter(wrappedDescriptor);
                        }
                        return converter;
                    }
                }

                #endregion

                #region Internal Properties

                internal PropertyDescriptor WrappedDescriptor
                {
                    get { return wrappedDescriptor; }
                }

                #endregion

                #endregion

                #region Constructors

                public LocalizedPropertyDescriptor(PropertyDescriptor descr)
                    : base(descr)
                {
                    wrappedDescriptor = descr;
                }

                #endregion

                #region Methods

                public override bool CanResetValue(object component)
                {
                    return wrappedDescriptor.CanResetValue(component);
                }

                public override object GetValue(object component)
                {
                    return wrappedDescriptor.GetValue(component);
                }

                public override void ResetValue(object component)
                {
                    wrappedDescriptor.ResetValue(component);
                }

                public override void SetValue(object component, object value)
                {
                    if (value != null && !PropertyType.IsAssignableFrom(value.GetType()))
                        value = Converter.ConvertFrom(value);
                    wrappedDescriptor.SetValue(component, value);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return wrappedDescriptor.ShouldSerializeValue(component);
                }

                #endregion
            }

            #endregion

            #region LocalizedTypeConverter class

            /// <summary>
            /// A <see cref="TypeConverter"/> wrapper that translates inner properties and/or drop-down values (values are translated only if property is localizable)
            /// <remarks>Since has neither default nor one <see cref="Type"/> parameter constructor is available (and since it is a private class) it canot be used and instanciated
            /// as a regular type converter. The instance of this class can be retrieved only via <see cref="LocalizedPropertyDescriptor.Converter"/> property.</remarks>
            /// </summary>
            private sealed class LocalizedTypeConverter: TypeConverter
            {
                #region LocalizedTypeDescriptorContextUnwrapper class

                /// <summary>
                /// A <see cref="ITypeDescriptorContext"/> that unwraps original <see cref="Instance"/> and <see cref="PropertyDescriptor"/> properties for the original <see cref="TypeConverter"/>.
                /// </summary>
                sealed class LocalizedTypeDescriptorContextUnwrapper: ITypeDescriptorContext
                {
                    #region Fields

                    readonly ITypeDescriptorContext wrappedContext;

                    #endregion

                    #region Constructors

                    public LocalizedTypeDescriptorContextUnwrapper(ITypeDescriptorContext wrappedContext)
                    {
                        this.wrappedContext = wrappedContext;
                    }

                    #endregion

                    #region ITypeDescriptorContext Members

                    public IContainer Container
                    {
                        get { return wrappedContext.Container; }
                    }

                    public object Instance
                    {
                        get { return ((LocalizedObjectDescriptor)wrappedContext.Instance).Object; }
                    }

                    public void OnComponentChanged()
                    {
                        wrappedContext.OnComponentChanged();
                    }

                    public bool OnComponentChanging()
                    {
                        return wrappedContext.OnComponentChanging();
                    }

                    public PropertyDescriptor PropertyDescriptor
                    {
                        get { return ((LocalizedPropertyDescriptor)wrappedContext.PropertyDescriptor).WrappedDescriptor; }
                    }

                    #endregion

                    #region IServiceProvider Members

                    public object GetService(Type serviceType)
                    {
                        return wrappedContext.GetService(serviceType);
                    }

                    #endregion
                }

                #endregion

                #region Fields

                private ITypeDescriptorContext unwrappedContext;

                private readonly PropertyDescriptor originalDescriptor;

                private Dictionary<object, string> dictionary;

                private Dictionary<string, object> reverseDictionary;

                private List<string> translatedValues;

                private bool? canTranslateValues;

                #endregion

                #region Properties

                private bool CanTranslateValues
                {
                    get
                    {
                        if (canTranslateValues.HasValue)
                            return canTranslateValues.Value;
                        canTranslateValues = originalDescriptor.Converter.GetStandardValuesSupported() && originalDescriptor.Converter.CanConvertTo(typeof(string));
                        if (canTranslateValues.Value)
                        {
                            ICollection values = originalDescriptor.Converter.GetStandardValues();
                            if (values == null)
                            {
                                canTranslateValues = false;
                                return false;
                            }
                            dictionary = new Dictionary<object, string>();
                            reverseDictionary = new Dictionary<string, object>();
                            translatedValues = new List<string>();
                            foreach (var value in values)
                            {
                                if (value == null || dictionary.ContainsKey(value))
                                {
                                    canTranslateValues = false;
                                    break;
                                }
                                string translatedEntry = Language.Translate(originalDescriptor.Converter.ConvertTo(value, typeof(string)) + Language.DistinctionSeparator + value.GetType().Name);
                                if (reverseDictionary.ContainsKey(translatedEntry))
                                {
                                    canTranslateValues = false;
                                    break;
                                }

                                dictionary.Add(value, translatedEntry);
                                reverseDictionary.Add(translatedEntry, value);
                                translatedValues.Add(translatedEntry);
                            }
                            if (!canTranslateValues.Value)
                            {
                                dictionary = null;
                                reverseDictionary = null;
                                translatedValues = null;
                            }
                        }
                        return canTranslateValues.Value;
                    }
                }

                #endregion

                #region Constructors

                public LocalizedTypeConverter(PropertyDescriptor originalDescriptor)
                {
                    this.originalDescriptor = originalDescriptor;
                }

                #endregion

                #region Methods

                #region Public Methods

                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return originalDescriptor.Converter.CanConvertFrom(UnwrapContext(context), sourceType);
                }

                public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                {
                    return originalDescriptor.Converter.CanConvertTo(UnwrapContext(context), destinationType);
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    object result;
                    if (CanTranslateValues && value != null && value.GetType() == typeof(string) && reverseDictionary.TryGetValue((string)value, out result))
                        return result;
                    return originalDescriptor.Converter.ConvertFrom(UnwrapContext(context), culture, value);
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (CanTranslateValues && destinationType == typeof(string) && value != null)
                    {
                        string result;
                        if (dictionary.TryGetValue(value, out result))
                            return result;
                        if (value is string)
                            return value;
                    }
                    return originalDescriptor.Converter.ConvertTo(UnwrapContext(context), culture, value, destinationType);
                }

                public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
                {
                    return originalDescriptor.Converter.CreateInstance(UnwrapContext(context), propertyValues);
                }

                public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
                {
                    return originalDescriptor.Converter.GetCreateInstanceSupported(UnwrapContext(context));
                }

                public override bool GetPropertiesSupported(ITypeDescriptorContext context)
                {
                    return originalDescriptor.Converter.GetPropertiesSupported(UnwrapContext(context));
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    return originalDescriptor.Converter.GetStandardValuesSupported(UnwrapContext(context));
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return originalDescriptor.Converter.GetStandardValuesExclusive(UnwrapContext(context));
                }

                public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    StandardValuesCollection result = originalDescriptor.Converter.GetStandardValues(UnwrapContext(context));
                    if (CanTranslateValues)
                    {
                        result = new StandardValuesCollection(translatedValues);
                    }
                    return result;
                }

                public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
                {
                    List<PropertyDescriptor> result = new List<PropertyDescriptor>();
                    PropertyDescriptorCollection properties = originalDescriptor.Converter.GetProperties(UnwrapContext(context), value, attributes);
                    if (properties != null)
                    {
                        foreach (PropertyDescriptor property in properties)
                        {
                            result.Add(new LocalizedPropertyDescriptor(property));
                        }
                    }
                    return new PropertyDescriptorCollection(result.ToArray());
                }

                #endregion

                #region Private Methods

                private ITypeDescriptorContext UnwrapContext(ITypeDescriptorContext context)
                {
                    if (unwrappedContext == null)
                    {
                        unwrappedContext = new LocalizedTypeDescriptorContextUnwrapper(context);
                    }
                    return unwrappedContext;
                }

                #endregion

                #endregion
            }

            #endregion

            #endregion

            #region Properties

            internal object Object { get; private set; }

            #endregion

            #region Constructors

            internal LocalizedObjectDescriptor(object obj)
            {
                Object = obj;
            }

            #endregion

            #region ICustomTypeDescriptor Members

            public AttributeCollection GetAttributes()
            {
                return TypeDescriptor.GetAttributes(Unwrap(Object), true);
            }

            public string GetClassName()
            {
                return TypeDescriptor.GetClassName(Unwrap(Object), true);
            }

            public string GetComponentName()
            {
                return TypeDescriptor.GetComponentName(Unwrap(Object), true);
            }

            public TypeConverter GetConverter()
            {
                return TypeDescriptor.GetConverter(Unwrap(Object), true);
            }

            public EventDescriptor GetDefaultEvent()
            {
                return TypeDescriptor.GetDefaultEvent(Unwrap(Object), true);
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                return TypeDescriptor.GetDefaultProperty(Unwrap(Object));
            }

            public object GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(Unwrap(Object), editorBaseType, true);
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                return TypeDescriptor.GetEvents(Unwrap(Object), attributes, true);
            }

            public EventDescriptorCollection GetEvents()
            {
                return TypeDescriptor.GetEvents(Unwrap(Object), true);
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = (Object as ICustomTypeDescriptor)?.GetProperties(attributes) ?? TypeDescriptor.GetProperties(Object, attributes, true);
                return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().Select(p => (PropertyDescriptor)new LocalizedPropertyDescriptor(p)).ToArray());
            }

            public PropertyDescriptorCollection GetProperties()
            {
                var properties = (Object as ICustomTypeDescriptor)?.GetProperties() ?? TypeDescriptor.GetProperties(Object, true);
                return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().Select(p => (PropertyDescriptor)new LocalizedPropertyDescriptor(p)).ToArray());
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return Unwrap(Object);
            }

            #endregion
        }

        #endregion
    }
}
