namespace Rememory.WebApi.Dtos.Notifications;

public class SaveNotificationSettingsRequestDto
{
    public string? Email { get; set; }
    public string? TelegramName { get; set; }
    public int PeriodInDays { get; set; }
}