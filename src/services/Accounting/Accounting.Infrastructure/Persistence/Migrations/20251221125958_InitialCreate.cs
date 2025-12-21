using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaniPay.Accounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_balance_history_account_account_id",
                table: "account_balance_history");

            migrationBuilder.DropForeignKey(
                name: "FK_accounting_transaction_account_account_id",
                table: "accounting_transaction");

            migrationBuilder.DropIndex(
                name: "IX_account_balance_history_account_id_created_at",
                table: "account_balance_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounting_transaction",
                table: "accounting_transaction");

            migrationBuilder.DropIndex(
                name: "IX_accounting_transaction_account_id_created_at",
                table: "accounting_transaction");

            migrationBuilder.DropIndex(
                name: "IX_accounting_transaction_idempotency_key",
                table: "accounting_transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_account",
                table: "account");

            migrationBuilder.DropIndex(
                name: "IX_account_account_number",
                table: "account");

            migrationBuilder.DropColumn(
                name: "account_number",
                table: "account");

            migrationBuilder.DropColumn(
                name: "iban",
                table: "account");

            migrationBuilder.RenameTable(
                name: "accounting_transaction",
                newName: "accounting_transactions");

            migrationBuilder.RenameTable(
                name: "account",
                newName: "accounts");

            migrationBuilder.RenameColumn(
                name: "direction",
                table: "account_balance_history",
                newName: "Direction");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "account_balance_history",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "account_balance_history",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "account_balance_history",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "operation_type",
                table: "account_balance_history",
                newName: "OperationType");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "account_balance_history",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "change_amount",
                table: "account_balance_history",
                newName: "ChangeAmount");

            migrationBuilder.RenameColumn(
                name: "balance_before",
                table: "account_balance_history",
                newName: "BalanceBefore");

            migrationBuilder.RenameColumn(
                name: "balance_after",
                table: "account_balance_history",
                newName: "BalanceAfter");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "account_balance_history",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "accounting_transactions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "direction",
                table: "accounting_transactions",
                newName: "Direction");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "accounting_transactions",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "accounting_transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "accounting_transactions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "accounting_transactions",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "operation_type",
                table: "accounting_transactions",
                newName: "OperationType");

            migrationBuilder.RenameColumn(
                name: "idempotency_key",
                table: "accounting_transactions",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "accounting_transactions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "accounting_transactions",
                newName: "CorrelationId");

            migrationBuilder.RenameColumn(
                name: "booked_at",
                table: "accounting_transactions",
                newName: "BookedAt");

            migrationBuilder.RenameColumn(
                name: "balance_before",
                table: "accounting_transactions",
                newName: "BalanceBefore");

            migrationBuilder.RenameColumn(
                name: "balance_after",
                table: "accounting_transactions",
                newName: "BalanceAfter");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "accounting_transactions",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "accounts",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "accounts",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "balance",
                table: "accounts",
                newName: "Balance");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "accounts",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "accounts",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_account_customer_id_currency",
                table: "accounts",
                newName: "IX_accounts_CustomerId_Currency");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "account_balance_history",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "OperationType",
                table: "account_balance_history",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "accounting_transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "OperationType",
                table: "accounting_transactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "accounting_transactions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "accounting_transactions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounting_transactions",
                table: "accounting_transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounts",
                table: "accounts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_account_balance_history_AccountId",
                table: "account_balance_history",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_transactions_AccountId_IdempotencyKey",
                table: "accounting_transactions",
                columns: new[] { "AccountId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_account_balance_history_accounts_AccountId",
                table: "account_balance_history",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_accounting_transactions_accounts_AccountId",
                table: "accounting_transactions",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_balance_history_accounts_AccountId",
                table: "account_balance_history");

            migrationBuilder.DropForeignKey(
                name: "FK_accounting_transactions_accounts_AccountId",
                table: "accounting_transactions");

            migrationBuilder.DropIndex(
                name: "IX_account_balance_history_AccountId",
                table: "account_balance_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounts",
                table: "accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_accounting_transactions",
                table: "accounting_transactions");

            migrationBuilder.DropIndex(
                name: "IX_accounting_transactions_AccountId_IdempotencyKey",
                table: "accounting_transactions");

            migrationBuilder.RenameTable(
                name: "accounts",
                newName: "account");

            migrationBuilder.RenameTable(
                name: "accounting_transactions",
                newName: "accounting_transaction");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "account_balance_history",
                newName: "direction");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "account_balance_history",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "account_balance_history",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "account_balance_history",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "OperationType",
                table: "account_balance_history",
                newName: "operation_type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "account_balance_history",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ChangeAmount",
                table: "account_balance_history",
                newName: "change_amount");

            migrationBuilder.RenameColumn(
                name: "BalanceBefore",
                table: "account_balance_history",
                newName: "balance_before");

            migrationBuilder.RenameColumn(
                name: "BalanceAfter",
                table: "account_balance_history",
                newName: "balance_after");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "account_balance_history",
                newName: "account_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "account",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "account",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "account",
                newName: "balance");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "account",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "account",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "account",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_CustomerId_Currency",
                table: "account",
                newName: "IX_account_customer_id_currency");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "accounting_transaction",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "accounting_transaction",
                newName: "direction");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "accounting_transaction",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "accounting_transaction",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "accounting_transaction",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "accounting_transaction",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "OperationType",
                table: "accounting_transaction",
                newName: "operation_type");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "accounting_transaction",
                newName: "idempotency_key");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "accounting_transaction",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                table: "accounting_transaction",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "BookedAt",
                table: "accounting_transaction",
                newName: "booked_at");

            migrationBuilder.RenameColumn(
                name: "BalanceBefore",
                table: "accounting_transaction",
                newName: "balance_before");

            migrationBuilder.RenameColumn(
                name: "BalanceAfter",
                table: "accounting_transaction",
                newName: "balance_after");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "accounting_transaction",
                newName: "account_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "reference_id",
                table: "account_balance_history",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<short>(
                name: "operation_type",
                table: "account_balance_history",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "account_number",
                table: "account",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "iban",
                table: "account",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "reference_id",
                table: "accounting_transaction",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<short>(
                name: "operation_type",
                table: "accounting_transaction",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "idempotency_key",
                table: "accounting_transaction",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "correlation_id",
                table: "accounting_transaction",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_account",
                table: "account",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_accounting_transaction",
                table: "accounting_transaction",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_account_balance_history_account_id_created_at",
                table: "account_balance_history",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_account_account_number",
                table: "account",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounting_transaction_account_id_created_at",
                table: "accounting_transaction",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_transaction_idempotency_key",
                table: "accounting_transaction",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_account_balance_history_account_account_id",
                table: "account_balance_history",
                column: "account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_accounting_transaction_account_account_id",
                table: "accounting_transaction",
                column: "account_id",
                principalTable: "account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
