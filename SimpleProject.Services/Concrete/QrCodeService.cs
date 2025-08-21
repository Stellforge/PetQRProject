using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class QrCodeService : IQrCodeService
{
    private readonly AppDbContext _db;
    public QrCodeService(AppDbContext db) => _db = db;

    public async Task<QrCodeDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.QrCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<QrCodeDto?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var e = await _db.QrCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<int> CreateAsync(QrCodeDto dto, CancellationToken ct = default)
    {
        var e = new QrCode { Code = dto.Code, Secret = dto.Secret, IsActive = dto.IsActive };
        _db.QrCodes.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var e = await _db.QrCodes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        e.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static QrCodeDto ToDto(QrCode e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Secret = e.Secret,
        IsActive = e.IsActive,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}

public class QrResolver : IQrResolver
{
    private readonly AppDbContext _db;
    public QrResolver(AppDbContext db) => _db = db;

    public async Task<(QrCode? Qr, Pet? Pet)> ResolveAsync(string qrCode, CancellationToken ct = default)
    {
        var qr = await _db.QrCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Code == qrCode && x.IsActive, ct);
        if (qr == null) return (null, null);

        var collar = await _db.Collars.AsNoTracking()
            .Where(c => c.QrCodeId == qr.Id && c.IsActive)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(ct);

        Pet? pet = null;
        if (collar != null)
            pet = await _db.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == collar.PetId, ct);

        return (qr, pet);
    }
}
