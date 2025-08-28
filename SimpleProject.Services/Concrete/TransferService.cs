using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete;
public class TransferService : ITransferService
{
    private readonly AppDbContext _db;
    private readonly IOwnershipService _ownership;
    public TransferService(AppDbContext db, IOwnershipService ownership) { _db = db; _ownership = ownership; }

    public async Task<int> CreateTicketAsync(QrTransferTicketDto dto, CancellationToken ct = default)
    {
        // basit doğrulamalar
        if (!await _db.Collars.AnyAsync(c => c.Id == dto.CollarId && !c.Deleted, ct))
            throw new ArgumentException("Collar not found");

        var e = new QrTransferTicket
        {
            CollarId = dto.CollarId,
            FromOwnerUserId = dto.FromOwnerUserId,
            FromDealerId = dto.FromDealerId,
            ToOwnerUserId = dto.ToOwnerUserId,
            Status = dto.Status,          
            Token = dto.Token,            
            ExpiresAt = dto.ExpiresAt,
            CreatedAt = dto.CreatedAt
        };

        _db.QrTransferTickets.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> AcceptAsync(string token, int toOwnerUserId, CancellationToken ct = default)
    {
        var t = await _db.QrTransferTickets.FirstOrDefaultAsync(x => x.Token == token, ct);
        if (t is null || t.Status != "Pending" || t.ExpiresAt < DateTime.UtcNow) return false;

        // sahipliği güncelle
        await _ownership.ActivateAsync(t.CollarId, toOwnerUserId, ct);

        t.Status = "Accepted";
        t.ToOwnerUserId = toOwnerUserId;
        t.UpdateDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(string token, CancellationToken ct = default)
    {
        var t = await _db.QrTransferTickets.FirstOrDefaultAsync(x => x.Token == token, ct);
        if (t is null || t.Status != "Pending") return false;
        t.Status = "Cancelled";
        t.UpdateDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

