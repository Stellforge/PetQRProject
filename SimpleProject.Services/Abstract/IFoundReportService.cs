using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IFoundReportService
{
    Task<FoundReportDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<FoundReportDto>> GetByLostReportAsync(int lostReportId, CancellationToken ct = default);
    Task<int> CreateAsync(FoundReportDto dto, CancellationToken ct = default);
}