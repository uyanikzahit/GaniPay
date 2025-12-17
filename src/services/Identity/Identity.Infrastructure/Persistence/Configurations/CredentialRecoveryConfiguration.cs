using GaniPay.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Identity.Infrastructure.Persistence.Configurations;

public sealed class CredentialRecoveryConfiguration : IEntityTypeConfiguration<CredentialRecovery>
{
    public void Configure(EntityTypeBuilder<CredentialRecovery> builder)
    {
        builder.ToTable("identity_credential_recovery");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CredentialId).HasColumnName("credential_id");

        // ✅ DB'de channel varchar/text ise enum'u STRING olarak sakla/oku
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .IsRequired();

        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.UsedAt).HasColumnName("used_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("ix_identity_credential_recovery_token_hash");
    }
}
