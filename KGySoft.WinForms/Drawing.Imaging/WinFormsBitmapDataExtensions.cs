#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WinFormsBitmapDataExtensions.cs
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

#region Used Namespaces

using System;
using System.Drawing;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows.Forms;

using KGySoft.Drawing.Shapes;
using KGySoft.Threading;
using KGySoft.WinForms;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Pen = KGySoft.Drawing.Shapes.Pen;

#endregion

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides WinForms-specific extension methods for the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> type. This class contains essentially the same text-drawing extension methods as
    /// the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions.htm" target="_blank">KGySoft.Drawing.Imaging.ReadWriteBitmapDataExtensions</a>
    /// class from <a href="https://github.com/koszeggy/KGySoft.Drawing" target="_blank">KGy SOFT Drawing Libraries</a>, but the methods in this class
    /// expect a WinForms-specific <see cref="TextFormatFlags"/>&#160;<see langword="enum"/> instead of a disposable <see cref="StringFormat"/> instance.
    /// </summary>
    public static class WinFormsBitmapDataExtensions
    {
        #region Methods

        #region DrawTextOutline

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate float overloads for convenience reasons.

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, float x, float y, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(color, text, font, x, y, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawTextOutline(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="width">The width of the text's bounding rectangle.</param>
        /// <param name="height">The height of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, float x, float y, float width, float height, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(color, text, font, x, y, width, height, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated to be consistent with KGySoft.Drawing.Shapes.BitmapDataExtensions

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
           => bitmapData.DrawTextOutline(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
           => bitmapData.DrawTextOutline(pen, text, font, location, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
           => bitmapData.DrawTextOutline(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
           => bitmapData.DrawTextOutline(pen, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(context, color, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(context, pen, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(context, color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
           => bitmapData.DrawTextOutline(context, pen, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawTextOutline.htm">EndDrawTextOutline</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawTextOutline(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw the outline of a text with the specified <see cref="Pen"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawTextOutline.htm">EndDrawTextOutline</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawTextOutline(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawTextOutline(pen, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawTextOutline.htm">EndDrawTextOutline</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawTextOutline(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw the outline of a text with the specified <see cref="Pen"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawTextOutline.htm">EndDrawTextOutline</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawTextOutline(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawTextOutline(pen, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextOutlineAsync(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextOutlineAsync(pen, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextOutlineAsync(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Pen pen, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextOutlineAsync(pen, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

#endif
        #endregion

        #endregion

        #region DrawText

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate float overloads for convenience reasons.

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, float x, float y, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(color, text, font, x, y, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="width">The width of the text's bounding rectangle.</param>
        /// <param name="height">The height of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, float x, float y, float width, float height, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(color, text, font, x, y, width, height, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated to be consistent with KGySoft.Filling.Shapes.BitmapDataExtensions

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
            => bitmapData.DrawText(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
            => bitmapData.DrawText(brush, text, font, location, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
            => bitmapData.DrawText(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see> or <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
            => bitmapData.DrawText(brush, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, parallelConfig);

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(context, color, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(context, brush, text, font, location, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(context, color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_FillPath.htm">FillPath</a> and <a href="https://koszeggy.github.io/docs/drawing/html/Overload_KGySoft_Drawing_Shapes_BitmapDataExtensions_DrawPath.htm">DrawPath</a> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null)
            => bitmapData.DrawText(context, brush, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions);

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawText.htm">EndDrawText</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawText(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw a text, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawText.htm">EndDrawText</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawText(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawText(brush, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawText.htm">EndDrawText</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawText(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Begins to draw a text, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawText">BeginDrawText</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.BeginDrawTextOutline">BeginDrawTextOutline</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginDrawPath">BeginDrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_Imaging_ReadWriteBitmapDataExtensions_EndDrawText.htm">EndDrawText</a> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginDrawText(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
            => bitmapData.BeginDrawText(brush, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws a text asynchronously, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextAsync(color, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws a text asynchronously, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, PointF location, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextAsync(brush, text, font, location, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws a text asynchronously, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextAsync(color, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

        /// <summary>
        /// Draws a text asynchronously, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="Font"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="bounds">The bounding rectangle that specifies the size and location the text should fit into, not counting the width of the outline.</param>
        /// <param name="formatFlags">A <see cref="TextFormatFlags"/> instance that specifies text formatting information, such as line spacing and alignment.</param>
        /// <param name="drawingOptions">A <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_DrawingOptions.htm">DrawingOptions</a> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.Imaging.WinFormsBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_Path.htm">Path</a> by using the <see cref="O:System.Drawing.Drawing2D.GraphicsPath.AddString">GraphicsPath.AddString</see> and <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_GraphicsPathExtensions_ToPath.htm" target="_blank">GraphicsPathExtensions.ToPath</a> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_FastThinLines.htm">FastThinLines</a> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Shapes_DrawingOptions_AntiAliasing.htm">AntiAliasing</a> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_IReadWriteBitmapData.htm">IReadWriteBitmapData</a> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Imaging_WorkingColorSpace.htm">WorkingColorSpace</a> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Brush brush, string text, Font font, RectangleF bounds, TextFormatFlags formatFlags, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => bitmapData.DrawTextAsync(brush, text, font, bounds, formatFlags.ToStringFormat(), drawingOptions, asyncConfig);

#endif
        #endregion

        #endregion

        #endregion
    }
}
