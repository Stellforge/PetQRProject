using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class ScanEventService : IScanEventService
{
    private readonly AppDbContext _db;
    private readonly IQrResolver _resolver;
    private readonly INotificationService _notifier;
    public ScanEventService(AppDbContext db, IQrResolver resolver, INotificationService notifier)
    { _db = db; _resolver = resolver; _notifier = notifier; }

    public async Task<ScanEventDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.ScanEvents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<IReadOnlyList<ScanEventDto>> GetByLostReportAsync(int lostReportId, CancellationToken ct = default)
    {
        var list = await _db.ScanEvents.AsNoTracking().Where(x => x.LostReportId == lostReportId).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<int> RecordAsync(ScanEventDto dto, CancellationToken ct = default)
    {
        var qr = await _db.QrCodes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == dto.QrCodeId && q.IsActive, ct);
        if (qr == null) throw new InvalidOperationException("QR not found or inactive.");

        var lr = await _db.LostReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.LostReportId, ct);
        if (lr is null) throw new InvalidOperationException("Lost report not found.");

        int? petId = dto.PetId;
        if (!petId.HasValue)
        {
            var collar = await _db.Collars.AsNoTracking()
                .Where(c => c.QrCodeId == qr.Id && c.IsActive)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(ct);
            if (collar != null) petId = collar.PetId;
        }

        var e = new ScanEvent
        {
            QrCodeId = qr.Id,
            PetId = petId,
            LostReportId = lr.Id,
            ResultType = dto.ResultType,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            ScanLat = dto.ScanLat,
            ScanLng = dto.ScanLng,
            Notes = dto.Notes,
            FinderPhone = dto.FinderPhone,
            FinderPhoto = dto.FinderPhoto
        };

        _db.ScanEvents.Add(e);
        await _db.SaveChangesAsync(ct);

        if (petId.HasValue)
        {
            var ownerId = await _db.Pets.AsNoTracking().Where(p => p.Id == petId.Value).Select(p => p.OwnerId).FirstAsync(ct);
            await _notifier.SendAsync(ownerId, "QR_SCANNED", "QR taraması kaydedildi",
                $"Evcil hayvanının QR'ı tarandı. (PetId: {petId})",
                payload: $"{{\"scanEventId\":{e.Id},\"petId\":{petId}}}", ct: ct);
        }

        return e.Id;
    }

    public async Task<(Pet? Pet, LostReport? ActiveLost, AppUser? Owner)> GetPublicByQrAsync(string qrCode, CancellationToken ct = default)
    {
        var (qr, pet) = await _resolver.ResolveAsync(qrCode, ct);
        if (qr == null || pet == null) return (null, null, null);

        var owner = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == pet.OwnerId, ct);
        var activeLost = await _db.LostReports.AsNoTracking().FirstOrDefaultAsync(x => x.PetId == pet.Id && x.IsActive, ct);

        return (pet, activeLost, owner);
    }

    private static ScanEventDto ToDto(ScanEvent e) => new()
    {
        Id = e.Id,
        QrCodeId = e.QrCodeId,
        PetId = e.PetId,
        LostReportId = e.LostReportId,
        ResultType = e.ResultType,
        IpAddress = e.IpAddress,
        UserAgent = e.UserAgent,
        ScanLat = e.ScanLat,
        ScanLng = e.ScanLng,
        Notes = e.Notes,
        FinderPhone = e.FinderPhone,
        FinderPhoto = e.FinderPhoto,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
