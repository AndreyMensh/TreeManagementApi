using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TreeManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExceptionJournals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    QueryParameters = table.Column<string>(type: "text", nullable: true),
                    BodyParameters = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionJournals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreeNodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TreeId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreeNodes_TreeNodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TreeNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionJournals_ExceptionType",
                table: "ExceptionJournals",
                column: "ExceptionType");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionJournals_Timestamp",
                table: "ExceptionJournals",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TreeNodes_ParentId",
                table: "TreeNodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeNodes_Path",
                table: "TreeNodes",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_TreeNodes_TreeId",
                table: "TreeNodes",
                column: "TreeId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeNodes_TreeId_Id",
                table: "TreeNodes",
                columns: new[] { "TreeId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExceptionJournals");

            migrationBuilder.DropTable(
                name: "TreeNodes");
        }
    }
}
