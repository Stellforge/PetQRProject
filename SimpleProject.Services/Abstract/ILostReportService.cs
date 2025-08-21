using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface ILostReportService
{
    Task<LostReportDto?> GetAsync(int id, CancellationToken ct = default);
    Task<LostReportDto?> GetActiveByPetAsync(int petId, CancellationToken ct = default);
    Task<int> OpenAsync(LostReportDto dto, CancellationToken ct = default);
    Task<bool> CloseAsync(int lostReportId, CancellationToken ct = default);
}