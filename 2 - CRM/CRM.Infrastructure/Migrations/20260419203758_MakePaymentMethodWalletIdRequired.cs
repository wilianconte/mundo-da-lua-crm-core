using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePaymentMethodWalletIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods");

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletId",
                schema: "crm",
                table: "payment_methods",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods",
                column: "WalletId",
                principalSchema: "crm",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods");

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletId",
                schema: "crm",
                table: "payment_methods",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods",
                column: "WalletId",
                principalSchema: "crm",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
