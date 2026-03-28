using Billiards.Abstractions;
using Billiards.Utils;

namespace Billiards.Platforms.Android;

public sealed class AndroidAppDialogService : IAppDialogService
{
    public Task ShowMessageAsync(string title, string message, string okText = "OK") =>
        InvokeOnCurrentPageAsync(page => page.DisplayAlertAsync(title, message, okText));

    public Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText) =>
        InvokeOnCurrentPageAsync(page => page.DisplayAlertAsync(title, message, acceptText, cancelText), false);

    public Task<string?> ShowPromptAsync(
        string title,
        string message,
        string acceptText,
        string cancelText,
        string? placeholder = null,
        string? initialValue = null) =>
        InvokeOnCurrentPageAsync(
            page => page.DisplayPromptAsync(
                title,
                message,
                acceptText,
                cancelText,
                placeholder: placeholder,
                initialValue: initialValue));

    public Task<string?> ShowSelectionAsync(string title, IReadOnlyList<string> options, string cancelText) =>
        InvokeOnCurrentPageAsync(
            page => page.DisplayActionSheetAsync(title, cancelText, null, options.ToArray()),
            defaultValue: null);

    private static Task InvokeOnCurrentPageAsync(Func<Page, Task> action) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = PageResolver.CurrentPage;
            if (page is null)
            {
                return;
            }

            await action(page);
        });

    private static Task<T> InvokeOnCurrentPageAsync<T>(Func<Page, Task<T>> action, T defaultValue = default!) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = PageResolver.CurrentPage;
            if (page is null)
            {
                return defaultValue;
            }

            return await action(page);
        });
}
