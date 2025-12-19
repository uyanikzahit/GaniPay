using GaniPay.Expense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaniPay.Expense.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<ExpenseDefinition>
{
    public void Configure(EntityTypeBuilder<ExpenseDefinition> builder)
    {
        builder.ToTable("expense");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");

        builder.Property(x => x.MinAmount).HasColumnName("min_amount").HasColumnType("numeric(18,2)");
        builder.Property(x => x.MaxAmount).HasColumnName("max_amount").HasColumnType("numeric(18,2)");
        builder.Property(x => x.Percent).HasColumnName("percent").HasColumnType("numeric(6,4)");
        builder.Property(x => x.FixedAmount).HasColumnName("fixed_amount").HasColumnType("numeric(18,2)");

        builder.Property(x => x.Currency).HasColumnName("currency").IsRequired();
        builder.Property(x => x.IsVisible).HasColumnName("is_visible").IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
