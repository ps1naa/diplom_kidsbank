using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTemplatesAndMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "mission_id",
                table: "education_modules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "education_missions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    min_age = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    max_age = table.Column<int>(type: "integer", nullable: false, defaultValue: 18),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_missions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reward_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BYN"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_task_templates_users_parent_id",
                        column: x => x.parent_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_education_modules_mission_id",
                table: "education_modules",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_education_missions_order",
                table: "education_missions",
                column: "order_index");

            migrationBuilder.CreateIndex(
                name: "ix_task_templates_parent_id",
                table: "task_templates",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_education_modules_education_missions_mission_id",
                table: "education_modules",
                column: "mission_id",
                principalTable: "education_missions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_education_modules_education_missions_mission_id",
                table: "education_modules");

            migrationBuilder.DropTable(
                name: "education_missions");

            migrationBuilder.DropTable(
                name: "task_templates");

            migrationBuilder.DropIndex(
                name: "IX_education_modules_mission_id",
                table: "education_modules");

            migrationBuilder.DropColumn(
                name: "mission_id",
                table: "education_modules");
        }
    }
}
