using KeyMapper.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyMapper.Api.Data;

public class KeyMapperDbContext(DbContextOptions<KeyMapperDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table =>
            {
                table.HasTrigger("trg_Users_UpdateTimestamp");
                table.HasTrigger("trg_Users_AuditSuspension");
            });
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.EmailVerificationToken).HasMaxLength(255);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.SuspensionReason).HasMaxLength(500);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(255);
        });

        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.ToTable("UserPreferences");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Theme).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(10).IsRequired();
            entity.HasOne(e => e.User)
                .WithOne(u => u.Preferences)
                .HasForeignKey<UserPreferences>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.RefreshTokenId);
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.DeviceId).HasMaxLength(100);
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.ActorType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TargetType).HasMaxLength(50);
            entity.Property(e => e.TargetId).HasMaxLength(200);
            entity.Property(e => e.IPAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(300);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
