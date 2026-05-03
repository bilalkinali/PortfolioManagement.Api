using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstrumentIsTracked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTracked",
                table: "Instruments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTracked",
                table: "Instruments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
