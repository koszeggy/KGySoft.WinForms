#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageViewerOptimizationOptions.cs
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
using System.Drawing;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Specifies the optimization options for the <see cref="ImageViewer"/> control.
    /// Used by the <see cref="ImageViewer.OptimizationOptions"/> property.
    /// </summary>
    [Flags]
    public enum ImageViewerOptimizationOptions
    {
        /// <summary>
        /// No optimizations are applied. This consumes the least memory and is the slowest option.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the <see cref="ImageViewer"/> may generate an image internally with an optimal pixel format for rendering.
        /// </summary>
        [Description("Specifies that the ImageViewer may generate an image internally with an optimal pixel format for rendering.")]
        UseOptimalPixelFormat = 1 << 0,

        /// <summary>
        /// When this option is set and an image is displayed with a <see cref="ImageViewer.Zoom"/> value less than 1.0, the <see cref="ImageViewer"/> may generate a resized bitmap of the image for rendering.
        /// This happens asynchronously in the background. Until the resized bitmap is generated, a lower quality rendering is used.
        /// </summary>
        [Description("When this option is set and an image is displayed with a ImageViewer.Zoom value less than 1.0, the ImageViewer may generate a resized bitmap of the image for rendering. "
            + "This happens asynchronously in the background. Until the resized bitmap is generated, a lower quality rendering is used.")]
        GenerateResizedBitmap = 1 << 1,

        /// <summary>
        /// Determines whether the <see cref="ImageViewer"/> should check the quickly available memory (that is, the freely available physical memory without paging)
        /// before applying optimizations specified by the other options. If the check fails, the optimizations are not applied, and the <see cref="ImageViewer"/>
        /// will use a slower rendering method that consumes less memory. When this option is not set, the <see cref="ImageViewer"/> will always try to apply optimizations,
        /// which may lead to paging and high memory consumption. Even if this option is not set, optimizations are automatically disabled when the required unmanaged memory cannot be allocated.
        /// </summary>
        [Description("Determines whether the ImageViewer should check if there is enough quickly available memory to apply optimizations. "
            + "When this option is set, the other options may be automatically disabled internally.")]
        CheckQuicklyAvailableMemory = 1 << 2,

        /// <summary>
        /// Specifies that the <see cref="ImageViewer"/> is allowed to access the <see cref="ImageViewer.Image"/> on a background thread.
        /// When this option is disabled, <see cref="ImageViewer"/> may clone <see cref="ImageViewer.Image"/> internally to generate a resized image in the background, which means more memory consumption.
        /// <para><note type="caution">This option is for advanced users only. If you assign the same <see cref="Image"/> instance to other controls than <see cref="ImageViewer"/> instances,
        /// this option must be disabled, because it may lead to exceptions ("bitmap region is already locked").
        /// You still can access the <see cref="Image"/> from other threads if you cooperatively lock on the <see cref="Image"/> instance.</note></para>
        /// </summary>
        [Description("CAUTION: For advanced users only!\r\n"
            + "Specifies that ImageViewer is allowed to access Image on a background thread. Do not use it when the same Image instance is assigned to other type of controls, because "
            + "it may lead to exceptions (\"bitmap region is already locked\"). You still can access Image from other threads if you cooperatively lock on the Image instance.\r\n\r\n"
            + "When this option is disabled, ImageViewer needs to clone Image internally to generate a resized image in the background, which means more memory consumption.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        UseUnsafeCooperativeLocking = 1 << 3,

        /// <summary>
        /// Specifies the default optimization options for the <see cref="ImageViewer"/> control, which includes the following:
        /// <see cref="UseOptimalPixelFormat"/>, <see cref="GenerateResizedBitmap"/> and <see cref="CheckQuicklyAvailableMemory"/>.
        /// </summary>
        Default = UseOptimalPixelFormat | GenerateResizedBitmap | CheckQuicklyAvailableMemory,
    }
}