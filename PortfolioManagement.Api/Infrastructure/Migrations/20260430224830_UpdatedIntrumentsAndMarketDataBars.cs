using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedIntrumentsAndMarketDataBars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarketDataBars_InstrumentId_Date",
                table: "MarketDataBars");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_Symbol",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "AdjustedClose",
                table: "MarketDataBars");

            migrationBuilder.AlterColumn<long>(
                name: "Volume",
                table: "MarketDataBars",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Open",
                table: "MarketDataBars",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Low",
                table: "MarketDataBars",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "High",
                table: "MarketDataBars",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Close",
                table: "MarketDataBars",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "MarketDataBars",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProviderSymbol",
                table: "Instruments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataBars_InstrumentId_Period_Date",
                table: "MarketDataBars",
                columns: new[] { "InstrumentId", "Period", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ProviderSymbol",
                table: "Instruments",
                column: "ProviderSymbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Symbol_Market",
                table: "Instruments",
                columns: new[] { "Symbol", "Market" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarketDataBars_InstrumentId_Period_Date",
                table: "MarketDataBars");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_ProviderSymbol",
                table: "Instruments");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_Symbol_Market",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "MarketDataBars");

            migrationBuilder.DropColumn(
                name: "ProviderSymbol",
                table: "Instruments");

            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "Open",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Low",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "High",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Close",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedClose",
                table: "MarketDataBars",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MarketDataBars_InstrumentId_Date",
                table: "MarketDataBars",
                columns: new[] { "InstrumentId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Symbol",
                table: "Instruments",
                column: "Symbol",
                unique: true);
        }
    }
}
