using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class CollarService : ICollarService
{
    private readonly AppDbContext _db;
    public CollarService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CollarDto>> GetByPetAsync(int petId, CancellationToken ct = default)
    {
        var list = await _db.Collars.AsNoTracking().Where(x => x.PetId == petId).ToListAsync(ct);
        return list.Select(x => new CollarDto
        {
            Id = x.Id,
            PetId = x.PetId,
            QrCodeId = x.QrCodeId,
            SerialNumber = x.SerialNumber,
            IsActive = x.IsActive,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Deleted = x.Deleted
        }).ToList();
    }

    public async Task<int> CreateAsync(CollarDto dto, CancellationToken ct = default)
    {
        var e = new Collar { PetId = dto.PetId, QrCodeId = dto.QrCodeId, SerialNumber = dto.SerialNumber, IsActive = dto.IsActive };
        _db.Collars.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var e = await _db.Collars.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        e.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Collars.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _db.Collars.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
