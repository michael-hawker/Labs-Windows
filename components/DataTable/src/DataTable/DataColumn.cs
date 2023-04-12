// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Labs.WinUI;
using CommunityToolkit.Labs.WinUI.SizerBaseLocal;

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
        if (PART_ColumnSizer != null)
        {
            PART_ColumnSizer.TargetControl = null;
            PART_ColumnSizer.ManipulationCompleted -= this.PART_ColumnSizer_ManipulationCompleted;
        }

        PART_ColumnSizer = GetTemplateChild(nameof(PART_ColumnSizer)) as ContentSizer;

        if (PART_ColumnSizer != null)
        {
            PART_ColumnSizer.TargetControl = this;
            PART_ColumnSizer.ManipulationCompleted += this.PART_ColumnSizer_ManipulationCompleted;
        }

        base.OnApplyTemplate();
    }

    private void PART_ColumnSizer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        var parent = this.FindAscendant<ItemsPresenter>();

        // TODO: Would be nice for Visual Tree helpers to have limit on depth search,
        // as could grab the direct Panel descendant and then search that for DataRow
        // vs. exploring the whole Header content as well (which has a Panel in our case as well...)

        if (parent != null)
        {
            foreach (DataRow row in parent.FindDescendants<DataRow>())
            {
                row.InvalidateArrange();
            }
        }        
    }
}
