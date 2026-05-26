#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ImageViewer.DisplayImageGenerator.cs
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
using KGySoft.Threading;

#endregion

#region Suppressions

#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class may cause a runtime exception - false alarm, ImageViewer is never a remote object.

#if NETFRAMEWORK
// ReSharper disable RedundantSuppressNullableWarningExpression 
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    partial class ImageViewer
    {
        #region PreviewGenerator class

        private sealed class DisplayImageGenerator : IDisposable
        {
            #region Nested classes

            #region GenerateDefaultImageTask class
            
            private sealed class GenerateDefaultImageTask : AsyncTaskBase
            {
                #region Fields

                internal Bitmap SourceBitmap = default!;
                internal bool InvalidateOwner;

                #endregion
            }

            #endregion

            #region GenerateResizedImageTask class

            private sealed class GenerateResizedImageTask : AsyncTaskBase
            {
                #region Fields

                internal Image SourceImage = default!;
                internal Size Size;

                #endregion
            }

            #endregion

            #endregion

            #region Constants

            private const int minBitmapSizeThreshold = 1024;
            private const int metafileDoublingTimeThreshold = 100; // in milliseconds

            #endregion

            #region Fields

            #region Static Fields

            /// <summary>
            /// These formats are not supported by Graphics even though a Bitmap can use them.
            /// On Linux/Mono some formats are completely unsupported, but they do not appear here.
            /// </summary>
            private static readonly PixelFormat[] unsupportedFormats = OSHelper.IsWindows
                ? [PixelFormat.Format16bppGrayScale]
                : [PixelFormat.Format16bppRgb555, PixelFormat.Format16bppRgb565];

            /// <summary>
            /// These formats are so slow that it is still faster to generate a 32bpp clone first than display them directly.
            /// </summary>
            private static readonly PixelFormat[] slowFormats = OSHelper.IsWindows
                ? [PixelFormat.Format48bppRgb, PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb]
                : Reflector.EmptyArray<PixelFormat>();

            #endregion

            #region Instance Fields

            private readonly ImageViewer owner;

            /// <summary>
            /// The default image to be displayed when no resized display image is needed or while its generation is in progress.
            /// Set by <see cref="GenerateDefaultImage"/>. If <see cref="isDefaultImageCloned"/> is true, then contains
            /// - A fast PARGB32 clone of the original image it that is a Bitmap
            /// - A clone of the original image if that is a Metafile so the original image will not be blocked to generate resized images
            /// Otherwise, it is the same reference as the owner.Image.
            /// If <see cref="enabled"/> is false, then may contain the original image even if it cannot be displayed.
            /// </summary>
            private volatile Image? defaultDisplayImage;

            private volatile bool isDefaultImageCloned;
            private volatile bool disposed;

            /// <summary>
            /// true if generator can generate new content. Turned off on low memory or by <see cref="Free"/>. Invalidating the image enables it again.
            /// </summary>
            private volatile bool enabled;

            private GenerateDefaultImageTask? generateDefaultImageTask;
            private GenerateResizedImageTask? generateResizedImageTask;

            /// <summary>
            /// The clone of the original image that is used to safely generate the resized display image.
            /// Used only when <see cref="AllowUnsafeCooperativeLocking"/> is false, so we cannot be sure that we can safely use the original image from another thread.
            /// </summary>
            private Image? origImageClone;

            /// <summary>
            /// If not null, contains the last cached size-adjusted display image.
            /// It is not disposed immediately when a new size (<see cref="requestedSize"/>) is started to be generated
            /// so it can be re-used when toggling smooth zooming.
            /// </summary>
            private volatile Bitmap? resizedDisplayImage;

            /// <summary>
            /// Just to cache <see cref="resizedDisplayImage"/>.Size,
            /// because accessing it on <see cref="resizedDisplayImage"/> without locking can lead to "object is used elsewhere" error.
            /// </summary>
            private Size resizedDisplayImageSize;

            /// <summary>
            /// The currently requested size of the size adjusted image. If it is the same as <see cref="resizedDisplayImageSize"/>,
            /// then <see cref="resizedDisplayImage"/> can be displayed.
            /// </summary>
            private Size requestedSize;

            /// <summary>
            /// Interpreted as a size in number of pixels, and gets the smallest size above which a metafile is not resized.
            /// Needed because metafiles with bitmaps might be drawn very slowly, because the DrawImage operation
            /// uses a process-wide lock, so not even the background thread helps. Written in multiple threads, so accessed always by volatile reads/writes.
            /// </summary>
            private long maxMetafileSizeThreshold = Int64.MaxValue;

            #endregion

            #endregion

            #region Properties

            #region Internal Properties
            
            internal Lock SyncRoot { get; } = new Lock();

            #endregion

            #region Private Properties

            private bool GenerateResizedBitmap => enabled && (owner.optimizations & ImageViewerOptimizationOptions.GenerateResizedBitmap) != 0;
            private bool CheckMemoryUsage => (owner.optimizations & ImageViewerOptimizationOptions.CheckQuicklyAvailableMemory) != 0;

            #endregion

            #endregion

            #region Constructors

            internal DisplayImageGenerator(ImageViewer owner) => this.owner = owner;

            #endregion

            #region Methods

            #region Static Methods

            private static void CancelRunningGenerate(AsyncTaskBase? task)
            {
                if (task == null)
                    return;
                task.Cancel();
            }

            private static void WaitForPendingGenerate(AsyncTaskBase? task)
            {
                if (task == null)
                    return;
                task.WaitForCompletion();
                task.Dispose();
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public void Dispose()
            {
                if (disposed)
                    return;
                Free();
                disposed = true;
            }

            #endregion

            #region Internal Methods

            internal void InvalidateImages()
            {
                // This cancels all tasks and disposes every generating resources
                Free();
                Debug.Assert(generateDefaultImageTask?.IsCanceled != false && generateResizedImageTask?.IsCanceled != false);

                // (Re-)enabling generating images
                enabled = true;
            }

            internal void InvalidateDisplayImage()
            {
                // Just canceling possible running generate. Not even clearing the possible already generated image.
                // A new task will be started if a new paint explicitly requires it, in which case the last image can be re-used if possible.
                CancelRunningGenerate(generateResizedImageTask);
            }

            internal (Image?, InterpolationMode) GetDisplayImage()
            {
                Debug.Assert(owner.image != null);

                // When turning on AllowUnsafeCooperativeLocking, we don't free the possibly existing clone immediately, so we do it here if there is no running resize task.
                if (owner.AllowUnsafeCooperativeLocking && generateResizedImageTask == null)
                {
                    origImageClone?.Dispose();
                    origImageClone = null;
                }

                InterpolationMode interpolationMode = InterpolationMode.NearestNeighbor;
                bool smoothing = owner.flags[smoothingEnabled];

                // 1.) Returning with a size adjusted display image
                if (smoothing && resizedDisplayImageSize == owner.targetRectangle.Size)
                    return (resizedDisplayImage, interpolationMode);

                // 2.) Checking if there is an already available default image. It might have to be resized on painting.
                Image? result = defaultDisplayImage;

                // 3.) Starting to generate cached images if needed
                if (enabled)
                {
                    if (result == null)
                        BeginGenerateDefaultDisplayImageIfNeeded();

                    BeginGenerateResizedDisplayImageIfNeeded();
                }

                // Smoothing Bitmap: leaving NearestNeighbor if a resized image is expected to be generated;
                // otherwise, using some interpolation to be applied during painting
                if (!owner.flags[isMetafile] && smoothing)
                {
                    float zoom = owner.zoom;
                    Size size = owner.imageSize;

                    // >4x zoom or shrunk image that is not greater than generating threshold: using HighQualityBicubic 
                    if (zoom >= 4f || zoom < 1f && size.Width <= minBitmapSizeThreshold && size.Height <= minBitmapSizeThreshold)
                        interpolationMode = InterpolationMode.HighQualityBicubic;
                    // 1-4x zoom: HighQualityBilinear for large images to prevent heavy lagging; otherwise, HighQualityBicubic
                    else if (zoom > 1f)
                        interpolationMode = size.Width > minBitmapSizeThreshold || size.Height > minBitmapSizeThreshold ? InterpolationMode.HighQualityBilinear : InterpolationMode.HighQualityBicubic;
                    // Shrinking larger images if generating is disabled: applying a hopefully-not-too-slow fallback interpolation
                    else if (!GenerateResizedBitmap && zoom < 1f)
                        interpolationMode = owner.targetRectangle.Width > minBitmapSizeThreshold || owner.targetRectangle.Height > minBitmapSizeThreshold ? InterpolationMode.Bilinear : InterpolationMode.Bicubic;
                }

                // 4.) Returning either a generated display or the original image
                if (result != null)
                    // here we already have a default display image we can return with
                    return (result, interpolationMode);

                // Too low memory: turning off image generation and freeing up resources.
                if (!enabled)
                {
                    Free();

                    // Assigning by original image to defaultDisplayImage so even large >= 48bpp images will be drawn directly.
                    defaultDisplayImage = owner.image;
                }

                // Unless a default image has been generated in the meantime we return with the original image, or null, if its pixel format is not supported.
                result = defaultDisplayImage ?? owner.image;
                if (ReferenceEquals(result, owner.image) && owner.pixelFormat.In(unsupportedFormats))
                    result = null;

                return (result, interpolationMode);
            }

            // returns true if there were pending tasks that were canceled
            internal bool CancelPendingTasks()
            {
                bool result = generateDefaultImageTask != null || generateResizedImageTask != null;
                if (!result)
                    return result;

                CancelRunningGenerate(generateDefaultImageTask);
                CancelRunningGenerate(generateResizedImageTask);
                WaitForPendingGenerate(generateDefaultImageTask);
                WaitForPendingGenerate(generateResizedImageTask);
                return result;
            }

            #endregion

            #region Private Methods

            private void Free()
            {
                // disabling to prevent starting new tasks while freeing resources
                enabled = false;
                CancelRunningGenerate(generateDefaultImageTask);
                CancelRunningGenerate(generateResizedImageTask);

                WaitForPendingGenerate(generateDefaultImageTask);
                WaitForPendingGenerate(generateResizedImageTask);

                requestedSize = default;
                Volatile.Write(ref maxMetafileSizeThreshold, Int64.MaxValue);

                lock (SyncRoot)
                {
                    if (isDefaultImageCloned)
                        defaultDisplayImage?.Dispose();
                    defaultDisplayImage = null;
                    isDefaultImageCloned = false;

                    resizedDisplayImageSize = default;
                    resizedDisplayImage?.Dispose();
                    resizedDisplayImage = null;

                    origImageClone?.Dispose();
                    origImageClone = null;
                }
            }

            private void BeginGenerateDefaultDisplayImageIfNeeded()
            {
                Debug.Assert(owner.image != null && owner.pixelFormat != default);

                // A task is already running or the display image is already generated.
                if (isDefaultImageCloned || generateDefaultImageTask != null)
                    return;

                Image image = owner.image!;
                Bitmap? bitmap = image as Bitmap;

                // Metafile: The default image is the same as the original. If anti-aliased images are required, a clone is created on demand from that task
                // Bitmap: generating a new default image for unsupported formats,
                bool isGenerateNeeded = bitmap != null && (owner.pixelFormat.In(unsupportedFormats)
                    // for non-PARGB32 images larger than 256x256 - note: leaving even slow formats unconverted below sizeThreshold / 4
                    || owner.pixelFormat != PixelFormat.Format32bppPArgb && (owner.imageSize.Width > minBitmapSizeThreshold >> 2 || owner.imageSize.Height > minBitmapSizeThreshold >> 2)
                    // and for native icons: converting because icons are handled oddly by GDI+, for example, the first column has half pixel width
                    || owner.flags[isIcon]);

                // skipping generating clone if we are running on low memory, and it would only serve performance
                // x4: because we want to convert it to 32bpp
                if (isGenerateNeeded && CheckMemoryUsage && !owner.pixelFormat.In(unsupportedFormats))
                {
                    long memoryPressure = (long)owner.imageSize.Width * owner.imageSize.Height * 4L;
                    if (!MemoryHelper.IsAvailableUnmanaged(memoryPressure))
                    {
                        isGenerateNeeded = false;
                        enabled = false; // disabling generating images as we cannot allocate the memory needed for the default image
                    }
                }

                if (!isGenerateNeeded)
                {
                    // A generated default image is set from another thread so handling possible concurrency.
                    if (defaultDisplayImage == null)
                        Interlocked.CompareExchange(ref defaultDisplayImage, image, null);
                    return;
                }

                var task = new GenerateDefaultImageTask
                {
                    SourceBitmap = bitmap!,
                    InvalidateOwner = owner.flags[isIcon]
                };

                bool operateAsync = owner.AllowUnsafeCooperativeLocking && !owner.IsDesignMode;

                // Forcing sync operation if pixel format is not supported,
                // or it is so slow (>= 48bpp) that it is faster to wait for the converted image than paint the existing one.
                if (owner.pixelFormat.In(unsupportedFormats) || owner.pixelFormat.In(slowFormats))
                    operateAsync = false;

                if (operateAsync)
                {
                    generateDefaultImageTask = task;
                    ThreadPool.QueueUserWorkItem(GenerateDefaultImage!, task);
                }
                else
                    GenerateDefaultImage(task);
            }

            private void BeginGenerateResizedDisplayImageIfNeeded()
            {
                Debug.Assert(owner.image != null && owner.pixelFormat != default);

                Image image = owner.image!;
                Size size = owner.targetRectangle.Size;
                bool metafile = owner.flags[isMetafile];

                // Metafile: If smoothing edges is enabled and the doubled metafile can be drawn fast enough
                // Bitmap: If smoothing resize is enabled, the image is shrunk and image size is larger than 1024x1024
                bool isGenerateNeeded = owner.flags[smoothingEnabled] && (metafile && (long)size.Width * size.Height < Volatile.Read(ref maxMetafileSizeThreshold)
                    || !metafile && owner.zoom < 1f && (owner.imageSize.Width > minBitmapSizeThreshold || owner.imageSize.Height > minBitmapSizeThreshold));

                // Not canceling the possible generate task here. It will call an invalidate in the end, and we can see whether we use the result.
                if (!isGenerateNeeded || size.Width < 1 || size.Height < 1)
                {
                    requestedSize = default;
                    return;
                }

                requestedSize = size;
                GenerateResizedImageTask? task = generateResizedImageTask;
                if (task != null)
                {
                    // If there is already a running generate task
                    if (!task.IsCanceled)
                    {
                        // It is generating the same size: we keep it
                        if (task.Size == size)
                            return;

                        // We just initiate cancellation but not awaiting the completion.
                        task.Cancel();
                    }

                    // We do not await the task (we are in a lock here that is used in the task, too).
                    // Instead, we invalidate the owner so another paint will be triggered some time later. Hopefully the task will have been finished by that time.
                    owner.Invalidate();
                    return;
                }

                Debug.Assert(generateResizedImageTask == null);
                task = new GenerateResizedImageTask { Size = size };

                // If unsafe cooperative locking is allowed, we can use the original image directly
                if (owner.AllowUnsafeCooperativeLocking)
                    task.SourceImage = image;
                else
                {
                    // Turning off optimizations if there is not enough memory to generate a clone bitmap
                    if (CheckMemoryUsage && image is Bitmap)
                    {
                        // Getting stride without locking the bitmap. Assuming stride is always aligned to 4 bytes
                        // (may not be correct for specially constructed bitmaps, but good enough for guessing memory usage).
                        int stride = ((owner.imageSize.Width * owner.pixelFormat.ToBitsPerPixel() + 31) >> 5) << 2;
                        long memoryPressure = (long)stride * owner.imageSize.Height;
                        if (!MemoryHelper.IsAvailableUnmanaged(memoryPressure))
                        {
                            task.Dispose(); // not assigned to a field yet, so we can dispose it safely
                            enabled = false;
                            return;
                        }
                    }

                    // Otherwise, we use a clone of the original image so it can be safely used from another thread.
                    try
                    {
                        // no locking is needed, because AllowUnsafeCooperativeLocking is false here
                        origImageClone ??= image is Bitmap bitmap ? bitmap.CloneCurrentFrame() : (Image)image.Clone();
                    }
                    catch (Exception e) when (!e.IsCriticalGdi())
                    {
                        task.Dispose(); // not assigned to a field yet, so we can dispose it safely
                        enabled = false;
                        return;
                    }                    
                    task.SourceImage = origImageClone;
                }

                generateResizedImageTask = task;
                ThreadPool.QueueUserWorkItem(GenerateResizedImage!, task);
            }

            private void GenerateDefaultImage(object state)
            {
                #region Local Methods

                Bitmap? DoGenerateDefaultImage(GenerateDefaultImageTask task)
                {
                    Size size = owner.imageSize;
                    Bitmap? result = null;
                    try
                    {
                        result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                        using IReadableBitmapData src = task.SourceBitmap.GetReadableBitmapData();
                        using IWritableBitmapData dst = result.GetWritableBitmapData();

                        // here allowing to use max parallelization as the original image is locked anyway
                        var cfg = new ParallelConfig { IsCancelRequestedCallback = () => task.IsCanceled, ThrowIfCanceled = false };

                        // When operating asynchronously, we are already on a pool thread, so the call does not block the UI.
                        src.CopyTo(dst, Point.Empty, null, null, cfg);
                    }
                    catch (Exception e) when (!e.IsCriticalGdi())
                    {
                        // The memory could not be allocated or some other error occurred (yes, we catch even OutOfMemoryException here)
                        // NOTE: practically we always can recover from here: we simply don't use a generated clone and the worker thread can be finished
                        task.Cancel();
                        enabled = false;
                    }
                    finally
                    {
                        if (task.IsCanceled)
                        {
                            result?.Dispose();
                            result = null;
                        }
                    }

                    return result;
                }

                #endregion

                var task = (GenerateDefaultImageTask)state;

                try
                {
                    // canceled, lost race, already disposed, or generating is disabled due to low memory while the original pixel format is supported
                    if (task.IsCanceled || isDefaultImageCloned || task.SourceBitmap != owner.image || disposed || (!enabled && !task.SourceBitmap.PixelFormat.In(unsupportedFormats)))
                        return;

                    Bitmap? result = null;

                    // Locking on the image to avoid the possible "bitmap region is already locked" issue.
                    // Until the default image is generated, it is locked during the paint, too.
                    bool lockOnImage = owner.AllowUnsafeCooperativeLocking;
                    if (lockOnImage)
                        Monitor.Enter(task.SourceBitmap);
                    try
                    {
                        try
                        {
                            // Generating the actual result. IsCanceled might be true if the lock above could not be immediately acquired
                            if (!task.IsCanceled)
                                result = DoGenerateDefaultImage(task);
                        }
                        finally
                        {
                            task.SetCompleted();
                        }
                    }
                    finally
                    {
                        if (lockOnImage)
                            Monitor.Exit(task.SourceBitmap);
                    }

                    if (result == null || task.IsCanceled)
                        return;

                    defaultDisplayImage = result;
                    isDefaultImageCloned = true;

                    // only for icons because otherwise the appearance is the same
                    if (task.InvalidateOwner)
                        owner.Invalidate();
                }
                finally
                {
                    task.Dispose();
                    generateDefaultImageTask = null;
                }
            }

            private void GenerateResizedImage(object state)
            {
                #region Local Methods

                Bitmap? GenerateResizedMetafile(GenerateResizedImageTask task)
                {
                    if (CheckMemoryUsage)
                    {
                        // for the source and resized bitmaps (metafiles always have a 32 bpp pixel format, the target is 32 bpp PARGB)
                        Size doubledSize = new Size(task.Size.Width << 1, task.Size.Height << 1);
                        long unmanagedPressure = (long)doubledSize.Width * doubledSize.Height * 4L + (long)task.Size.Width * task.Size.Height * 4L;

                        // During resizing a large managed buffer of target.Width * source.Height of PColorF (16 bytes) is allocated internally.
                        long managedPressure = (long)task.Size.Width * doubledSize.Height * 16L;

                        if (!MemoryHelper.IsAvailableUnmanaged(unmanagedPressure) || !MemoryHelper.IsAvailableManaged(managedPressure))
                            task.Cancel();
                    }

                    if (task.IsCanceled)
                        return null;

                    // MetafileExtensions.ToBitmap does the same if antialiasing is requested but this way the process can be canceled
                    Bitmap? result = null;
                    Bitmap? doubled = null;
                    try
                    {
                        bool lockOnImage = owner.AllowUnsafeCooperativeLocking;
                        if (lockOnImage)
                            Monitor.Enter(task.SourceImage);
                        try
                        {
                            // NOTE: not using the image drawing constructor here, because it uses bilinear interpolation, which may cause ugly black edges for bitmap drawing records for legacy GDI metafile types.
                            var doubledSize = new Size(task.Size.Width << 1, task.Size.Height << 1);
                            doubled = new Bitmap(doubledSize.Width, doubledSize.Height, PixelFormat.Format32bppPArgb);
                            long timestampStart, timestampEnd;
                            using (var g = Graphics.FromImage(doubled))
                            {
                                // Interpolation mode must always be NN here. Matters when the metafile contains image drawing records, and the metafile type is WMF or EmfOnly.
                                // In this case the enlarged result with interpolation may cause ugly black contours at transparent edges.
                                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                timestampStart = TimeHelper.GetTimeStamp();
                                g.DrawImage(task.SourceImage, new Rectangle(Point.Empty, doubledSize));
                                timestampEnd = TimeHelper.GetTimeStamp();
                            }

                            // Not just the antialiasing, but also the doubling time of a metafile can be really slow, especially if the metafile contains bitmap drawing records.
                            // This is more problematic than the shrinking, because the DrawImage operation uses a process-wide lock, blocking every other drawing operation in other threads as well.
                            // Therefore, limiting the size of this operation even though it is likely still faster than the shrinking afterward.
                            Debug.WriteLine($"Metafile doubling time: {TimeHelper.GetTimeSpan(timestampEnd - timestampStart).TotalMilliseconds:N2} ms");
                            if (timestampEnd - timestampStart > TimeHelper.GetInterval(metafileDoublingTimeThreshold))
                                Volatile.Write(ref maxMetafileSizeThreshold, (long)task.Size.Width * task.Size.Height);
                        }
                        finally
                        {
                            if (lockOnImage)
                                Monitor.Exit(task.SourceImage);
                        }

                        if (!task.IsCanceled)
                        {
                            result = new Bitmap(task.Size.Width, task.Size.Height, PixelFormat.Format32bppPArgb);
                            using IReadableBitmapData src = doubled.GetReadableBitmapData();
                            using IReadWriteBitmapData dst = result.GetReadWriteBitmapData();

                            // As we are already on a pool thread this is not a UI blocking call
                            src.DrawInto(dst,
                                new Rectangle(Point.Empty, doubled.Size),
                                new Rectangle(Point.Empty, task.Size),
                                null, null, default,
                                new ParallelConfig
                                {
                                    IsCancelRequestedCallback = () => task.IsCanceled,
                                    ThrowIfCanceled = false,
                                    MaxDegreeOfParallelism = Math.Max(1, ParallelHelper.CoreCount - 2)
                                });
                        }
                    }
                    catch (Exception e) when (!e.IsCriticalGdi())
                    {
                        // The memory could not be allocated or some other error occurred (yes, we catch even OutOfMemoryException here)
                        // NOTE: practically we always can recover from here: we simply don't use a generated preview and the worker thread can be finished
                        task.Cancel();
                        enabled = false;
                    }
                    finally
                    {
                        doubled?.Dispose();
                        if (task.IsCanceled)
                        {
                            result?.Dispose();
                            result = null;
                        }
                    }

                    return result;
                }

                Bitmap? GenerateResizedBitmap(GenerateResizedImageTask task)
                {
                    if (CheckMemoryUsage)
                    {
                        long unmanagedPressure = (long)task.Size.Width * task.Size.Height * 4L;

                        // During resizing a large managed buffer of target.Width * source.Height of PColorF (16 bytes) is allocated internally.
                        long managedPressure = (long)task.Size.Width * owner.imageSize.Height * 16L;

                        if (!MemoryHelper.IsAvailableUnmanaged(unmanagedPressure) || !MemoryHelper.IsAvailableManaged(managedPressure))
                        {
                            // unlike in GenerateResizedMetafile, here we set enabled to false, so the caller GetDisplayImage can use fallback interpolations
                            task.Cancel();
                            enabled = false;
                        }
                    }

                    if (task.IsCanceled)
                        return null;

                    // BitmapExtensions.Resize does the same but this way the process can be canceled
                    Bitmap? result = null;
                    try
                    {
                        result = new Bitmap(task.Size.Width, task.Size.Height, PixelFormat.Format32bppPArgb);
                        bool lockOnImage = owner.AllowUnsafeCooperativeLocking;
                        if (lockOnImage)
                            Monitor.Enter(task.SourceImage);
                        try
                        {
                            using IReadableBitmapData src = ((Bitmap)task.SourceImage).GetReadableBitmapData();
                            using IReadWriteBitmapData dst = result.GetReadWriteBitmapData();

                            // As we are already on a pool thread this call does not block the UI.
                            src.DrawInto(dst,
                                new Rectangle(Point.Empty, task.SourceImage.Size),
                                new Rectangle(Point.Empty, task.Size),
                                null, null, default,
                                parallelConfig: new AsyncConfig
                                {
                                    IsCancelRequestedCallback = () => task.IsCanceled,
                                    ThrowIfCanceled = false,
                                    MaxDegreeOfParallelism = Math.Max(1, ParallelHelper.CoreCount - 2)
                                });
                        }
                        finally
                        {
                            if (lockOnImage)
                                Monitor.Exit(task.SourceImage);
                        }
                    }
                    catch (Exception e) when (!e.IsCriticalGdi())
                    {
                        // The memory could not be allocated or some other error occurred (yes, we catch even OutOfMemoryException here)
                        // NOTE: practically we always can recover from here: we simply don't use a generated preview and the worker thread can be finished
                        task.Cancel();
                        enabled = false;
                    }
                    finally
                    {
                        if (task.IsCanceled)
                        {
                            result?.Dispose();
                            result = null;
                        }
                    }

                    return result;
                }

                #endregion

                var task = (GenerateResizedImageTask)state;

                try
                {
                    // canceled or lost race
                    if (task.IsCanceled || task.SourceImage != (origImageClone ?? owner.image) || task.Size != requestedSize || !enabled || disposed)
                        return;

                    // returning if we already have the result
                    if (task.Size == resizedDisplayImageSize)
                    {
                        owner.Invalidate();
                        return;
                    }

                    // Before creating the preview releasing previous cached result. It is important to free it here, before checking the free memory.
                    // The lock ensures that no disposed image is displayed
                    lock (SyncRoot)
                    {
                        resizedDisplayImageSize = default;
                        resizedDisplayImage?.Dispose();
                        resizedDisplayImage = null;
                    }

                    // 1.) If there is no cloned display image generating that one first so the UI can use that while the original image will be free to create the resized images from.
                    // Not needed when unsafe cooperative locking is not allowed, because then the task.SourceImage is already a clone of the original image.
                    if (!isDefaultImageCloned && owner.AllowUnsafeCooperativeLocking)
                    {
                        // The clone is just being generated. Invalidating and returning to come back later.
                        if (defaultDisplayImage == null || generateDefaultImageTask != null)
                        {
                            owner.Invalidate();
                            return;
                        }

                        if (CheckMemoryUsage && !owner.pixelFormat.In(unsupportedFormats))
                        {
                            long memoryPressure = (long)owner.imageSize.Width * owner.imageSize.Height * 4L;
                            if (!MemoryHelper.IsAvailableUnmanaged(memoryPressure))
                            {
                                enabled = false;
                                return;
                            }
                        }

                        Debug.Assert(ReferenceEquals(owner.image, defaultDisplayImage), "If isDefaultImageCloned is false, then defaultDisplayImage is expected to be the original instance here.");
                        Debug.Assert(owner.flags[isMetafile] || owner.pixelFormat == PixelFormat.Format32bppPArgb, "Clone is expected to be missing for metafiles and 32bpp PARGB bitmaps only.");
                        Image clone;

                        // This may block the UI in OnPaint but once the clone is created OnPaint will use that instead of the original image.
                        lock (task.SourceImage) // always locking, because AllowUnsafeCooperativeLocking is true here
                        {
                            try
                            {
                                // we do not allow canceling this part because this would be started again and again
                                clone = task.SourceImage is Bitmap bitmap
                                    ? bitmap.ConvertPixelFormat(PixelFormat.Format32bppPArgb)
                                    : (Image)task.SourceImage.Clone();
                            }
                            catch (Exception e) when (!e.IsCriticalGdi())
                            {
                                enabled = false;
                                return;
                            }
                        }

                        defaultDisplayImage = clone;
                        isDefaultImageCloned = true;
                    }

                    if (task.IsCanceled)
                        return;

                    // 2.) Generating the size-adjusted display image
                    Bitmap? result = null;
                    try
                    {
                        if (!task.IsCanceled)
                            result = task.SourceImage is Metafile ? GenerateResizedMetafile(task) : GenerateResizedBitmap(task);
                    }
                    finally
                    {
                        task.SetCompleted();
                    }

                    // setting latest cache (even if the task has been canceled as we have a completed result)
                    if (result != null)
                    {
                        resizedDisplayImage = result;
                        resizedDisplayImageSize = task.Size;
                    }

                    if (task.IsCanceled)
                        return;

                    owner.Invalidate();
                }
                finally
                {
                    task.Dispose();
                    generateResizedImageTask = null;
                }
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion
    }
}