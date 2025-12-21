using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaniPay.Accounting.Infrastructure.Persistence.Migrations;

public partial class Baseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Baseline: DB mevcut. Şema değişikliği yok.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Baseline: geri alma yok.
    }
}
