using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;

public class OwnershipService : IOwnershipService
{
    private readonly AppDbContext _db;
    public OwnershipService(AppDbContext db) => _db = db;

    public async Task<QrOwnershipDto?> GetActiveAsync(int collarId, CancellationToken ct = default)
    {
        var e = await _db.QrOwnerships.AsNoTracking().FirstOrDefaultAsync(x => x.CollarId == collarId && x.IsActive && !x.Deleted, ct);
        return e is null ? null : new QrOwnershipDto { Id = e.Id, CollarId = e.CollarId, OwnerUserId = e.OwnerUserId, ActivatedAt = e.ActivatedAt, IsActive = e.IsActive, CreateDate = e.CreateDate, UpdateDate = e.UpdateDate, Deleted = e.Deleted };
    }

    public async Task<int> ActivateAsync(int collarId, int ownerUserId, CancellationToken ct = default)
    {
        var actives = await _db.QrOwnerships.Where(x => x.CollarId == collarId && x.IsActive).ToListAsync(ct);
        foreach (var a in actives) { a.IsActive = false; a.UpdateDate = DateTime.UtcNow; }

        var e = new QrOwnership { CollarId = collarId, OwnerUserId = ownerUserId, ActivatedAt = DateTime.UtcNow, IsActive = true };
        _db.QrOwnerships.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> DeactivateAsync(int collarId, CancellationToken ct = default)
    {
        var list = await _db.QrOwnerships.Where(x => x.CollarId == collarId && x.IsActive).ToListAsync(ct);
        if (list.Count == 0) return false;
        foreach (var e in list) { e.IsActive = false; e.UpdateDate = DateTime.UtcNow; }
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
