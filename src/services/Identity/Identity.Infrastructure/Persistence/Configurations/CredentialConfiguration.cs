using GaniPay.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Identity.Infrastructure.Persistence.Configurations;

public sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> b)
    {
        b.ToTable("identity_credential");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();

        b.Property(x => x.LoginType).HasColumnName("login_type").HasMaxLength(32).IsRequired();
        b.Property(x => x.LoginValue).HasColumnName("login_value").HasMaxLength(256).IsRequired();

        b.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        b.Property(x => x.PasswordSalt).HasColumnName("password_salt").HasMaxLength(512).IsRequired();
        b.Property(x => x.PasswordAlgo).HasColumnName("password_algo").HasMaxLength(64).IsRequired();

        b.Property(x => x.FailedLoginCount).HasColumnName("failed_login_count").IsRequired();
        b.Property(x => x.IsLocked).HasColumnName("is_locked").IsRequired();
        b.Property(x => x.LockReason).HasColumnName("lock_reason").HasMaxLength(512);
        b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        b.HasIndex(x => new { x.LoginType, x.LoginValue }).IsUnique();

        b.HasMany(x => x.Recoveries)
            .WithOne(x => x.Credential!)
            .HasForeignKey(x => x.CredentialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
