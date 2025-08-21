using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IQrCodeService
{
    Task<QrCodeDto?> GetAsync(int id, CancellationToken ct = default);
    Task<QrCodeDto?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<int> CreateAsync(QrCodeDto dto, CancellationToken ct = default);
    Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default);
}