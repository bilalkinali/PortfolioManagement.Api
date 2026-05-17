using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PortfolioManagement.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstrumentId = table.Column<int>(type: "integer", nullable: false),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Cik = table.Column<string>(type: "text", nullable: true),
                    CompositeFigi = table.Column<string>(type: "text", nullable: true),
                    CurrencyName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    HomepageUrl = table.Column<string>(type: "text", nullable: true),
                    ListDate = table.Column<string>(type: "text", nullable: true),
                    Locale = table.Column<string>(type: "text", nullable: true),
                    Market = table.Column<string>(type: "text", nullable: true),
                    MarketCap = table.Column<decimal>(type: "numeric", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PrimaryExchange = table.Column<string>(type: "text", nullable: true),
                    RoundLot = table.Column<long>(type: "bigint", nullable: true),
                    ShareClassFigi = table.Column<string>(type: "text", nullable: true),
                    ShareClassSharesOutstanding = table.Column<long>(type: "bigint", nullable: true),
                    SicCode = table.Column<string>(type: "text", nullable: true),
                    SicDescription = table.Column<string>(type: "text", nullable: true),
                    TickerRoot = table.Column<string>(type: "text", nullable: true),
                    TickerSuffix = table.Column<string>(type: "text", nullable: true),
                    TotalEmployees = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    WeightedSharesOutstanding = table.Column<long>(type: "bigint", nullable: true),
                    AddressLine1 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    DelistedUtc = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockProfiles_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockProfiles_InstrumentId",
                table: "StockProfiles",
                column: "InstrumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockProfiles");
        }
    }
}
