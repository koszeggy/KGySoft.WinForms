#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
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
#if !NET5_0_OR_GREATER
using System.Collections.Specialized;
#endif
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

#endregion

namespace KGySoft.WinForms.Reflection
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Nested Types

        /// <summary>
        /// A value-compared variable-length tuple of types
        /// </summary>
        private readonly struct TypesKey : IEquatable<TypesKey>
        {
            #region Properties

            internal Type[] Types { get; }

            #endregion

            #region Constructors

            internal TypesKey(Type[] types) => Types = types;

            #endregion

            #region Methods

            public override bool Equals(object? obj) => obj is TypesKey key && Equals(key);

            public bool Equals(TypesKey other)
            {
                if (Types.Length != other.Types.Length)
                    return false;
                for (int i = 0; i < Types.Length; i++)
                {
                    if (!ReferenceEquals(Types[i], other.Types[i]))
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                var result = 13;

                // ReSharper disable once ForCanBeConvertedToForeach - performance
                for (int i = 0; i < Types.Length; i++)
                    result = result * 397 + Types[i].GetHashCode();

                return result;
            }

            public override string ToString() => $"({Types.Select(t => t.GetName(TypeNameKind.ShortName)).Join(", ")})";

            #endregion
        }

        #endregion

        #region Fields

        private static readonly LockFreeCacheOptions cacheOptions = new LockFreeCacheOptions { ThresholdCapacity = 128, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromSeconds(1) };

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type? FieldType, string? FieldNamePattern), FieldAccessor?>? fields;
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string PropertyName), PropertyAccessor?>? properties;
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string MethodName), MethodAccessor?>? methodsByName;
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string MethodName, TypesKey ParameterTypes), MethodAccessor?>? methodsByTypes;

        #endregion

        #region Properties

        #region Application

        internal static bool ComCtlSupportsVisualStyles => TryGetPropertyValue<bool>(typeof(Application), nameof(ComCtlSupportsVisualStyles));

        #endregion

        #region Control

        internal static object? PaintEvent
        {
            get
            {
                string fieldName = OSHelper.IsMono ? "PaintEvent" :
#if NETFRAMEWORK
                    "EventPaint";
#else
                    "s_paintEvent";
#endif

                return GetFieldValue<object?>(typeof(Control), fieldName, false);
            }
        }

        #endregion

        #endregion

        #region Methods

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
            => (ContentAlignment)InvokeMethod(control, "RtlTranslateContent", alignment)!;

        internal static bool ShowKeyboardCues(this Control control) => (bool)GetPropertyValue(control, "ShowKeyboardCues")!;

        internal static void PaintBackground(this Control c, PaintEventArgs e, Rectangle rectangle, Color backColor, Point scrollOffset = default)
        {
            if (TryInvokeMethod(c, "PaintBackground", [typeof(PaintEventArgs), typeof(Rectangle), typeof(Color), typeof(Point)], e, rectangle, backColor, scrollOffset))
                return;

            // transparent background on Mono (this draws the possible background image)
            if (backColor.A == 0 && OSHelper.IsMono && TryInvokeMethod(c, "PaintControlBackground", [typeof(PaintEventArgs)], e))
                return;

            // fallback solution (including Mono): painting the specified back color
            // Transparent back color: obtaining the color from the parent (occurs only when none of the method invoked above worked)
            while (backColor.A == 0 && c.Parent is Control parent)
            {
                backColor = parent.BackColor;
                c = parent;
            }

            e.Graphics.FillRectangle((backColor.A == Byte.MaxValue ? backColor : backColor.ToColor32().ToOpaque().ToColor()).GetBrush(), rectangle);
        }

        internal static void OnPaint(this Control control, PaintEventArgs e) => InvokeMethod(control, "OnPaint", e);

        internal static void SetDoubleBuffered(this Control control, bool value) => GetProperty(typeof(Control), "DoubleBuffered")!.Set(control, value);
        internal static void SetStyle(this Control control, ControlStyles flags, bool value)
            => GetMethodByName(typeof(Control), "SetStyle")!.Invoke(control, flags, value);

        // NOTE: on newer .NET versions the state parameter is an enum, but reflection works with the underlying type (int) as well
        internal static void SetState(this Control control, int state, bool value)
            => GetMethodByName(typeof(Control), "SetState")?.Invoke(control, state, value);

        #endregion

        #region ButtonBase

        internal static void SetShowToolTip(this ButtonBase instance, bool value) => GetProperty(typeof(ButtonBase), "ShowToolTip")?.Set(instance, value);

        internal static void Animate(this ButtonBase instance) => TryInvokeMethod(instance, "Animate", []);

        #endregion

        #region Button

        internal static Size GetSystemSize(this Button button) => GetFieldValueOrDefault(button, Size.Empty, "systemSize");
        internal static void SetSystemSize(this Button button, Size value) => SetFieldValue(button, "systemSize", value, false);

        #endregion

        #region ComboBox

        internal static void SetMouseEvents(this ComboBox comboBox)
        {
            SetFieldValue(comboBox, "mouseEvents", true, false);
            SetFieldValue(comboBox, "mousePressed", true, false);
        }

        #endregion

        #region Error Provider

        internal static void SetCurrentChanged(this ErrorProvider errorProvider, EventHandler currentChanged)
            => SetFieldValue(errorProvider, "currentChanged", currentChanged);

        internal static BindingManagerBase? GetErrorManager(this ErrorProvider errorProvider)
            => GetFieldValue<BindingManagerBase?>(errorProvider, "errorManager");

        internal static void UnwireEvents(this ErrorProvider errorProvider, BindingManagerBase listManager)
            => TryInvokeMethod(errorProvider, "UnwireEvents", listManager);

        #endregion

        #region ControlPaint

        internal static void DrawBackgroundImage(this Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
            => TryInvokeMethod(typeof(ControlPaint), "DrawBackgroundImage", [typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft)],
                g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft);

        internal static void DrawImageColorized(this Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            if (TryInvokeMethod(typeof(ControlPaint), "DrawImageColorized", [typeof(Graphics), typeof(Image), typeof(Rectangle), typeof(Color)],
                    graphics, image, destination, replaceBlack))
            {
                return;
            }

            // fallback solution: manually drawing the recolored image
            Bitmap? recolored = null;
            try
            {
                if (replaceBlack.ToArgb() != Color.Black.ToArgb())
                {
                    recolored = new Bitmap(image);
                    recolored.ReplaceColor(Color.Black, replaceBlack);
                }

                graphics.DrawImage(recolored ?? image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }
            finally
            {
                recolored?.Dispose();
            }
        }

        internal static void DrawHighContrastFocusRectangle(this Graphics graphics, Rectangle rectangle, Color color)
        {
            if (TryInvokeMethod(typeof(ControlPaint), "DrawHighContrastFocusRectangle", [typeof(Graphics), typeof(Rectangle), typeof(Color)], graphics, rectangle, color))
                return;

            // fallback solution: manually drawing a simple focus rectangle, ignoring such fine details like rounding, etc.
            using Pen pen = new(color);
            pen.DashStyle = DashStyle.Dot;
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        }

        #endregion

        #region Form

#if !NET5_0_OR_GREATER
        internal static BitVector32 FormState(this Form form)
        {
            var formState = GetFieldValue<BitVector32>(form, "formState", false);
            return formState;
        }
#endif

        #endregion

        #region Font

        internal static IntPtr? GetNativeFont(this Font font)
        {
            string propertyName = OSHelper.IsMono ? "NativeObject" : "NativeFont";
            PropertyAccessor? property = GetProperty(typeof(Font), propertyName);
            return property?.GetInstanceValue<Font, IntPtr>(font);
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

        private static T TryGetPropertyValue<T>(Type type, string propertyName, T defaultValue = default!)
        {
            PropertyAccessor? property = GetProperty(type, propertyName);
            if (property == null)
                return defaultValue;
            return property.GetStaticValue<T>();
        }

        private static MethodAccessor? GetMethodByName(Type type, string methodName)
        {
            static MethodAccessor? GetMethodAccessor((Type DeclaringType, string MethodName) key)
            {
                MethodInfo? method = key.DeclaringType.GetMethod(key.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                return method == null ? null : MethodAccessor.GetAccessor(method);
            }

            if (methodsByName == null)
                Interlocked.CompareExchange(ref methodsByName, ThreadSafeCacheFactory.Create<(Type, string), MethodAccessor?>(GetMethodAccessor, cacheOptions), null);
            return methodsByName[(type, methodName)];
        }

        private static MethodAccessor? GetMethodByTypes(Type type, string methodName, TypesKey parameterTypes)
        {
            #region Local Methods
            
            static MethodAccessor? GetMethodAccessor((Type DeclaringType, string MethodName, TypesKey ParameterTypes) key)
            {
                for (Type? t = key.DeclaringType; t != typeof(object); t = t.BaseType)
                {
                    MethodInfo[] methods = t!.GetMember(key.MethodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        .Cast<MethodInfo>()
                        .Where(m => !m.IsGenericMethodDefinition && m.GetParameters().Length == key.ParameterTypes.Types.Length)
                        .ToArray();

                    foreach (MethodInfo mi in methods)
                    {
                        if (!mi.GetParameters().Select(p => p.ParameterType).SequenceEqual(key.ParameterTypes.Types))
                            continue;

                        return MethodAccessor.GetAccessor(mi);
                    }
                }

                return null;
            }

            #endregion

            if (methodsByTypes == null)
                Interlocked.CompareExchange(ref methodsByTypes, ThreadSafeCacheFactory.Create<(Type, string, TypesKey), MethodAccessor?>(GetMethodAccessor, null, cacheOptions), null);
            return methodsByTypes[(type, methodName, parameterTypes)];
        }

        private static object? TryInvokeMethod(object instance, string methodName, params object?[] parameters)
        {
            var method = GetMethodByName(instance.GetType(), methodName);
            return method?.Invoke(instance, parameters);
        }

        private static object? InvokeMethod(object instance, string methodName, params object?[] parameters)
        {
            var method = GetMethodByName(instance.GetType(), methodName);
            if (method == null)
                throw new InvalidOperationException(Res.AccessorsMethodDoesNotExist(methodName, instance.GetType()));
            return method.Invoke(instance, parameters);
        }

        // NOTE: now this method is used for void methods only, so it can return bool to indicate success
        private static bool TryInvokeMethod(object instance, string methodName, Type[] parameterTypes, params object?[] parameters)
        {
            var method = GetMethodByTypes(instance.GetType(), methodName, new TypesKey(parameterTypes));
            if (method == null)
                return false;
            method.Invoke(instance, parameters);
            return true;
        }

        // NOTE: now this method is used for void methods only, so it can return bool to indicate success
        private static bool TryInvokeMethod(Type type, string methodName, Type[] parameterTypes, params object?[] parameters)
        {
            var method = GetMethodByTypes(type, methodName, new TypesKey(parameterTypes));
            if (method == null)
                return false;
            method.Invoke(null, parameters);
            return true;
        }

        private static FieldAccessor? GetField(Type type, Type? fieldType, string? fieldNamePattern)
        {
            #region Local Methods

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

            #endregion

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

        private static void SetFieldValue<TInstance, TValue>(TInstance instance, string fieldNamePattern, TValue value, bool throwIfMissing = true)
            where TInstance : class
        {
            Type type = instance.GetType();
            FieldAccessor? field = GetField(type, typeof(TValue), fieldNamePattern);
            if (field == null)
            {
                if (throwIfMissing)
                    throw new InvalidOperationException(Res.AccessorsInstanceFieldDoesNotExist(fieldNamePattern, type));
                return;
            }

            field.SetInstanceValue(instance, value);
        }

        #endregion

        #endregion
    }
}
