using SimpleProject.Domain.Entities;

namespace SimpleProject.Services.Abstract;
public interface IQrResolver
{
    Task<(QrCode? Qr, Pet? Pet)> ResolveAsync(string qrCode, CancellationToken ct = default);
}