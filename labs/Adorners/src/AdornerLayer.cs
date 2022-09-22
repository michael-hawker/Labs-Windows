// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI;

#if WINAPPSDK
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Helpers;
#else
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI.WebUI;
#endif

namespace CommunityToolkit.Labs.WinUI;

/// <summary>
/// An adornment layer which can hold content to show on top of other components. If none is specified, one will be injected into your app content for you.
/// </summary>
public partial class AdornerLayer : Canvas
{
    public static UIElement GetXaml(FrameworkElement obj)
    {
        return (UIElement)obj.GetValue(XamlProperty);
    }

    public static void SetXaml(FrameworkElement obj, UIElement value)
    {
        obj.SetValue(XamlProperty, value);
    }

    public static readonly DependencyProperty XamlProperty =
        DependencyProperty.RegisterAttached("Xaml", typeof(UIElement), typeof(AdornerLayer), new PropertyMetadata(null, OnXamlPropertyChanged));

    private static async void OnXamlPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is FrameworkElement fe)
        {
            if (!fe.IsLoaded || fe.Parent is null)
            {
                fe.Loaded += XamlPropertyFrameworkElement_Loaded;
            }
            else if (args.NewValue is UIElement adorner)
            {
                var layer = await GetAdornerLayerAsync(fe);

                if (layer is not null)
                {
                    AttachAdorner(layer, fe, adorner);
                }
            }

            // TODO: Handle removing Adorner
        }
    }

    private static async void XamlPropertyFrameworkElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            fe.Loaded -= XamlPropertyFrameworkElement_Loaded;

            var layer = await GetAdornerLayerAsync(fe);

            if (layer is not null)
            {
                AttachAdorner(layer, fe, GetXaml(fe));
            }
        }
    }

    /// <summary>
    /// Retrieves the closest (or creates an) <see cref="AdornerLayer"/> for the given element. If awaited, the retrieved adorner layer is guaranteed to be loaded. This is to assist adorners with being able to be positioned in relation to the loaded element.
    /// There may be multiple <see cref="AdornerLayer"/>s within an application, as each <see cref="ScrollViewer"/> should have one to enable relational scrolling along content that may be outside of the viewport.
    /// </summary>
    /// <param name="adornedElement">Element to adorn.</param>
    /// <returns>Loaded <see cref="AdornerLayer"/> responsible for that element.</returns>
    public static async Task<AdornerLayer?> GetAdornerLayerAsync(FrameworkElement adornedElement)
    {
        // 1. Find Adorner Layer for element or top-most element
        FrameworkElement? lastElement = null;
        
        var adornerLayerOrTopMostElement = adornedElement.FindAscendant<FrameworkElement>((element) =>
        {
            lastElement = element;
            if (element is AdornerLayer) // TODO: Search also for new AdornerDecorator panel...
            {
                return true;
            }
            else if (element is ScrollViewer scoller)
            {
                return true;
                // TODO:
                //   1. Look down for ScrollContentPresenter (return that)
                //   2. Below where Grid code is now... Remove Content
                //   3. Add 'AdornerDecorator' Panel (simple grid-ish thing)
                //   4. Set content of AdornerDecorator to previously removed content
                //   5. Return adorner layer of decorator.
            }
            // TODO: Need to figure out porting new DO toolkit helpers to Uno, only needed for custom adorner layer placement...
            /*else
            {
                // TODO: Use BreadthFirst Search w/ Depth Limited?
                var child = element.FindFirstLevelDescendants<AdornerLayer>();

                if (child != null)
                {
                    lastElement = child;
                    return true;
                }
            }*/

            return false;
        }) ?? lastElement;

        // Check cases where we may have found a child that we want to use instead of the element returned by search.
        if (lastElement is AdornerLayer)
        {
            adornerLayerOrTopMostElement = lastElement;
        }

        if (adornerLayerOrTopMostElement is AdornerLayer layer)
        {
            await layer.WaitUntilLoadedAsync();

            // If we just have an adorner layer now, we're done!
            return layer;
        }
        else
        {
            // TODO: Windows.UI.Xaml.Internal.RootScrollViewer is a maybe different and what was causing issues before I looked for ScrollViewers along the way?
            // It's an internal unexposed type, so maybe it inherits from ScrollViewer? Not sure yet, but might need to detect and
            // do something different here?

            // ScrollViewers need AdornerLayers so they can provide adorners that scroll with the adorned elements (as it worked in WPF).
            // Note: ScrollViewers and the Window were the main AdornerLayer integration points in WPF.
            if (adornerLayerOrTopMostElement is ScrollViewer scroller)
            {
                var adornerLayer = new AdornerLayer();

                var content = scroller.Content as FrameworkElement;
                scroller.Content = null;

                var layerContainer = new Grid()
                {
                    Children = { content, adornerLayer } // Adorner last so it's 'on top'
                };

                scroller.Content = layerContainer;

                await adornerLayer.WaitUntilLoadedAsync();

                return adornerLayer;
            }
            // Grid seems like the easiest place for us to inject AdornerLayers automatically at the top-level (if needed) - not sure how common this will be?
            else if (adornerLayerOrTopMostElement is Grid grid) 
            {
                var adornerLayer = new AdornerLayer();

                // TODO: Handle if grid row/columns change.
                Grid.SetRowSpan(adornerLayer, grid.RowDefinitions.Count);
                Grid.SetColumnSpan(adornerLayer, grid.ColumnDefinitions.Count);
                grid.Children.Add(adornerLayer);

                await adornerLayer.WaitUntilLoadedAsync();

                return adornerLayer;
            }
        }

        return null;
    }

    // TODO: Temp helper? Build into 'Adorner' base class?
    private static void AttachAdorner(AdornerLayer layer, FrameworkElement adornedElement, UIElement adorner)
    {
        // Add adorner XAML content to the Adorner Layer

        var border = new Border()
        {
            Child = adorner,
            Width = adornedElement.ActualWidth, // TODO: Register/tie to size of element better for changes.
            Height = adornedElement.ActualHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var coord = layer.CoordinatesTo(adornedElement);

        Canvas.SetLeft(border, coord.X);
        Canvas.SetTop(border, coord.Y);

        layer.Children.Add(border);
    }
}
