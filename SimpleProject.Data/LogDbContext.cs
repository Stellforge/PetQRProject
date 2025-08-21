using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using SimpleProject.Domain.Entities;

namespace SimpleProject.Data;
public sealed class LogDbContext : DbContext
{
    public DbSet<ErrorLog>? ErrorLog { get; set; }
    public DbSet<EntityLog>? EntityLog { get; set; }

    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorLog>().Ignore(a => a.AdminUser);

        modelBuilder.Entity<EntityLog>().Ignore(a => a.AdminUser);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeAsUtcValueConverter>();
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeAsUtcValueConverter>();
    }

    private class DateTimeAsUtcValueConverter() : ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    private class NullableDateTimeAsUtcValueConverter() : ValueConverter<DateTime?, DateTime?>(v => v.HasValue ? v.Value.ToUniversalTime() : null, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
}


