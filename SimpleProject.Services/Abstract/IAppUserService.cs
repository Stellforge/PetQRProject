using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IAppUserService
{
    Task<AppUserDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AppUserDto>> GetAllAsync(CancellationToken ct = default);
    Task<int> CreateAsync(AppUserDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(AppUserDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}