using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInstrumentForSecImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssetType",
                table: "Instruments",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Instruments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cik",
                table: "Instruments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Market",
                table: "Instruments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments",
                column: "Cik",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Cik",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Market",
                table: "Instruments");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Instruments",
                newName: "AssetType");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Instruments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
