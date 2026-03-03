using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyLimit",
                table: "virtual_cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyLimit",
                table: "virtual_cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    action_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recurring_allowances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: false),
                    next_payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_allowances", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurring_allowances_users_kid_id",
                        column: x => x.kid_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recurring_allowances_users_parent_id",
                        column: x => x.parent_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_date",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_unread",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_recurring_allowances_kid_active",
                table: "recurring_allowances",
                columns: new[] { "kid_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_recurring_allowances_kid_id",
                table: "recurring_allowances",
                column: "kid_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_allowances_parent_id",
                table: "recurring_allowances",
                column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "recurring_allowances");

            migrationBuilder.DropColumn(
                name: "DailyLimit",
                table: "virtual_cards");

            migrationBuilder.DropColumn(
                name: "MonthlyLimit",
                table: "virtual_cards");
        }
    }
}
