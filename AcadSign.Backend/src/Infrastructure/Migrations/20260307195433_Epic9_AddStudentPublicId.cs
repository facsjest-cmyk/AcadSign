using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadSign.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Epic9_AddStudentPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Students",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_Students_PublicId",
                table: "Students",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_PublicId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Students");
        }
    }
}
