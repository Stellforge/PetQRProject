using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;

namespace SimpleProject.Services.Abstract;
public interface IScanEventService
{
    Task<ScanEventDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ScanEventDto>> GetByLostReportAsync(int lostReportId, CancellationToken ct = default);
    Task<int> RecordAsync(ScanEventDto dto, CancellationToken ct = default);
    Task<(Pet? Pet, LostReport? ActiveLost, AppUser? Owner)> GetPublicByQrAsync(string qrCode, CancellationToken ct = default);
}