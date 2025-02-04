// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ProjectTemplateExperiment.Samples;

/// <summary>
/// An example sample page of a custom control inheriting from Panel.
/// </summary>
[ToolkitSampleMultiChoiceOption("LayoutOrientation", title: "Orientation", "Horizontal", "Vertical")]

[ToolkitSample(id: nameof(ProjectTemplateCustomSample), "Custom control", description: $"A sample for showing how to create and use a {nameof(ProjectTemplate)} custom control.")]
public sealed partial class ProjectTemplateCustomSample : Page
{
    public ProjectTemplateCustomSample()
    {
        this.InitializeComponent();
    }

    // TODO: See https://github.com/CommunityToolkit/Labs-Windows/issues/149
    public static Orientation ConvertStringToOrientation(string orientation) => orientation switch
    {
        "Vertical" => Orientation.Vertical,
        "Horizontal" => Orientation.Horizontal,
        _ => throw new System.NotImplementedException(),
    };
}
