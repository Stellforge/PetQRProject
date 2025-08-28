using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract
{
    public interface ICollarService
    {
        Task<CollarDto?> GetAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CollarDto>> GetByPetAsync(int petId, CancellationToken ct = default);
        Task<IReadOnlyList<CollarDto>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);
        Task<IReadOnlyList<CollarDto>> GetByQrAsync(int qrCodeId, CancellationToken ct = default);
        Task<int> CreateAsync(CollarDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, CollarDto dto, CancellationToken ct = default);
        Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
    }
}
