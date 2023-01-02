// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ProjectTemplateExperiment.Samples;

[ToolkitSampleBoolOption("IsTextVisible", true, Title = "IsVisible")]
// Single values without a colon are used for both label and value.
// To provide a different label for the value, separate with a colon surrounded by a single space on both sides ("label : value").
[ToolkitSampleMultiChoiceOption("TextSize", "Small : 12", "Normal : 16", "Big : 32", Title = "Text size")]
[ToolkitSampleMultiChoiceOption("TextFontFamily", "Segoe UI", "Arial", "Consolas", Title = "Font family")]
[ToolkitSampleMultiChoiceOption("TextForeground", "Teal : #0ddc8c", "Sand : #e7a676", "Dull green : #5d7577", Title = "Text foreground")]

[ToolkitSample(id: nameof(ProjectTemplateTemplatedSample), "Templated control", description: "A sample for showing how to create and use a templated control.")]
public sealed partial class ProjectTemplateTemplatedSample : Page
{
    public ProjectTemplateTemplatedSample()
    {
        this.InitializeComponent();
    }
}
