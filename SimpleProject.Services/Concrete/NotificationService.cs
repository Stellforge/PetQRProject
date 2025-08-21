using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    public NotificationService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationDto>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var list = await _db.Notifications.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.Id).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<int> CreateAsync(NotificationDto dto, CancellationToken ct = default)
    {
        var e = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Body = dto.Body,
            Payload = dto.Payload,
            IsRead = dto.IsRead,
            SentDate = dto.SentDate ?? DateTime.UtcNow
        };
        _db.Notifications.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> MarkAsReadAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        e.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _db.Notifications.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> SendAsync(int userId, string type, string? title, string? body, string? payload = null, DateTime? sentDate = null, CancellationToken ct = default)
    {
        var dto = new NotificationDto { UserId = userId, Type = type, Title = title, Body = body, Payload = payload, SentDate = sentDate ?? DateTime.UtcNow, IsRead = false };
        return await CreateAsync(dto, ct);
    }

    private static NotificationDto ToDto(Notification e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Type = e.Type,
        Title = e.Title,
        Body = e.Body,
        Payload = e.Payload,
        IsRead = e.IsRead,
        SentDate = e.SentDate,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
