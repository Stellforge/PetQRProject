using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IPetService
{
    Task<PetDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PetDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<int> CreateAsync(PetDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(PetDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
