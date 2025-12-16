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
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.CustomerId).IsRequired();

        b.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256);

        b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.PasswordSalt).HasMaxLength(256).IsRequired();
        b.Property(x => x.PasswordAlgo).HasMaxLength(64).IsRequired();

        b.Property(x => x.FailedLoginCount).IsRequired();

        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.LockoutEndAt);
        b.Property(x => x.LockReason).HasMaxLength(256);
        b.Property(x => x.LastLoginAt);

        b.Property(x => x.PhoneVerifiedAt);
        b.Property(x => x.EmailVerifiedAt);

        b.Property(x => x.RegistrationStatus).HasConversion<int>().IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();

        b.HasIndex(x => x.CustomerId);
        b.HasIndex(x => x.PhoneNumber).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();
    }
}
