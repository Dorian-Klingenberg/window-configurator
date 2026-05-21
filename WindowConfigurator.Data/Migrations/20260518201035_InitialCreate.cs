using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowConfigurator.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    WebhookCallbackUrl = table.Column<string>(type: "TEXT", nullable: false),
                    AllowedProductLineKeys = table.Column<string>(type: "TEXT", nullable: false),
                    MixedProductLinesAllowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Branding_LogoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Branding_PrimaryColor = table.Column<string>(type: "TEXT", nullable: true),
                    Branding_AccentColor = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuoteSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultProductLineKey = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerEmail = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "TEXT", nullable: true),
                    MagicLinkToken = table.Column<string>(type: "TEXT", nullable: true),
                    MagicLinkExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteSessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguredWindowItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuoteSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    LineItemNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetsEgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProductLineKey = table.Column<string>(type: "TEXT", nullable: false),
                    FrameWidthDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    FrameHeightDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    RoughOpeningWidthDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    RoughOpeningHeightDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    OutsideMeasureWidthDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    OutsideMeasureHeightDecimal = table.Column<decimal>(type: "TEXT", nullable: true),
                    SectionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthoritativePrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    PricingComputedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguredWindowItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguredWindowItems_QuoteSessions_QuoteSessionId",
                        column: x => x.QuoteSessionId,
                        principalTable: "QuoteSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguredWindowItems_QuoteSessionId",
                table: "ConfiguredWindowItems",
                column: "QuoteSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteSessions_TenantId",
                table: "QuoteSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants",
                column: "ApiKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguredWindowItems");

            migrationBuilder.DropTable(
                name: "QuoteSessions");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
