using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface ICollarService
{
    Task<IReadOnlyList<CollarDto>> GetByPetAsync(int petId, CancellationToken ct = default);
    Task<int> CreateAsync(CollarDto dto, CancellationToken ct = default);
    Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}