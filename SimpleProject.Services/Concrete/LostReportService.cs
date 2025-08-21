using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class LostReportService : ILostReportService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifier;
    public LostReportService(AppDbContext db, INotificationService notifier) { _db = db; _notifier = notifier; }

    public async Task<LostReportDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.LostReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<LostReportDto?> GetActiveByPetAsync(int petId, CancellationToken ct = default)
    {
        var e = await _db.LostReports.AsNoTracking().FirstOrDefaultAsync(x => x.PetId == petId && x.IsActive, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<int> OpenAsync(LostReportDto dto, CancellationToken ct = default)
    {
        var exists = await _db.LostReports.AsNoTracking().FirstOrDefaultAsync(x => x.PetId == dto.PetId && x.IsActive, ct);
        if (exists != null) return exists.Id;

        var e = new LostReport
        {
            PetId = dto.PetId,
            OwnerId = dto.OwnerId,
            IsActive = true,
            LostDateTime = dto.LostDateTime ?? DateTime.UtcNow,
            LostLat = dto.LostLat,
            LostLng = dto.LostLng,
            Description = dto.Description,
            Status = dto.Status
        };
        _db.LostReports.Add(e);
        await _db.SaveChangesAsync(ct);

        await _notifier.SendAsync(dto.OwnerId, "LOST_OPENED", "Kayıp ilanı açıldı",
            $"PetId {dto.PetId} için kayıp ilanı açıldı.", payload: $"{{\"lostReportId\":{e.Id}}}", ct: ct);

        return e.Id;
    }

    public async Task<bool> CloseAsync(int lostReportId, CancellationToken ct = default)
    {
        var e = await _db.LostReports.FirstOrDefaultAsync(x => x.Id == lostReportId, ct);
        if (e is null) return false;

        e.IsActive = false;
        e.ResolvedDate = DateTime.UtcNow;
        e.Status = 2;
        await _db.SaveChangesAsync(ct);

        await _notifier.SendAsync(e.OwnerId, "LOST_CLOSED", "Kayıp ilanı kapatıldı",
            $"Kayıp ilanı kapatıldı. (LostReportId: {e.Id})", payload: $"{{\"lostReportId\":{e.Id}}}", ct: ct);

        return true;
    }

    private static LostReportDto ToDto(LostReport e) => new()
    {
        Id = e.Id,
        PetId = e.PetId,
        OwnerId = e.OwnerId,
        IsActive = e.IsActive,
        LostDateTime = e.LostDateTime,
        LostLat = e.LostLat,
        LostLng = e.LostLng,
        Description = e.Description,
        Status = e.Status,
        ResolvedDate = e.ResolvedDate,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
