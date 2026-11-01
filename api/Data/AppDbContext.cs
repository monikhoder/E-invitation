
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Guest> Guests { get; set; } = null!;

    public DbSet<AdminUser> AdminUsers { get; set; } = null!;

    public DbSet<PaymentSettings> PaymentSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.Username)
                .IsUnique();
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.Property(x => x.InvitationToken)
                .HasMaxLength(43);

            entity.Property(x => x.Wish)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.InvitationToken)
                .IsUnique();

        });

        modelBuilder.Entity<PaymentSettings>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AbaUrl)
                .HasMaxLength(2000);

            entity.Property(x => x.QrImageContentType)
                .HasMaxLength(50);

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();
        });
    }
}
