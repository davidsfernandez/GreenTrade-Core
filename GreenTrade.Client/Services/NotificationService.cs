namespace GreenTrade.Client.Services;

public class NotificationService
{
    public event Action<string, string>? OnShow;

    public void ShowSuccess(string message) => OnShow?.Invoke(message, "success");
    public void ShowError(string message) => OnShow?.Invoke(message, "danger");
    public void ShowInfo(string message) => OnShow?.Invoke(message, "info");
}
