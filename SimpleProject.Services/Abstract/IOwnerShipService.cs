using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IOwnershipService
{
    Task<QrOwnershipDto?> GetActiveAsync(int collarId, CancellationToken ct = default);
    Task<int> ActivateAsync(int collarId, int ownerUserId, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int collarId, CancellationToken ct = default);
}