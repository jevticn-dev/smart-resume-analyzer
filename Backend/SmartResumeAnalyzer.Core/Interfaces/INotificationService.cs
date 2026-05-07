using SmartResumeAnalyzer.Core.DTOs.Notification;

namespace SmartResumeAnalyzer.Core.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task GenerateNotificationsAsync();
    }
}