using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class PetImageService : IPetImageService
{
    private readonly AppDbContext _db;
    public PetImageService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PetImageDto>> GetByPetAsync(int petId, CancellationToken ct = default)
    {
        var list = await _db.PetImages.AsNoTracking().Where(x => x.PetId == petId).ToListAsync(ct);
        return list.Select(x => new PetImageDto
        {
            Id = x.Id,
            PetId = x.PetId,
            Url = x.Url,
            IsPrimary = x.IsPrimary,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Deleted = x.Deleted
        }).ToList();
    }

    public async Task<int> CreateAsync(PetImageDto dto, CancellationToken ct = default)
    {
        if (dto.IsPrimary)
        {
            var primaries = await _db.PetImages.Where(i => i.PetId == dto.PetId && i.IsPrimary).ToListAsync(ct);
            foreach (var i in primaries) i.IsPrimary = false;
        }

        var e = new PetImage { PetId = dto.PetId, Url = dto.Url, IsPrimary = dto.IsPrimary };
        _db.PetImages.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.PetImages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _db.PetImages.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetPrimaryAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.PetImages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;

        var all = await _db.PetImages.Where(i => i.PetId == e.PetId).ToListAsync(ct);
        foreach (var i in all) i.IsPrimary = (i.Id == id);

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
