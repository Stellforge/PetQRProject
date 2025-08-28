using Microsoft.EntityFrameworkCore;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services.Abstract;

namespace SimpleProject.Services.Concrete
{
    public class CollarService : ICollarService
    {
        private readonly AppDbContext _db;
        public CollarService(AppDbContext db) => _db = db;

        // ---- Queries ----
        public async Task<CollarDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var e = await _db.Collars
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);

            return e is null ? null : MapToDto(e);
        }

        public async Task<IReadOnlyList<CollarDto>> GetByPetAsync(int petId, CancellationToken ct = default)
        {
            var list = await _db.Collars.AsNoTracking()
                .Where(x => x.PetId == petId && !x.Deleted)
                .ToListAsync(ct);

            return list.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<CollarDto>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        {
            var list = await _db.Collars.AsNoTracking()
                .Where(x => x.SubjectId == subjectId && !x.Deleted)
                .ToListAsync(ct);

            return list.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<CollarDto>> GetByQrAsync(int qrCodeId, CancellationToken ct = default)
        {
            var list = await _db.Collars.AsNoTracking()
                .Where(x => x.QrCodeId == qrCodeId && !x.Deleted)
                .ToListAsync(ct);

            return list.Select(MapToDto).ToList();
        }

        public async Task<int> CreateAsync(CollarDto dto, CancellationToken ct = default)
        {
            var hasPet = dto.PetId.HasValue;
            var hasSubject = dto.SubjectId.HasValue;
            if (hasPet == hasSubject)
                throw new ArgumentException("Either PetId or SubjectId must be provided (but not both).");

            // (opsiyonel) referans varlık doğrulamaları
            if (!await _db.QrCodes.AsNoTracking().AnyAsync(x => x.Id == dto.QrCodeId, ct))
                throw new ArgumentException("QrCode not found.");

            if (hasPet && !await _db.Pets.AsNoTracking().AnyAsync(x => x.Id == dto.PetId!.Value, ct))
                throw new ArgumentException("Pet not found.");

            if (hasSubject && !await _db.Subjects!.AsNoTracking().AnyAsync(x => x.Id == dto.SubjectId!.Value, ct))
                throw new ArgumentException("Subject not found.");

            var e = MapFromDto(dto);
            _db.Collars.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.Id;
        }
        public async Task<bool> UpdateAsync(int id, CollarDto dto, CancellationToken ct = default)
        {
            var e = await _db.Collars.AsTracking().FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
            if (e is null) return false;

            // Sadece temel alanları güncelliyoruz; ilişkileri (Pet/Subject/Qr) değiştireceksen burada kontrollü yap.
            e.SerialNumber = dto.SerialNumber;
            e.IsActive = dto.IsActive;
            e.AssetType = dto.AssetType;
            e.FriendlyName = dto.FriendlyName;

            await _db.SaveChangesAsync(ct);
            return true;
        }
        public async Task<bool> ActivateAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var e = await _db.Collars.AsTracking().FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
            if (e is null) return false;

            e.IsActive = isActive;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var e = await _db.Collars.AsTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e is null) return false;

            e.Deleted = true;
            await _db.SaveChangesAsync(ct);
            return true;
        }
        private static CollarDto MapToDto(Collar x) => new()
        {
            Id = x.Id,
            PetId = x.PetId,
            QrCodeId = x.QrCodeId,
            SerialNumber = x.SerialNumber,
            IsActive = x.IsActive,
            SubjectId = x.SubjectId,
            AssetType = x.AssetType,
            FriendlyName = x.FriendlyName,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Deleted = x.Deleted
        };
        private static Collar MapFromDto(CollarDto dto) => new()
        {
            PetId = dto.PetId,       
            SubjectId = dto.SubjectId,
            QrCodeId = dto.QrCodeId,
            SerialNumber = dto.SerialNumber,
            IsActive = dto.IsActive,
            AssetType = dto.AssetType,
            FriendlyName = dto.FriendlyName
        };
    }
}
