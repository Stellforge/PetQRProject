using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<int> CreateAsync(NotificationDto dto, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<int> SendAsync(int userId, string type, string? title, string? body, string? payload = null, DateTime? sentDate = null, CancellationToken ct = default);
}