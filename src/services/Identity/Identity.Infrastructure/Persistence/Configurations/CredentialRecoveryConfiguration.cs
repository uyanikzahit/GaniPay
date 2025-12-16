using GaniPay.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Identity.Infrastructure.Persistence.Configurations;

public sealed class CredentialRecoveryConfiguration : IEntityTypeConfiguration<CredentialRecovery>
{
    public void Configure(EntityTypeBuilder<CredentialRecovery> b)
    {
        b.ToTable("identity_credential_recovery");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.CredentialId).IsRequired();
        b.Property(x => x.Channel).HasConversion<int>().IsRequired();

        b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.ExpiresAt).IsRequired();
        b.Property(x => x.UsedAt);

        b.Property(x => x.CreatedAt).IsRequired();

        b.HasIndex(x => x.CredentialId);

        b.HasOne<Credential>()
            .WithMany()
            .HasForeignKey(x => x.CredentialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
