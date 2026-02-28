using Microsoft.EntityFrameworkCore;
using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.DataAccess.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<WorkerLog> WorkerLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseCurrency).HasMaxLength(3);
            entity.Property(e => e.TargetCurrency).HasMaxLength(3);
            entity.Property(e => e.Rate).HasPrecision(18, 6);
            entity.HasIndex(e => new { e.CreatedAt, e.BaseCurrency, e.TargetCurrency });
        });

        modelBuilder.Entity<WorkerLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LogLevel).HasMaxLength(20);
            entity.Property(e => e.Message).IsRequired();
        });
    }
}
