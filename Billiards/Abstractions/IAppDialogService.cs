namespace Billiards.Abstractions;

public interface IAppDialogService
{
    Task ShowMessageAsync(string title, string message, string okText = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText);
    Task<string?> ShowPromptAsync(
        string title,
        string message,
        string acceptText,
        string cancelText,
        string? placeholder = null,
        string? initialValue = null);
    Task<string?> ShowSelectionAsync(string title, IReadOnlyList<string> options, string cancelText);
}
