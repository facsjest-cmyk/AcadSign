using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcadSign.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Epic8_AuditLogsImmutable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    certificate_serial = table.Column<string>(type: "character varying(100)", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_correlation_id",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_document_id",
                table: "audit_logs",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_event_type",
                table: "audit_logs",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.Sql(
                "CREATE OR REPLACE FUNCTION prevent_audit_modification() " +
                "RETURNS TRIGGER AS $$ " +
                "BEGIN " +
                "    RAISE EXCEPTION 'Modification des logs d''audit interdite'; " +
                "END; " +
                "$$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(
                "CREATE TRIGGER audit_logs_immutable " +
                "BEFORE UPDATE OR DELETE ON audit_logs " +
                "FOR EACH ROW EXECUTE FUNCTION prevent_audit_modification();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_logs_immutable ON audit_logs;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_audit_modification();");

            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
