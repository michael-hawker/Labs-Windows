// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//// Seem to need something like this as Grid doesn't behave properly in measuring for our scenario... (and more than we need anyway).

namespace CommunityToolkit.WinUI.Controls;

/// <summary>
/// A <see cref="StickyPanel"/> provides the majority of its space for its first (or last) child.
/// Any remaining children only get the space they request. This can be useful for when you have a
/// button or element that appears at the end of a row conditionally for instance.
/// You can think of it as a single row/column grid that has one column as Star and the rest as Auto.
/// </summary>
public partial class StickyPanel : Panel
{
    public Orientation Orientation
    {
        get { return (Orientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(StickyPanel), new PropertyMetadata(Orientation.Horizontal));

    public StickyPriority Prioritize
    {
        get { return (StickyPriority)GetValue(PrioritizeProperty); }
        set { SetValue(PrioritizeProperty, value); }
    }
    
    public static readonly DependencyProperty PrioritizeProperty =
        DependencyProperty.Register(nameof(Prioritize), typeof(StickyPriority), typeof(StickyPanel), new PropertyMetadata(StickyPriority.First));

    protected override Size MeasureOverride(Size availableSize)
    {
        // Pretend we have full size in one direction to understand how large our children want to be (like a StackPanel).
        Size childConstraint = (Orientation == Orientation.Vertical)
            ? new Size(availableSize.Width, double.PositiveInfinity)
            : new Size(double.PositiveInfinity, availableSize.Height);

        double size = 0;
        double maxAlternateDimension = 0;

        UIElement? priority = null;

        var elements = Children.Where(static e => e.Visibility == Visibility.Visible);

        if (Prioritize == StickyPriority.First)
        {
            priority = elements.FirstOrDefault();
            elements = elements.Skip(1);
        }
        else
        {
            priority = elements.LastOrDefault();
            elements = elements.Take(elements.Count() - 1);
        }

        priority?.Measure(childConstraint);
        maxAlternateDimension = Math.Max(maxAlternateDimension,
            (Orientation == Orientation.Horizontal
                ? priority?.DesiredSize.Height
                : priority?.DesiredSize.Width)
            ?? 0);

        foreach (UIElement child in elements)
        {
            child.Measure(childConstraint);
            size += Orientation == Orientation.Horizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
            maxAlternateDimension = Math.Max(maxAlternateDimension,
                Orientation == Orientation.Horizontal
                    ? child.DesiredSize.Height
                    : child.DesiredSize.Width);
        }

        if (Orientation == Orientation.Horizontal)
        {
            return new Size(size + priority?.DesiredSize.Width ?? 0, maxAlternateDimension);
        }
        else
        {
            return new Size(maxAlternateDimension, size + priority?.DesiredSize.Height ?? 0);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double fixedSize = 0;
        UIElement? priority = null;

        // Figure out which elements we should look at
        var elements = Children.Where(static e => e.Visibility == Visibility.Visible);

        if (Prioritize == StickyPriority.First)
        {
            priority = elements.FirstOrDefault();
            elements = elements.Skip(1);
        }
        else
        {
            priority = elements.LastOrDefault();
            elements = elements.Take(elements.Count() - 1);
        }

        // Get the size of all our other elements
        if (Orientation == Orientation.Horizontal)
        {
            fixedSize = elements.Sum(static e => e.DesiredSize.Width);
        }
        else
        {
            fixedSize = elements.Sum(static e => e.DesiredSize.Height);
        }

        double pos = 0;
        double size = 0;

        if (Orientation == Orientation.Horizontal)
        {
            // Layout the first one with the most space
            if (Prioritize == StickyPriority.First)
            {
                size = finalSize.Width - fixedSize;
                priority?.Arrange(new Rect(pos, 0, size, finalSize.Height));
                pos += size;
            }

            // Layout rest of elements
            foreach (var child in elements)
            {
                size = child.DesiredSize.Width;
                child.Arrange(new Rect(pos, 0, size, finalSize.Height));
                pos += size;
            }

            // Layout last one with most space, if in that mode
            if (Prioritize == StickyPriority.Last)
            {
                priority?.Arrange(new Rect(pos, 0, finalSize.Width - fixedSize, finalSize.Height));
            }
        }
        else
        {
            // Layout the first one with the most space
            if (Prioritize == StickyPriority.First)
            {
                size = finalSize.Height - fixedSize;
                priority?.Arrange(new Rect(0, pos, finalSize.Width, size));
                pos += size;
            }

            // Layout rest of elements
            foreach (var child in elements)
            {
                size = child.DesiredSize.Height;
                child.Arrange(new Rect(0, pos, finalSize.Width, size));
                pos += size;
            }

            // Layout last one with most space, if in that mode
            if (Prioritize == StickyPriority.Last)
            {
                priority?.Arrange(new Rect(pos, 0, finalSize.Width, finalSize.Height - fixedSize));
            }
        }

        return finalSize;
    }
}

public enum StickyPriority
{
    First,
    Last
}
