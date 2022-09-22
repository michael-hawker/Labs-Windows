// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.WinUI;

public static partial class FrameworkElementExtensions
{
    public static Task<bool> WaitUntilLoadedAsync(this FrameworkElement element, TaskCreationOptions? options = null)
    {
        if (element.IsLoaded && element.Parent != null)
        {
            return Task.FromResult(true);
        }

        var taskCompletionSource = options.HasValue ? new TaskCompletionSource<bool>(options.Value)
                : new TaskCompletionSource<bool>();
        try
        {
            void LoadedCallback(object sender, RoutedEventArgs args)
            {
                element.Loaded -= LoadedCallback;
                taskCompletionSource.SetResult(true);
            }

            element.Loaded += LoadedCallback;
        }
        catch (Exception e)
        {
            taskCompletionSource.SetException(e);
        }

        return taskCompletionSource.Task;
    }
}
