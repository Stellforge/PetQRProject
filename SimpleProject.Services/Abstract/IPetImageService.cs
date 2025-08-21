using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IPetImageService
{
    Task<IReadOnlyList<PetImageDto>> GetByPetAsync(int petId, CancellationToken ct = default);
    Task<int> CreateAsync(PetImageDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> SetPrimaryAsync(int id, CancellationToken ct = default);
}
