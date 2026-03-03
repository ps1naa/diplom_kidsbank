using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blocked_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_blocks_spending_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "spending_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_category_blocks_users_blocked_by_id",
                        column: x => x.blocked_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_category_blocks_users_kid_id",
                        column: x => x.kid_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_blocks_blocked_by_id",
                table: "category_blocks",
                column: "blocked_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_blocks_category_id",
                table: "category_blocks",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_blocks_kid_category",
                table: "category_blocks",
                columns: new[] { "kid_id", "category_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_blocks");
        }
    }
}
