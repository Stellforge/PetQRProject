using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class FoundReportService : IFoundReportService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifier;
    public FoundReportService(AppDbContext db, INotificationService notifier) { _db = db; _notifier = notifier; }

    public async Task<FoundReportDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.FoundReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<IReadOnlyList<FoundReportDto>> GetByLostReportAsync(int lostReportId, CancellationToken ct = default)
    {
        var list = await _db.FoundReports.AsNoTracking().Where(x => x.LostReportId == lostReportId).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<int> CreateAsync(FoundReportDto dto, CancellationToken ct = default)
    {
        var lr = await _db.LostReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.LostReportId && x.PetId == dto.PetId, ct);
        if (lr is null) throw new InvalidOperationException("Lost report not found for given pet.");

        var e = new FoundReport
        {
            PetId = dto.PetId,
            LostReportId = dto.LostReportId,
            FinderName = dto.FinderName,
            FinderPhone = dto.FinderPhone,
            Message = dto.Message,
            FoundLat = dto.FoundLat,
            FoundLng = dto.FoundLng,
            FoundDateTime = dto.FoundDateTime ?? DateTime.UtcNow,
            FinderPhoto = dto.FinderPhoto,
            IsContactShared = dto.IsContactShared
        };
        _db.FoundReports.Add(e);
        await _db.SaveChangesAsync(ct);

        var title = "Buluntu kaydı oluşturuldu";
        var body = dto.IsContactShared
            ? $"Bulucu: {dto.FinderName} / {dto.FinderPhone}"
            : $"Mesaj: {dto.Message}";
        await _notifier.SendAsync(lr.OwnerId, "FOUND_CREATED", title, body,
            payload: $"{{\"foundReportId\":{e.Id},\"lostReportId\":{e.LostReportId}}}", ct: ct);

        return e.Id;
    }

    private static FoundReportDto ToDto(FoundReport e) => new()
    {
        Id = e.Id,
        PetId = e.PetId,
        LostReportId = e.LostReportId,
        FinderName = e.FinderName,
        FinderPhone = e.FinderPhone,
        Message = e.Message,
        FoundLat = e.FoundLat,
        FoundLng = e.FoundLng,
        FoundDateTime = e.FoundDateTime,
        FinderPhoto = e.FinderPhoto,
        IsContactShared = e.IsContactShared,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
