using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedDataStatusOnInstruments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Positions");

            migrationBuilder.AlterColumn<int>(
                name: "InstrumentId",
                table: "Positions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Period",
                table: "MarketDataBars",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DataStatus",
                table: "Instruments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTracked",
                table: "Instruments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "DataStatus",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "IsTracked",
                table: "Instruments");

            migrationBuilder.AlterColumn<int>(
                name: "InstrumentId",
                table: "Positions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Positions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Period",
                table: "MarketDataBars",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id");
        }
    }
}
