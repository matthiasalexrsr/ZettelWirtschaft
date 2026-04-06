namespace DocuDesk.Desktop.Services;

public interface IUserNotificationService
{
    void ShowInfo(string message, string title);

    void ShowWarning(string message, string title);
}
