using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaniPay.TransactionLimit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "limit_definition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MetricType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_limit_definition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "limit_customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LimitDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<short>(type: "smallint", nullable: true),
                    Month = table.Column<short>(type: "smallint", nullable: true),
                    Day = table.Column<short>(type: "smallint", nullable: true),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_limit_customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_limit_customer_limit_definition_LimitDefinitionId",
                        column: x => x.LimitDefinitionId,
                        principalTable: "limit_definition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_limit_customer_LimitDefinitionId",
                table: "limit_customer",
                column: "LimitDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_limit_definition_Code",
                table: "limit_definition",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "limit_customer");

            migrationBuilder.DropTable(
                name: "limit_definition");
        }
    }
}
