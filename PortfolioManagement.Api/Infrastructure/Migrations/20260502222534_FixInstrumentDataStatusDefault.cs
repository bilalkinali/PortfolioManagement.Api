using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixInstrumentDataStatusDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                 UPDATE "Instruments"
                 SET "DataStatus" = 1
                 WHERE "DataStatus" = 0;
             """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
