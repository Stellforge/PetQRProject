using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class SubjectService : ISubjectService
{
    private readonly AppDbContext _db;
    public SubjectService(AppDbContext db) => _db = db;

    public async Task<SubjectDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Subjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
        return e is null ? null : new SubjectDto
        {
            Id = e.Id,
            Type = e.Type,
            Name = e.Name,
            FotoUrl = e.FotoUrl,
            Notes = e.Notes,
            CreateDate = e.CreateDate,
            UpdateDate = e.UpdateDate,
            Deleted = e.Deleted
        };
    }

    public async Task<IReadOnlyList<SubjectDto>> SearchAsync(string? type, string? name, CancellationToken ct = default)
    {
        var q = _db.Subjects.AsNoTracking().Where(x => !x.Deleted);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.Type == type);
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(x => x.Name.Contains(name));
        var list = await q.ToListAsync(ct);
        return list.Select(x => new SubjectDto
        {
            Id = x.Id,
            Type = x.Type,
            Name = x.Name,
            FotoUrl = x.FotoUrl,
            Notes = x.Notes,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Deleted = x.Deleted
        }).ToList();
    }

    public async Task<int> CreateAsync(SubjectDto dto, CancellationToken ct = default)
    {
        var e = new Subject { Type = dto.Type, Name = dto.Name, FotoUrl = dto.FotoUrl, Notes = dto.Notes };
        _db.Subjects.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> UpdateAsync(int id, SubjectDto dto, CancellationToken ct = default)
    {
        var e = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
        if (e is null) return false;
        e.Type = dto.Type; e.Name = dto.Name; e.FotoUrl = dto.FotoUrl; e.Notes = dto.Notes; e.UpdateDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
        if (e is null) return false;
        e.Deleted = true; e.UpdateDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
