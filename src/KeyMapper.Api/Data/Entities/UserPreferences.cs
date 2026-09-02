namespace KeyMapper.Api.Data.Entities;

public class UserPreferences
{
    public int UserId { get; set; }
    public string Theme { get; set; } = "light";
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool DesktopNotificationsEnabled { get; set; } = true;
    public bool SyncSuccessNotification { get; set; }
    public bool SyncFailureNotification { get; set; } = true;
    public bool AppContextLoggingEnabled { get; set; }
    public string Language { get; set; } = "en";

    public User User { get; set; } = null!;
}
