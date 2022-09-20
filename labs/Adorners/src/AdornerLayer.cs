// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

    private static void OnXamlPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is FrameworkElement fe)
        {
            if (!fe.IsLoaded || fe.Parent is null)
            {
                fe.Loaded += XamlPropertyFrameworkElement_Loaded;
            }
            else if (args.NewValue is UIElement adorner)
            {
                var layer = GetAdornerLayer(fe);

                if (layer is not null)
                {
                    AttachAdorner(layer, fe, adorner);
                }
            }

            // TODO: Handle removing Adorner
        }
    }

    private static void XamlPropertyFrameworkElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            fe.Loaded -= XamlPropertyFrameworkElement_Loaded;

            var layer = GetAdornerLayer(fe);

            if (layer is not null)
            {
                AttachAdorner(layer, fe, GetXaml(fe));
            }
        }
    }

    public static AdornerLayer? GetAdornerLayer(FrameworkElement adornedElement)
    {
        // 1. Find Adorner Layer for element or top-most element

        // TODO: Is this needed or can the visual tree get us all the way there?
        /*var parent = adornedElement.FindParent<FrameworkElement>((element) =>
        {
            // TODO: Stop in ScrollViewer?
            if (element is AdornerLayer || element.Parent is null)
            {
                return true;
            }

            return false;
        });*/

        FrameworkElement? lastElement = null;
        
        var adornerLayerOrTopMostElement = adornedElement.FindAscendant<FrameworkElement>((element) =>
        {
            lastElement = element;
            // TODO: Stop in ScrollViewer?
            if (element is AdornerLayer)
            {
                return true;
            }
            else
            {
                var child = element.FindChild<AdornerLayer>();

                if (child != null)
                {
                    lastElement = child;
                    return true;
                }
            }

            return false;
        }) ?? lastElement;

        if (lastElement is AdornerLayer)
        {
            adornerLayerOrTopMostElement = lastElement;
        }

        if (adornerLayerOrTopMostElement is AdornerLayer layer)
        {
            return layer;
        }
        else
        {
            // Inject AdornerLayer
            if (adornerLayerOrTopMostElement is ScrollViewer scroller) // TODO: Switch?
            {
                var adornerLayer = new AdornerLayer();

                // Preserve existing content
                var content = scroller.Content as UIElement;

                /*if (content is FrameworkElement fe)
                {
                    void Fe_Unloaded(object sender, RoutedEventArgs e)
                    {
                        fe.Unloaded -= Fe_Unloaded; // TODO: Temp

                        var layerContainer = new Grid();

                        layerContainer.Children.Add(content); // TODO: This also fails...
                        layerContainer.Children.Add(adornerLayer); // We want to add second so it's on top.

                        scroller.Content = layerContainer;
                    }

                    fe.Unloaded += Fe_Unloaded;
                }*/

                /*_ = scroller.OnDependencyPropertyChanged(ScrollViewer.ContentProperty, async (sender, dp) =>
                {
                    await CompositionTargetHelper.ExecuteAfterCompositionRenderingAsync(() =>
                    {
                        var layerContainer = new Grid();

                        layerContainer.Children.Add(content); // TODO: It won't let us disconnect and reconnect this content easily, this is still detecting it attached to another element.
                        layerContainer.Children.Add(adornerLayer); // We want to add second so it's on top.

                        scroller.Content = layerContainer;
                    });
                });*/

                //// scroller.Content = null;
                
                // TODO: Need to understand how to reparent content, above doesn't work...

                return adornerLayer;
            }
            else if (adornerLayerOrTopMostElement is Grid grid) 
            {
                var adornerLayer = new AdornerLayer();

                // TODO: Handle if grid changes?
                Grid.SetRowSpan(adornerLayer, grid.RowDefinitions.Count);
                Grid.SetColumnSpan(adornerLayer, grid.ColumnDefinitions.Count);
                grid.Children.Add(adornerLayer);

                return adornerLayer;
            }
        }

        return null;
    }

    // TODO: Temp helper? Build into 'Adorner' base class?
    private static void AttachAdorner(AdornerLayer layer, FrameworkElement adornedElement, UIElement adorner)
    {
        // Add adorner XAML content to the Adorner Layer

        var coord = layer.CoordinatesTo(adornedElement);

        Canvas.SetLeft(adorner, coord.X);
        Canvas.SetTop(adorner, coord.Y);

        layer.Children.Add(adorner);
    }
}
