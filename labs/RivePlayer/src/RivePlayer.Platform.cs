// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Labs.WinUI.Rive;

// This file contains platform-specific customizations of RivePlayer.

#if WINDOWS_WINAPPSDK

using SkiaSharp.Views.Windows;

// WinAppSdk doesn't have SKSwapChainPanel yet.
public partial class RivePlayer : SKXamlCanvas, IDisposable
{
    // SKXamlCanvas doesn't support rendering in a background thread.
    public bool DrawInBackground { get; set; }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        this.PaintNextAnimationFrame(e.Surface, e.Info.Width, e.Info.Height);
    }
#else

// SkiaSharp.Views.UWP is on Uno too.
using SkiaSharp.Views.UWP;

// SKSwapChainPanel performs better than SKXamlCanvas.
public partial class RivePlayer : SKSwapChainPanel, IDisposable
{
    private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        this.PaintNextAnimationFrame(e.Surface, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
    }
#endif

#if HAS_UNO
    // FrameworkElement has a method named Dispose in Uno, which prevents us from implementing
    // IDisposable.Dispose. We just have to wait until the destructor runs.
#else
    public void Dispose()
    {
        this.OnDispose(true);
    }
#endif

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
#if HAS_UNO
        // XamlRoot.IsHostVisible isn't implemented in Uno.
        OnXamlRootChanged(isHostVisible:true);
#else
        this.XamlRoot.Changed += (XamlRoot xamlRoot, XamlRootChangedEventArgs a) =>
        {
            OnXamlRootChanged(xamlRoot.IsHostVisible);
        };
        OnXamlRootChanged(this.XamlRoot.IsHostVisible);
#endif
    }
}

