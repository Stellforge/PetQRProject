using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;

public class DealerService : IDealerService
{
    private readonly AppDbContext _db;
    public DealerService(AppDbContext db) => _db = db;

    public async Task<int> CreateDealerAsync(DealerDto dto, CancellationToken ct = default)
    {
        var e = new Dealer { Code = dto.Code, Name = dto.Name, Contact = dto.Contact };
        _db.Dealers.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<int> CreateBatchAsync(CodeBatchDto dto, CancellationToken ct = default)
    {
        if (!await _db.Dealers.AnyAsync(d => d.Id == dto.DealerId, ct))
            throw new ArgumentException("Dealer not found");

        var e = new CodeBatch { DealerId = dto.DealerId, BatchCode = dto.BatchCode, Quantity = dto.Quantity, CreatedAt = dto.CreatedAt };
        _db.CodeBatches.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<int> AssignToCollarAsync(CodeAssignmentDto dto, CancellationToken ct = default)
    {
        if (!await _db.CodeBatches.AnyAsync(b => b.Id == dto.BatchId, ct)) throw new ArgumentException("Batch not found");
        if (!await _db.Collars.AnyAsync(c => c.Id == dto.CollarId && !c.Deleted, ct)) throw new ArgumentException("Collar not found");

        var e = new CodeAssignment { BatchId = dto.BatchId, CollarId = dto.CollarId, AssignedAt = dto.AssignedAt };
        _db.CodeAssignments.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }
}
