using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaniPay.Accounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BASELINE MIGRATION
            // Bu migration, veritabanında tablolar zaten mevcut olduğu için boş bırakılmıştır.
            // Amaç: EF Core'un şema durumunu takip edebilmesi için migration history'ye kaydın düşmesi.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline olduğu için rollback'te DB objelerine dokunmuyoruz.
        }
    }
}
