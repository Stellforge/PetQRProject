using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class AppUserService : IAppUserService
{
    private readonly AppDbContext _db;
    public AppUserService(AppDbContext db) => _db = db;

    public async Task<AppUserDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : ToDto(e, hidePassword: true);
    }

    public async Task<IReadOnlyList<AppUserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.AppUsers.AsNoTracking().ToListAsync(ct);
        return list.Select(e => ToDto(e, hidePassword: true)).ToList();
    }

    public async Task<int> CreateAsync(AppUserDto dto, CancellationToken ct = default)
    {
        var e = new AppUser
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Email = dto.Email,
            Phone = dto.Phone,
            Password = dto.Password, 
            Status = dto.Status
        };
        _db.AppUsers.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> UpdateAsync(AppUserDto dto, CancellationToken ct = default)
    {
        var e = await _db.AppUsers.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
        if (e is null) return false;

        e.Name = dto.Name;
        e.Surname = dto.Surname;
        e.Email = dto.Email;
        e.Phone = dto.Phone;
        e.Password = dto.Password; 
        e.Status = dto.Status;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return false;
        _db.AppUsers.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static AppUserDto ToDto(AppUser e, bool hidePassword = false) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Surname = e.Surname,
        Email = e.Email,
        Phone = e.Phone,
        Password = hidePassword ? null : e.Password,
        Status = e.Status,
        CreateDate = e.CreateDate,
        UpdateDate = e.UpdateDate,
        Deleted = e.Deleted
    };
}
