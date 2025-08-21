using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class PetService : IPetService
{
    private readonly AppDbContext _db;
    public PetService(AppDbContext db) => _db = db;

    public async Task<PetDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Pets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<IReadOnlyList<PetDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
    {
        var list = await _db.Pets.AsNoTracking().Where(x => x.OwnerId == ownerId).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<int> CreateAsync(PetDto dto, CancellationToken ct = default)
    {
        var e = new Pet
        {
            OwnerId = dto.OwnerId,
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            Color = dto.Color,
            Sex = dto.Sex,
            BirthDate = dto.BirthDate,
            PrimaryImage = dto.PrimaryImage,
            Status = dto.Status
        };
        _db.Pets.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> UpdateAsync(PetDto dto, CancellationToken ct = default)
    {
        var e = await _db.Pets.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
        if (e is null) return false;

        e.OwnerId = dto.OwnerId;
        e.Name = dto.Name;
        e.Species = dto.Species;
        e.Breed = dto.Breed;
        e.Color = dto.Color;
        e.Sex = dto.Sex;
        e.BirthDate = dto.BirthDate;
        e.PrimaryImage = dto.PrimaryImage;
        e.Status = dto.Status;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Pets.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _db.Pets.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static PetDto ToDto(Pet e) => new()
    {
        Id = e.Id,
        OwnerId = e.OwnerId,
        Name = e.Name,
        Species = e.Species,
        Breed = e.Breed,
        Color = e.Color,
        Sex = e.Sex,
        BirthDate = e.BirthDate,
        PrimaryImage = e.PrimaryImage,
        Status = e.Status,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
