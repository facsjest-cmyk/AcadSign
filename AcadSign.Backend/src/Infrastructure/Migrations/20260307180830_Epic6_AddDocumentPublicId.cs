using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadSign.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Epic6_AddDocumentPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PublicId",
                table: "Documents",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_PublicId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Documents");
        }
    }
}
