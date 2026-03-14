using Android.Content;
using Android.Views;
using Android.Widget;
using Billiards.Abstractions;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;

namespace Billiards.Platforms.Android;

public sealed class AndroidAppDialogService : IAppDialogService
{
    public Task ShowMessageAsync(string title, string message, string okText = "OK")
    {
        var tcs = CreateTaskSource<object?>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            var builder = new MaterialAlertDialogBuilder(activity);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(okText, (_, _) => tcs.TrySetResult(null));

            var dialog = builder.Create();
            if (dialog is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            dialog.SetOnCancelListener(new CancelListener(() => tcs.TrySetResult(null)));
            dialog.Show();
        });

        return tcs.Task;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText)
    {
        var tcs = CreateTaskSource<bool>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
            {
                tcs.TrySetResult(false);
                return;
            }

            var builder = new MaterialAlertDialogBuilder(activity);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(acceptText, (_, _) => tcs.TrySetResult(true));
            builder.SetNegativeButton(cancelText, (_, _) => tcs.TrySetResult(false));

            var dialog = builder.Create();
            if (dialog is null)
            {
                tcs.TrySetResult(false);
                return;
            }

            dialog.SetOnCancelListener(new CancelListener(() => tcs.TrySetResult(false)));
            dialog.Show();
        });

        return tcs.Task;
    }

    public Task<string?> ShowPromptAsync(
        string title,
        string message,
        string acceptText,
        string cancelText,
        string? placeholder = null,
        string? initialValue = null)
    {
        var tcs = CreateTaskSource<string?>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            var layout = new LinearLayout(activity)
            {
                Orientation = Orientation.Vertical
            };
            layout.SetPadding(ToPx(activity, 24), ToPx(activity, 8), ToPx(activity, 24), 0);

            var inputLayout = new TextInputLayout(activity)
            {
                Hint = placeholder ?? string.Empty
            };
            inputLayout.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);

            var editText = new TextInputEditText(activity);
            editText.SetSingleLine(true);
            if (!string.IsNullOrWhiteSpace(initialValue))
            {
                editText.Text = initialValue;
                editText.SetSelection(editText.Text?.Length ?? 0);
            }

            inputLayout.AddView(editText);
            layout.AddView(inputLayout);

            var builder = new MaterialAlertDialogBuilder(activity);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetView(layout);
            builder.SetPositiveButton(acceptText, (_, _) => tcs.TrySetResult(editText.Text));
            builder.SetNegativeButton(cancelText, (_, _) => tcs.TrySetResult(null));

            var dialog = builder.Create();
            if (dialog is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            dialog.SetOnShowListener(new ShowListener(() =>
            {
                editText.RequestFocus();
                dialog.Window?.SetSoftInputMode(SoftInput.StateAlwaysVisible);
            }));
            dialog.SetOnCancelListener(new CancelListener(() => tcs.TrySetResult(null)));
            dialog.Show();
        });

        return tcs.Task;
    }

    public Task<string?> ShowSelectionAsync(string title, IReadOnlyList<string> options, string cancelText)
    {
        var tcs = CreateTaskSource<string?>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var activity = Platform.CurrentActivity;
            if (activity is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            var builder = new MaterialAlertDialogBuilder(activity);
            builder.SetTitle(title);
            builder.SetItems(options.ToArray(), (_, args) =>
            {
                if (args.Which >= 0 && args.Which < options.Count)
                {
                    tcs.TrySetResult(options[args.Which]);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            });
            builder.SetNegativeButton(cancelText, (_, _) => tcs.TrySetResult(null));

            var dialog = builder.Create();
            if (dialog is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            dialog.SetOnCancelListener(new CancelListener(() => tcs.TrySetResult(null)));
            dialog.Show();
        });

        return tcs.Task;
    }

    private static TaskCompletionSource<T> CreateTaskSource<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static int ToPx(Context context, int dp) =>
        (int)(dp * (context.Resources?.DisplayMetrics?.Density ?? 1f) + 0.5f);

    private sealed class CancelListener(Action onCancel) : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        public void OnCancel(IDialogInterface? dialog) => onCancel();
    }

    private sealed class ShowListener(Action onShow) : Java.Lang.Object, IDialogInterfaceOnShowListener
    {
        public void OnShow(IDialogInterface? dialog) => onShow();
    }
}
