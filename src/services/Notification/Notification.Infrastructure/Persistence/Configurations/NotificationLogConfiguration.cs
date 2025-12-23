using GaniPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> b)
    {
        b.ToTable("notification_log");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id");

        b.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        b.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.TemplateCode)
            .HasColumnName("template_code")
            .HasMaxLength(100)
            .IsRequired();

        // payload json string (MVP)
        b.Property(x => x.Payload)
            .HasColumnName("payload")
            .IsRequired();

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(x => x.SentAt)
            .HasColumnName("sent_at");

        b.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        b.HasIndex(x => new { x.CustomerId, x.CreatedAt });
        b.HasIndex(x => x.TemplateCode);
    }
}
