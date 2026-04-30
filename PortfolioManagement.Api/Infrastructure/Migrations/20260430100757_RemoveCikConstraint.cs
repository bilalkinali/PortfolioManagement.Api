using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCikConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments",
                column: "Cik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Cik",
                table: "Instruments",
                column: "Cik",
                unique: true);
        }
    }
}
