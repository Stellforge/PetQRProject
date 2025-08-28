using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface ITransferService
{
    Task<int> CreateTicketAsync(QrTransferTicketDto dto, CancellationToken ct = default); // token üretimi dışarıda veya içeride
    Task<bool> AcceptAsync(string token, int toOwnerUserId, CancellationToken ct = default);
    Task<bool> CancelAsync(string token, CancellationToken ct = default);
}

