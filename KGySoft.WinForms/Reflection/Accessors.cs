using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.Reflection;

namespace KGySoft.WinForms.Reflection
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        private static readonly LockFreeCacheOptions cacheOptions = new LockFreeCacheOptions { ThresholdCapacity = 128, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromSeconds(1) };

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type? FieldType, string? FieldNamePattern), FieldAccessor?>? fields;
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string PropertyName), PropertyAccessor?>? properties;
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string MethodName), MethodAccessor?>? methods;

        // not from methods because these are not unique
        private static MethodAccessor? methodGraphicsExtensions_CreateRoundedRectangle;
        private static MethodAccessor? methodControl_PaintBackground;
        private static MethodAccessor? methodButtonBase_Animate;
        private static MethodAccessor? methodControlPaint_DrawImageDisabled;
        private static MethodAccessor? methodControlPaint_DrawBackgroundImage;
        private static MethodAccessor? methodControlPaint_DrawImageColorized;

        #endregion

        #region Properties

        #region Application

        internal static bool ComCtlSupportsVisualStyles => (bool)GetPropertyValue(typeof(Application), "ComCtlSupportsVisualStyles")!;

        #endregion

        #region Control

        internal static object PaintEvent
        {
            get
            {
                const string fieldName =
#if NETFRAMEWORK
                "EventPaint";
#else
                    "s_paintEvent";
#endif

                return GetFieldValue<object>(typeof(Control), fieldName)!;
            }
        }

        #endregion


        #endregion

        #region Methods

        #region Field Accessors

        internal static void SetCurrentChanged(this ErrorProvider errorProvider, EventHandler currentChanged)
            => SetFieldValue(errorProvider, "currentChanged", currentChanged);

        internal static BindingManagerBase? GetErrorManager(this ErrorProvider errorProvider)
            => GetFieldValue<BindingManagerBase?>(errorProvider, "errorManager");

        #endregion

        #region Internal Methods

        #region Control

        internal static int GetControlState(this Control control)
        {
            // simple field name pattern is not guaranteed to work because there are at least 3 fields in Control that has "state" in its name.
#if NETFRAMEWORK || NETCOREAPP3_0
            const string fieldName = "state";
#else
            const string fieldName = "_state";
#endif
            var field = GetField(typeof(Control), null, fieldName);
            if (field == null)
                throw new InvalidOperationException(Res.AccessorsInstanceFieldDoesNotExist(fieldName, typeof(Control)));

            // actually a private State enum but enums can be unboxed as their underlying type
            return (int)field.Get(control)!;
        }

        internal static ContentAlignment RtlTranslateContent(this Control control, ContentAlignment alignment)
            => (ContentAlignment)GetMethod(typeof(Control), "RtlTranslateContent")!.Invoke(control, alignment)!;

        internal static bool ShowKeyboardCues(this Control control) => (bool)GetPropertyValue(control, "ShowKeyboardCues")!;

        internal static void PaintBackground(this Control c, PaintEventArgs e, Rectangle rectangle, Color backColor, Point scrollOffset)
        {
            methodControl_PaintBackground ??= MethodAccessor.GetAccessor(typeof(Control).GetMethod("PaintBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(PaintEventArgs), typeof(Rectangle), typeof(Color), typeof(Point)}, null)!);
            methodControl_PaintBackground.Invoke(c, e, rectangle, backColor, scrollOffset);
        }

        internal static void OnPaint(this Control control, PaintEventArgs e) => GetMethod(typeof(Control), "OnPaint")!.Invoke(control, e);

        internal static void SetDoubleBuffered(this Control control, bool value) => GetProperty(typeof(Control), "DoubleBuffered")!.Set(control, value);
        internal static void SetStyle(this Control control, ControlStyles flags, bool value)
            => GetMethod(typeof(Control), "SetStyle")!.Invoke(control, flags, value);

        #endregion

        #region ButtonBase

        internal static void SetShowToolTip(this ButtonBase instance, bool value) => GetProperty(typeof(ButtonBase), "ShowToolTip")!.Set(instance, value);

        internal static void Animate(this ButtonBase instance)
        {
            methodButtonBase_Animate ??= MethodAccessor.GetAccessor(typeof(ButtonBase).GetMethod("Animate", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)!);
            methodButtonBase_Animate.Invoke(instance);
        }

        #endregion

        #region Button

        internal static Size GetSystemSize(this Button button) => GetFieldValueOrDefault(button, Size.Empty, "systemSize");
        internal static void SetSystemSize(this Button button, Size value) => SetFieldValue(button, "systemSize", value, false);

        #endregion

        #region Error Provider

        internal static void UnwireEvents(this ErrorProvider errorProvider, BindingManagerBase listManager)
            => InvokeMethod(errorProvider, "UnwireEvents", listManager);

        #endregion

        #region ControlPaint

        internal static void DrawImageDisabled(this Graphics graphics, Image image, Rectangle imageBounds, Color background, bool unscaledImage)
        {
            methodControlPaint_DrawImageDisabled ??= MethodAccessor.GetAccessor(typeof(ControlPaint)
                .GetMethod("DrawImageDisabled", BindingFlags.Static | BindingFlags.NonPublic, null, 
                    new[] {typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color), typeof(bool)}, null)!);
            methodControlPaint_DrawImageDisabled.Invoke(null, graphics, image, imageBounds, background, unscaledImage);
        }

        internal static void DrawBackgroundImage(this Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            methodControlPaint_DrawBackgroundImage ??= MethodAccessor.GetAccessor(typeof(ControlPaint)
                .GetMethod("DrawBackgroundImage", BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] {typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft)}, null)!);
            methodControlPaint_DrawBackgroundImage.Invoke(null, g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft);
        }

        internal static void DrawImageColorized(this Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            methodControlPaint_DrawImageColorized ??= MethodAccessor.GetAccessor(typeof(ControlPaint)
                .GetMethod("DrawImageColorized", BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] {typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color)}, null)!);
            methodControlPaint_DrawImageColorized.Invoke(null, graphics, image, destination, replaceBlack);
        }

        #endregion

        #region Form

#if NETFRAMEWORK || NETCOREAPP3_0
        internal static bool IsGripVisible(this Form form)
        {
            var formState = GetFieldValue<BitVector32>(form, "formState", false);
            var section = GetFieldValue<BitVector32.Section>(typeof(Form), "FormStateRenderSizeGrip", false);
            return formState[section] != 0;
        }
#endif

        #endregion

        #region GraphicsExtensions

        internal static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            methodGraphicsExtensions_CreateRoundedRectangle ??= MethodAccessor.GetAccessor(typeof(Drawing.GraphicsExtensions).GetMethod("CreateRoundedRectangle", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Rectangle), typeof(int) }, null)!);
            return (GraphicsPath)methodGraphicsExtensions_CreateRoundedRectangle.Invoke(null, bounds, radius)!;
        }

        #endregion

        #endregion

        #region Private Methods

        private static PropertyAccessor? GetProperty(Type type, string propertyName)
        {
            static PropertyAccessor? GetPropertyAccessor((Type DeclaringType, string PropertyName) key)
            {
                // Properties are meant to be used for visible members so always exact names are searched
                PropertyInfo? property = key.DeclaringType.GetProperty(key.PropertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                return property == null ? null : PropertyAccessor.GetAccessor(property);
            }

            if (properties == null)
                Interlocked.CompareExchange(ref properties, ThreadSafeCacheFactory.Create<(Type, string), PropertyAccessor?>(GetPropertyAccessor, cacheOptions), null);
            return properties[(type, propertyName)];
        }

        private static object? GetPropertyValue(object instance, string propertyName)
        {
            PropertyAccessor? property = GetProperty(instance.GetType(), propertyName);
            if (property == null)
                throw new InvalidOperationException(Res.AccessorsInstancePropertyDoesNotExist(propertyName, instance.GetType()));
            return property.Get(instance);
        }

        private static object? GetPropertyValue(Type type, string propertyName)
        {
            PropertyAccessor? property = GetProperty(type, propertyName);
            if (property == null)
                throw new InvalidOperationException(Res.AccessorsStaticPropertyDoesNotExist(propertyName, type));
            return property.Get(null);
        }

        private static MethodAccessor? GetMethod(Type type, string methodName)
        {
            static MethodAccessor? GetMethodAccessor((Type DeclaringType, string MethodName) key)
            {
                // Properties are meant to be used for visible members so always exact names are searched
                MethodInfo? method = key.DeclaringType.GetMethod(key.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                return method == null ? null : MethodAccessor.GetAccessor(method);
            }

            if (methods == null)
                Interlocked.CompareExchange(ref methods, ThreadSafeCacheFactory.Create<(Type, string), MethodAccessor?>(GetMethodAccessor, cacheOptions), null);
            return methods[(type, methodName)];
        }

        private static object? InvokeMethod(object instance, string methodName, params object[] parameters)
        {
            var method = GetMethod(instance.GetType(), methodName);
            if (method == null)
                throw new InvalidOperationException(Res.AccessorsInstanceMethodDoesNotExist(methodName, instance.GetType()));
            return method.Invoke(instance, parameters);
        }

        private static FieldAccessor? GetField(Type type, Type? fieldType, string? fieldNamePattern)
        {
            // Fields are meant to be used for non-visible members either by type or name pattern (or both)
            FieldAccessor? GetFieldAccessor((Type DeclaringType, Type? FieldType, string? FieldNamePattern) key)
            {
                for (Type? t = key.DeclaringType; t != typeof(object); t = t.BaseType)
                {
                    FieldInfo[] fieldArray = t!.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? field = fieldArray.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType) && f.Name == key.FieldNamePattern) // exact name first
                        ?? fieldArray.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType)
                            && (key.FieldNamePattern == null || f.Name.Contains(key.FieldNamePattern, StringComparison.OrdinalIgnoreCase)));

                    if (field != null)
                        return FieldAccessor.GetAccessor(field);
                }

                return null;
            }

            if (fields == null)
                Interlocked.CompareExchange(ref fields, ThreadSafeCacheFactory.Create<(Type, Type?, string?), FieldAccessor?>(GetFieldAccessor, cacheOptions), null);
            return fields[(type, fieldType, fieldNamePattern)];
        }

        private static T? GetFieldValue<T>(object instance, string? fieldNamePattern = null, bool throwIfMissing = true)
        {
            Type type = instance.GetType();
            FieldAccessor? field = GetField(type, typeof(T), fieldNamePattern);
            if (field == null)
            {
                if (throwIfMissing)
                    throw new InvalidOperationException(Res.AccessorsInstanceFieldDoesNotExist(fieldNamePattern, type));
                return default;
            }

            return (T)field.Get(instance)!;
        }

        private static T? GetFieldValue<T>(Type type, string? fieldNamePattern = null, bool throwIfMissing = true)
        {
            FieldAccessor? field = GetField(type, typeof(T), fieldNamePattern);
            if (field == null)
            {
                if (throwIfMissing)
                    throw new InvalidOperationException(Res.AccessorsStaticFieldDoesNotExist(fieldNamePattern, type));
                return default;
            }

            return (T)field.Get(null)!;
        }

        private static T? GetFieldValueOrDefault<T>(object instance, T? defaultValue = default, string? fieldNamePattern = null)
        {
            FieldAccessor? field = GetField(instance.GetType(), typeof(T), fieldNamePattern);
            return field == null ? defaultValue : (T)field.Get(instance)!;
        }

        private static void SetFieldValue(object instance, string fieldNamePattern, object? value, bool throwIfMissing = true)
        {
            Type type = instance.GetType();
            FieldAccessor? field = GetField(type, null, fieldNamePattern);
            if (field == null)
            {
                if (throwIfMissing)
                    throw new InvalidOperationException(Res.AccessorsInstanceFieldDoesNotExist(fieldNamePattern, type));
                return;
            }

            field.Set(instance, value);
        }

        #endregion

        #endregion
    }
}
