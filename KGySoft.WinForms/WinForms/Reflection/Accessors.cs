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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if !NET5_0_OR_GREATER
using System.Collections.Specialized;
#endif
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

using KGySoft.Collections;
#if NETFRAMEWORK
using KGySoft.CoreLibraries;
#endif
using KGySoft.Reflection;

#endregion

#region Suppressions

#if NETFRAMEWORK
// ReSharper disable RedundantSuppressNullableWarningExpression 
#endif

#endregion

namespace KGySoft.WinForms.Reflection
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        private static readonly LockFreeCacheOptions cacheOptions = new() { ThresholdCapacity = 16, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromMilliseconds(100) };

        // Property keys and lookup callbacks. Public flags are added to support possible future compatibility for originally non-visible properties.
        private static readonly object propApplication_ComCtlSupportsVisualStyles = new();
        private static readonly object propControl_ShowKeyboardCues = new();
        private static readonly object propControl_DoubleBuffered = new();
        private static readonly object propControl_ShowToolTip = new();
        private static readonly object propFont_NativeFont = new();
        private static readonly object propComboBox_ComboListBox = new();
        private static readonly object propComboBox_TextBox = new();
        private static readonly object propComboBox_ButtonArea = new();
        private static readonly object propDateTimePicker_DropDownArrowRect = new();
        private static readonly Dictionary<object, Func<PropertyInfo?>> propertyLookup = new(9)
        {
            [propApplication_ComCtlSupportsVisualStyles] = () => typeof(Application).GetProperty(nameof(ComCtlSupportsVisualStyles), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public),
            [propControl_ShowKeyboardCues] = () => typeof(Control).GetProperty(nameof(ShowKeyboardCues), BindingFlags.Instance | BindingFlags.NonPublic),
            [propControl_DoubleBuffered] = () => typeof(Control).GetProperty(nameof(DoubleBuffered), BindingFlags.Instance | BindingFlags.NonPublic),
            [propControl_ShowToolTip] = () => typeof(ButtonBase).GetProperty(nameof(ShowToolTip), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
            [propFont_NativeFont] = () => typeof(Font).GetProperty(OSHelper.IsMono ? "NativeObject" : "NativeFont", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
            [propComboBox_ComboListBox] = () => typeof(ComboBox).GetProperty("UIAComboListBox", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
            [propComboBox_TextBox] = () => typeof(ComboBox).GetProperty("UIATextBox", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
            [propComboBox_ButtonArea] = () => typeof(ComboBox).GetProperty("ButtonArea", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
            [propDateTimePicker_DropDownArrowRect] = () => typeof(DateTimePicker).GetProperty("drop_down_arrow_rect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
        };

        // Method keys and lookup callbacks. A public binding flag is added by FindMethod to support possible future compatibility for originally non-visible methods.
        private static readonly object methodControl_RtlTranslateContent = new();
        private static readonly object methodControl_GetStyle = new();
        private static readonly object methodControl_OnPaintBackground = new();
        private static readonly object methodControl_OnPaint = new();
        private static readonly object methodControl_PaintTransparentBackground = new();
        private static readonly object methodControl_PaintBackground = new();
        private static readonly object methodControl_PaintControlBackground = new();
        private static readonly object methodControl_SetState = new();
        private static readonly object methodControl_SetStyle = new();
        private static readonly object methodButtonBase_Animate = new();
        private static readonly object methodErrorProvider_UnwireEvents = new();
        private static readonly object methodControlPaint_DrawBackgroundImage = new();
        private static readonly object methodControlPaint_DrawHighContrastFocusRectangle = new();
        private static readonly Dictionary<object, Func<MethodInfo?>> methodLookup = new(13)
        {
            [methodControl_RtlTranslateContent] = () => FindMethod(typeof(Control), nameof(RtlTranslateContent), [typeof(ContentAlignment)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_GetStyle] = () => FindMethod(typeof(Control), nameof(GetStyle), [typeof(ControlStyles)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_OnPaintBackground] = () => FindMethod(typeof(Control), nameof(OnPaintBackground), [typeof(PaintEventArgs)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_OnPaint] = () => FindMethod(typeof(Control), nameof(OnPaint), [typeof(PaintEventArgs)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_PaintTransparentBackground] = () => FindMethod(typeof(Control), nameof(ControlExtensions.PaintTransparentBackground), [typeof(PaintEventArgs), typeof(Rectangle), typeof(Region)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_PaintBackground] = () => FindMethod(typeof(Control), nameof(ControlExtensions.PaintBackground), [typeof(PaintEventArgs), typeof(Rectangle), typeof(Color), typeof(Point)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_PaintControlBackground] = () => FindMethod(typeof(Control), "PaintControlBackground", [typeof(PaintEventArgs)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_SetState] = () => FindMethod(typeof(Control), nameof(SetState), [/*int|State*/null, typeof(bool)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControl_SetStyle] = () => FindMethod(typeof(Control), nameof(SetStyle), [typeof(ControlStyles), typeof(bool)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodButtonBase_Animate] = () => FindMethod(typeof(ButtonBase), nameof(Animate), [], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodErrorProvider_UnwireEvents] = () => FindMethod(typeof(ErrorProvider), nameof(UnwireEvents), [typeof(BindingManagerBase)], BindingFlags.Instance | BindingFlags.NonPublic),
            [methodControlPaint_DrawBackgroundImage] = () => FindMethod(typeof(ControlPaint), nameof(ControlPaintHelper.DrawBackgroundImage), [typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft)], BindingFlags.Static | BindingFlags.NonPublic),
            [methodControlPaint_DrawHighContrastFocusRectangle] = () => FindMethod(typeof(ControlPaint), nameof(ControlPaintHelper.DrawHighContrastFocusRectangle), [typeof(Graphics), typeof(Rectangle), typeof(Color)], BindingFlags.Static | BindingFlags.NonPublic),
        };

        // Field keys and lookup callbacks. The DeclaredOnly flag is implicitly added by FindField.
        private static readonly object fieldControl_PaintEvent = new();
        private static readonly object fieldButton_systemSize = new();
        private static readonly object fieldComboBox_mouseEvents = new();
        private static readonly object fieldComboBox_mousePressed = new();
        private static readonly object fieldErrorProvider_currentChanged = new();
        private static readonly object fieldErrorProvider_errorManager = new();
        private static readonly object fieldForm_formState = new();
        private static readonly Dictionary<object, Func<FieldInfo?>> fieldLookup = new(7)
        {
#if NETFRAMEWORK
            [fieldControl_PaintEvent] = () => FindField(typeof(Control), OSHelper.IsMono ? "PaintEvent" : "EventPaint", typeof(object), BindingFlags.Static | BindingFlags.NonPublic),
#else
            [fieldControl_PaintEvent] = () => FindField(typeof(Control), "s_paintEvent", typeof(object), BindingFlags.Static | BindingFlags.NonPublic),
#endif
            [fieldButton_systemSize] = () => FindField(typeof(Button), "systemSize", typeof(Size), BindingFlags.Instance | BindingFlags.NonPublic),
            [fieldComboBox_mouseEvents] = () => FindField(typeof(ComboBox), "mouseEvents", typeof(bool), BindingFlags.Instance | BindingFlags.NonPublic),
            [fieldComboBox_mousePressed] = () => FindField(typeof(ComboBox), "mousePressed", typeof(bool), BindingFlags.Instance | BindingFlags.NonPublic),
            [fieldErrorProvider_currentChanged] = () => FindField(typeof(ErrorProvider), "currentChanged", typeof(EventHandler), BindingFlags.Instance | BindingFlags.NonPublic),
            [fieldErrorProvider_errorManager] = () => FindField(typeof(ErrorProvider), "errorManager", typeof(BindingManagerBase), BindingFlags.Instance | BindingFlags.NonPublic),
#if !NET5_0_OR_GREATER
            [fieldForm_formState] = () => FindField(typeof(Form), "formState", typeof(Form), BindingFlags.Instance | BindingFlags.NonPublic),
#endif
        };

        private static IThreadSafeCacheAccessor<object, PropertyAccessor?>? properties;
        private static IThreadSafeCacheAccessor<object, MethodAccessor?>? methods;
        private static IThreadSafeCacheAccessor<object, FieldAccessor?>? fields;

        #endregion

        #region Properties

        #region Application

        internal static bool ComCtlSupportsVisualStyles => TryGetProperty(propApplication_ComCtlSupportsVisualStyles)?.GetStaticValue<bool>() ?? false;

        #endregion

        #region Control

        internal static object? PaintEvent => TryGetField(fieldControl_PaintEvent)?.GetStaticValue<object>();

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        #region Control

        internal static ContentAlignment RtlTranslateContent(this Control control, ContentAlignment alignment)
            => GetMethod(methodControl_RtlTranslateContent, typeof(Control), nameof(RtlTranslateContent)).InvokeInstanceFunction<Control, ContentAlignment, ContentAlignment>(control, alignment);

        internal static bool ShowKeyboardCues(this Control control)
            => GetProperty(propControl_ShowKeyboardCues, typeof(Control), nameof(ShowKeyboardCues)).GetInstanceValue<Control, bool>(control);

        // NOTE: we must use the Region overload, because that's what is available both in .NET Framework and Core
        internal static bool TryPaintTransparentBackground(this Control c, PaintEventArgs e, Rectangle rectangle, Region? region = null)
        {
            if (TryGetMethod(methodControl_PaintTransparentBackground) is not MethodAccessor accessor)
                return false;

            accessor.InvokeInstanceAction(c, e, rectangle, region);
            return true;
        }

        internal static bool TryPaintBackground(this Control c, PaintEventArgs e, Rectangle rectangle, Color backColor, Point scrollOffset)
        {
#if NETFRAMEWORK
            if (!OSHelper.IsMono)
#endif
            {
                // We must use always the 4 parameters overload, because the others have been removed in .NET (they exist in .NET Framework only).
                if (TryGetMethod(methodControl_PaintBackground) is not MethodAccessor accessor)
                    return false;
                accessor.InvokeInstanceAction(c, e, rectangle, backColor, scrollOffset);
                return true;
            }

#if NETFRAMEWORK
            // PaintControlBackground on Mono is similar as PaintBackground, except that it cannot use a custom back color, bounds and background image offset.
            // We can ignore the latter two, but not the custom back color (see more details at ControlExtensions.PaintBackground).
            // ISSUE: Mono may draw the transparent background incorrectly (e.g. when the parent is a GroupBox), so not allowing alpha back color here either.
            if (backColor.A != Byte.MaxValue || backColor.ToArgb() != c.BackColor.ToArgb() || TryGetMethod(methodControl_PaintControlBackground) is not MethodAccessor accessorMono)
                return false;

            // Here we call the native method only if c.BackColor is the same as the desired opaque backColor. It draws the possible background image on the back color.
            accessorMono.InvokeInstanceAction(c, e);
            return true;
#endif
        }

        internal static bool GetStyle(this Control control, ControlStyles styles)
            => GetMethod(methodControl_GetStyle, typeof(Control), nameof(GetStyle)).InvokeInstanceFunction<Control, ControlStyles, bool>(control, styles);

        internal static void OnPaintBackground(this Control control, PaintEventArgs e)
            => GetMethod(methodControl_OnPaintBackground, typeof(Control), nameof(OnPaintBackground)).InvokeInstanceAction(control, e);

        internal static void OnPaint(this Control control, PaintEventArgs e)
            => GetMethod(methodControl_OnPaint, typeof(Control), nameof(OnPaint)).InvokeInstanceAction(control, e);
        
        internal static void DoubleBuffered(this Control control, bool value)
            => GetProperty(propControl_DoubleBuffered, typeof(Control), nameof(DoubleBuffered)).SetInstanceValue(control, value);
        
        internal static void SetStyle(this Control control, ControlStyles flags, bool value)
            => GetMethod(methodControl_SetStyle, typeof(Control), nameof(SetStyle)).InvokeInstanceAction(control, flags, value);

        // NOTE: on newer .NET versions the state parameter is an enum, but reflection works with the underlying type (int) as well, so using always the non-generic Invoke with int
        internal static void SetState(this Control control, int state, bool value)
            => TryGetMethod(methodControl_SetState)?.Invoke(control, state, value);

        #endregion

        #region ButtonBase

        internal static void ShowToolTip(this ButtonBase instance, bool value) => TryGetProperty(propControl_ShowToolTip)?.SetInstanceValue(instance, value);
        internal static void Animate(this ButtonBase instance) => TryGetMethod(methodButtonBase_Animate)?.InvokeInstanceAction(instance);

        #endregion

        #region Button

        internal static Size? GetSystemSize(this Button button) => TryGetField(fieldButton_systemSize)?.GetInstanceValue<Button, Size>(button);
        internal static void SetSystemSize(this Button button, Size value) => TryGetField(fieldButton_systemSize)?.SetInstanceValue(button, value);

        #endregion

        #region ComboBox

        internal static void SetMouseEvents(this ComboBox comboBox)
        {
            Debug.Assert(!OSHelper.IsMono);
            TryGetField(fieldComboBox_mouseEvents)?.SetInstanceValue(comboBox, true);
            TryGetField(fieldComboBox_mousePressed)?.SetInstanceValue(comboBox, true);
        }

        internal static Control? InnerListBox(this ComboBox comboBox)
        {
            Debug.Assert(OSHelper.IsMono);
            return TryGetProperty(propComboBox_ComboListBox)?.GetInstanceValue<ComboBox, Control?>(comboBox);
        }

        internal static TextBox? InnerTextBox(this ComboBox comboBox)
        {
            Debug.Assert(OSHelper.IsMono);
            return TryGetProperty(propComboBox_TextBox)?.GetInstanceValue<ComboBox, TextBox?>(comboBox);
        }

        internal static Rectangle? GetButtonArea(this ComboBox comboBox)
        {
            Debug.Assert(OSHelper.IsMono);
            return TryGetProperty(propComboBox_ButtonArea)?.GetInstanceValue<ComboBox, Rectangle>(comboBox);
        }

        #endregion

        #region DateTimePicker

        internal static Rectangle? DropDownArrowRect(this DateTimePicker dateTimePicker)
        {
            Debug.Assert(OSHelper.IsMono);
            return TryGetProperty(propDateTimePicker_DropDownArrowRect)?.GetInstanceValue<DateTimePicker, Rectangle>(dateTimePicker);
        }

        #endregion

        #region Error Provider

        internal static void SetCurrentChanged(this ErrorProvider errorProvider, EventHandler currentChanged)
            => TryGetField(fieldErrorProvider_currentChanged)?.SetInstanceValue(errorProvider, currentChanged);

        internal static BindingManagerBase? GetErrorManager(this ErrorProvider errorProvider)
            => TryGetField(fieldErrorProvider_errorManager)?.GetInstanceValue<ErrorProvider, BindingManagerBase>(errorProvider);

        internal static void UnwireEvents(this ErrorProvider errorProvider, BindingManagerBase listManager)
            => TryGetMethod(methodErrorProvider_UnwireEvents)?.InvokeInstanceAction(errorProvider, listManager);

        #endregion

        #region ControlPaint

        internal static bool TryDrawBackgroundImage(this Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (TryGetMethod(methodControlPaint_DrawBackgroundImage) is not MethodAccessor accessor)
                return false;

            accessor.Invoke(null, g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft);
            return true;
        }

        internal static bool TryDrawHighContrastFocusRectangle(this Graphics graphics, Rectangle rectangle, Color color)
        {
            if (TryGetMethod(methodControlPaint_DrawHighContrastFocusRectangle) is not MethodAccessor accessor)
                return false;

            accessor.InvokeStaticAction(graphics, rectangle, color);
            return true;
        }

        #endregion

        #region Form

#if !NET5_0_OR_GREATER
        internal static BitVector32 FormState(this Form form) => TryGetField(fieldForm_formState)?.GetInstanceValue<Form, BitVector32>(form) ?? default;
#endif

        #endregion

        #region Font

        internal static IntPtr? GetNativeFont(this Font font) => TryGetProperty(propFont_NativeFont)?.GetInstanceValue<Font, IntPtr>(font);

        #endregion

        #endregion

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static PropertyAccessor? TryGetProperty(object key)
        {
            #region Local Methods
            
            [MethodImpl(MethodImplOptions.NoInlining)]
            static PropertyAccessor? GetPropertyAccessor(object key)
            {
                if (!propertyLookup.TryGetValue(key, out var func))
                    throw new InvalidOperationException(Res.InternalError("GetPropertyAccessor: Property key found"));
                PropertyInfo? result = func.Invoke();
                return result is null ? null : PropertyAccessor.GetAccessor(result);
            }

            #endregion

            if (properties == null)
                Interlocked.CompareExchange(ref properties, ThreadSafeCacheFactory.Create<object, PropertyAccessor?>(GetPropertyAccessor, cacheOptions), null);
            return properties[key];
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static PropertyAccessor GetProperty(object key, Type type, string propertyName)
        {
            #region Local Methods

            [MethodImpl(MethodImplOptions.NoInlining)]
            static PropertyAccessor Throw(Type type, string propertyName) => throw new InvalidOperationException(Res.AccessorsPropertyDoesNotExist(propertyName, type));

            #endregion

            return TryGetProperty(key) ?? Throw(type, propertyName);
        }

        [SuppressMessage("Style", "IDE0220:Add explicit cast", Justification = "False alarm, methods are queried by GetMember")]
        private static MethodInfo? FindMethod(Type declaringType, string methodName, Type?[] parameterTypes, BindingFlags bindingFlags)
        {
            // LINQ is not a problem, this method is called only once per key by the cache item loader
            if (parameterTypes.All(t => t is not null))
                return declaringType.GetMethod(methodName, bindingFlags | BindingFlags.Public, null, parameterTypes!, null);
            
            // not all parameters are specified: matching the parameters manually
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop - false alarm, methods are queried
            foreach (MethodInfo mi in declaringType.GetMember(methodName, MemberTypes.Method, bindingFlags | BindingFlags.Public))
            {
                if (mi.IsGenericMethodDefinition)
                    continue;
                ParameterInfo[] methodParams = mi.GetParameters();
                if (methodParams.Length != parameterTypes.Length)
                    continue;
                if (methodParams.Zip(parameterTypes).All(((ParameterInfo Info, Type? ExpectedType) p) => p.ExpectedType is null || p.Info.ParameterType == p.ExpectedType))
                    return mi;
            }

            return null;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static MethodAccessor? TryGetMethod(object key)
        {
            #region Local Methods
            
            [MethodImpl(MethodImplOptions.NoInlining)]
            static MethodAccessor? GetMethodAccessor(object key)
            {
                if (!methodLookup.TryGetValue(key, out var func))
                    throw new InvalidOperationException(Res.InternalError("GetMethodAccessor: Method key found"));
                MethodInfo? result = func.Invoke();
                return result is null ? null : MethodAccessor.GetAccessor(result);
            }

            #endregion

            if (methods == null)
                Interlocked.CompareExchange(ref methods, ThreadSafeCacheFactory.Create<object, MethodAccessor?>(GetMethodAccessor, null, cacheOptions), null);
            return methods[key];
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static MethodAccessor GetMethod(object key, Type type, string methodName)
        {
            #region Local Methods

            [MethodImpl(MethodImplOptions.NoInlining)]
            static MethodAccessor Throw(Type type, string methodName) => throw new InvalidOperationException(Res.AccessorsMethodDoesNotExist(methodName, type));

            #endregion

            return TryGetMethod(key) ?? Throw(type, methodName);
        }

        private static FieldInfo? FindField(Type declaringType, string? namePattern, Type? fieldType, BindingFlags bindingFlags)
        {
            FieldInfo[] candidates = declaringType.GetFields(bindingFlags | BindingFlags.DeclaredOnly);
            return candidates.FirstOrDefault(f => (fieldType == null || f.FieldType == fieldType) && f.Name == namePattern) // exact name first
                ?? candidates.FirstOrDefault(f => (fieldType == null || f.FieldType == fieldType)
                    && (namePattern == null || f.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static FieldAccessor? TryGetField(object key)
        {
            #region Local Methods
            
            [MethodImpl(MethodImplOptions.NoInlining)]
            static FieldAccessor? GetFieldAccessor(object key)
            {
                if (!fieldLookup.TryGetValue(key, out var func))
                    throw new InvalidOperationException(Res.InternalError("GetFieldAccessor: Field key found"));
                FieldInfo? result = func.Invoke();
                return result is null ? null : FieldAccessor.GetAccessor(result);
            }

            #endregion

            if (fields == null)
                Interlocked.CompareExchange(ref fields, ThreadSafeCacheFactory.Create<object, FieldAccessor?>(GetFieldAccessor, null, cacheOptions), null);
            return fields[key];
        }

        #endregion

        #endregion
    }
}
