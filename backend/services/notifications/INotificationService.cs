using backend.dtos.notifications;

namespace backend.Services.Notifications;

public interface INotificationService
{
    Task<List<ProcessedNotificationDto>> GetNotificationsAsync(int businessId, CancellationToken cancellationToken = default);
    Task<List<ProcessedNotificationDto>> ProcessNewsItemsAsync(List<NewsItemDto> items, int businessId, CancellationToken cancellationToken = default);
}
