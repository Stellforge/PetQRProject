using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface IDealerService
{
    Task<int> CreateDealerAsync(DealerDto dto, CancellationToken ct = default);
    Task<int> CreateBatchAsync(CodeBatchDto dto, CancellationToken ct = default);
    Task<int> AssignToCollarAsync(CodeAssignmentDto dto, CancellationToken ct = default);
}
