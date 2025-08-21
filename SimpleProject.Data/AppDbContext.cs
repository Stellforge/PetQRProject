using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using SimpleProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SimpleProject.Domain.Enums;
using SimpleProject.Domain;

namespace SimpleProject.Data;

public class AppDbContext : DbContext
{
    public DbSet<AdminRole>? AdminRole { get; set; }
    public DbSet<AdminUser>? AdminUser { get; set; }
    public DbSet<Brand>? Brand { get; set; }
    public DbSet<CustomField>? CustomField { get; set; }
    public DbSet<CustomFieldOption>? CustomFieldOption { get; set; }
    public DbSet<CustomFieldValue>? CustomFieldValue { get; set; }
    public DbSet<ExcelUpload>? ExcelUpload { get; set; }
    public DbSet<AppUser>? AppUsers { get; set; }
    public DbSet<Pet>? Pets { get; set; }
    public DbSet<PetImage>? PetImages { get; set; }
    public DbSet<QrCode>? QrCodes { get; set; }
    public DbSet<Collar>? Collars { get; set; }
    public DbSet<LostReport>? LostReports { get; set; }
    public DbSet<FoundReport>? FoundReports { get; set; }
    public DbSet<ScanEvent>? ScanEvents { get; set; }
    public DbSet<Notification>? Notifications { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>().Ignore(a => a.EntityLogs);
        modelBuilder.Entity<AdminUser>().Ignore(a => a.ErrorLogs);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("AppUser", "dbo");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Surname).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Password).HasMaxLength(255);
            e.Property(x => x.Status).IsRequired();
        });

        modelBuilder.Entity<Pet>(e =>
        {
            e.ToTable("Pet", "dbo");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Species).HasMaxLength(50);
            e.Property(x => x.Breed).HasMaxLength(100);
            e.Property(x => x.Color).HasMaxLength(50);
            e.Property(x => x.Sex).HasMaxLength(10);
            e.Property(x => x.PrimaryImage).HasMaxLength(500);
            e.Property(x => x.Status).IsRequired();

            e.HasOne(x => x.Owner)
             .WithMany(u => u.Pets)
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PetImage>(e =>
        {
            e.ToTable("PetImage", "dbo");
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
            e.Property(x => x.IsPrimary).IsRequired();

            e.HasOne(x => x.Pet)
             .WithMany(p => p.Images)
             .HasForeignKey(x => x.PetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QrCode>(e =>
        {
            e.ToTable("QrCode", "dbo");
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.Secret).HasMaxLength(100);
            e.Property(x => x.IsActive).IsRequired();
        });

        modelBuilder.Entity<Collar>(e =>
        {
            e.ToTable("Collar", "dbo");
            e.Property(x => x.SerialNumber).HasMaxLength(100);
            e.Property(x => x.IsActive).IsRequired();

            e.HasOne(x => x.Pet)
             .WithMany(p => p.Collars)
             .HasForeignKey(x => x.PetId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.QrCode)
             .WithMany()
             .HasForeignKey(x => x.QrCodeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LostReport>(e =>
        {
            e.ToTable("LostReport", "dbo");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.IsActive).IsRequired();

            e.Property(x => x.LostLat).HasColumnType("decimal(9,6)");
            e.Property(x => x.LostLng).HasColumnType("decimal(9,6)");

            e.HasOne(x => x.Pet)
             .WithMany(p => p.LostReports)
             .HasForeignKey(x => x.PetId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Owner)
             .WithMany(u => u.LostReports)
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FoundReport>(e =>
        {
            e.ToTable("FoundReport", "dbo");
            e.Property(x => x.FinderName).HasMaxLength(100);
            e.Property(x => x.FinderPhone).HasMaxLength(30);
            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.FinderPhoto).HasMaxLength(500);
            e.Property(x => x.FoundLat).HasColumnType("decimal(9,6)");
            e.Property(x => x.FoundLng).HasColumnType("decimal(9,6)");
            e.Property(x => x.IsContactShared).IsRequired();

            e.HasOne(x => x.Pet)
             .WithMany(p => p.FoundReports)
             .HasForeignKey(x => x.PetId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LostReport)
             .WithMany(l => l.FoundReports)
             .HasForeignKey(x => x.LostReportId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScanEvent>(e =>
        {
            e.ToTable("ScanEvent", "dbo");
            e.Property(x => x.IpAddress).HasMaxLength(64);
            e.Property(x => x.UserAgent).HasMaxLength(300);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.FinderPhone).HasMaxLength(30);
            e.Property(x => x.FinderPhoto).HasMaxLength(500);
            e.Property(x => x.ScanLat).HasColumnType("decimal(9,6)");
            e.Property(x => x.ScanLng).HasColumnType("decimal(9,6)");

            e.HasOne(x => x.QrCode)
             .WithMany()
             .HasForeignKey(x => x.QrCodeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Pet)
             .WithMany(p => p.ScanEvents)
             .HasForeignKey(x => x.PetId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.LostReport)
             .WithMany(l => l.ScanEvents)
             .HasForeignKey(x => x.LostReportId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notification", "dbo");
            e.Property(x => x.Type).HasMaxLength(50).IsRequired();
            e.Property(x => x.Title).HasMaxLength(150);
            e.Property(x => x.Body).HasMaxLength(1000);
            e.Property(x => x.IsRead).IsRequired();

            e.HasOne(x => x.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        BaseEntityDefaults(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Conventions.Remove<SqlServerOnDeleteConvention>();

        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeAsUtcValueConverter>();
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeAsUtcValueConverter>();
    }

    private class DateTimeAsUtcValueConverter()
        : ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private class NullableDateTimeAsUtcValueConverter()
        : ValueConverter<DateTime?, DateTime?>(v => v.HasValue ? v.Value.ToUniversalTime() : null, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

    private static void BaseEntityDefaults(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(e => typeof(Entity).IsAssignableFrom(e.ClrType)))
        {
            if (modelBuilder.Entity(entity.Name).Metadata.FindProperty(Consts.Status) != null)
            {
                modelBuilder.Entity(entity.Name)
                    .Property(Consts.Status)
                    .HasDefaultValue((int)Status.ACTIVE);
            }

            modelBuilder.Entity(entity.Name).Property("Deleted").HasDefaultValue(false);
            modelBuilder.Entity(entity.Name).Property("CreateDate").HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity(entity.Name).Property("UpdateDate").HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
