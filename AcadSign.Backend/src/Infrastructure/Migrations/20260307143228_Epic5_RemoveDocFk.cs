using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadSign.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Epic5_RemoveDocFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchDocuments_Documents_DocumentId1",
                table: "BatchDocuments");

            migrationBuilder.DropIndex(
                name: "IX_BatchDocuments_DocumentId1",
                table: "BatchDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentId1",
                table: "BatchDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentId1",
                table: "BatchDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchDocuments_DocumentId1",
                table: "BatchDocuments",
                column: "DocumentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchDocuments_Documents_DocumentId1",
                table: "BatchDocuments",
                column: "DocumentId1",
                principalTable: "Documents",
                principalColumn: "Id");
        }
    }
}
