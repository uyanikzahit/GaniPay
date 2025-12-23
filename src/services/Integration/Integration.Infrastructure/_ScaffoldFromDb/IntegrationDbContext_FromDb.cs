using System;
using System.Collections.Generic;
using GaniPay.Integration.Infrastructure._ScaffoldFromDb.Models;
using Microsoft.EntityFrameworkCore;

namespace GaniPay.Integration.Infrastructure._ScaffoldFromDb;

public partial class IntegrationDbContext_FromDb : DbContext
{
    public IntegrationDbContext_FromDb(DbContextOptions<IntegrationDbContext_FromDb> options)
        : base(options)
    {
    }

    public virtual DbSet<IntegrationLog> IntegrationLogs { get; set; }

    public virtual DbSet<IntegrationProvider> IntegrationProviders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<IntegrationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("integration_log_pkey");

            entity.ToTable("integration_log");

            entity.HasIndex(e => e.CreatedAt, "ix_integration_log_created_at");

            entity.HasIndex(e => e.ProviderId, "ix_integration_log_provider_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Operation)
                .HasMaxLength(50)
                .HasColumnName("operation");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.RequestPayload).HasColumnName("request_payload");
            entity.Property(e => e.ResponsePayload).HasColumnName("response_payload");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Provider).WithMany(p => p.IntegrationLogs)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_integration_log_provider");
        });

        modelBuilder.Entity<IntegrationProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("integration_provider_pkey");

            entity.ToTable("integration_provider");

            entity.HasIndex(e => e.Code, "integration_provider_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BaseUrl)
                .HasMaxLength(255)
                .HasColumnName("base_url");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
