// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Labs.WinUI;

namespace CommunityToolkit.WinUI.Controls;

[TemplatePart(Name = nameof(PART_ColumnSizer), Type = typeof(ContentSizer))]
public partial class DataColumn : ContentControl
{
    private static GridLength StarLength = new GridLength(1, GridUnitType.Star);

    private ContentSizer? PART_ColumnSizer;

    public GridLength DesiredWidth
    {
        get { return (GridLength)GetValue(DesiredWidthProperty); }
        set { SetValue(DesiredWidthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DesiredWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DesiredWidthProperty =
        DependencyProperty.Register(nameof(DesiredWidth), typeof(GridLength), typeof(DataColumn), new PropertyMetadata(StarLength));

    public DataColumn()
    {
        this.DefaultStyleKey = typeof(DataColumn);
    }

    protected override void OnApplyTemplate()
    {
        PART_ColumnSizer = GetTemplateChild(nameof(PART_ColumnSizer)) as ContentSizer;

        if (PART_ColumnSizer != null)
        {
            PART_ColumnSizer.TargetControl = this;
        }

        base.OnApplyTemplate();
    }
}
