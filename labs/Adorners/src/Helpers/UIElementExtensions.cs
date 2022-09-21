// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Labs.WinUI;

public static class UIElementExtensions
{
    public static async Task OnDependencyPropertyChanged(this UIElement element, DependencyProperty dp, Action<DependencyObject, DependencyProperty> callback, TaskCreationOptions? options = null)
    {
        if (callback is null)
        {
            ThrowArgumentNullException();
        }

        var taskCompletionSource = options.HasValue ? new TaskCompletionSource<bool>(options.Value)
                : new TaskCompletionSource<bool>();

        void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            try
            {
                callback!(sender, dp);
                taskCompletionSource.SetResult(true);
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
        }

        var token = element.RegisterPropertyChangedCallback(dp, DependencyPropertyChangedCallback);

        await taskCompletionSource.Task; // TODO: Is this ok?

        element.UnregisterPropertyChangedCallback(dp, token);

        static void ThrowArgumentNullException() => throw new ArgumentNullException("The parameter \"callback\" must not be null.");
    }
}
