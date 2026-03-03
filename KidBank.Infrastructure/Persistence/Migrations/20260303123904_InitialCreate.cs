using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievement_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    requirement_json = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievement_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "education_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_education_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    options_json = table.Column<string>(type: "text", nullable: false),
                    correct_option_index = table.Column<int>(type: "integer", nullable: false),
                    explanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizzes_education_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "education_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spending_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    is_allowed_for_kids = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spending_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_spending_categories_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    total_xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_activity_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "achievement_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    required_progress = table.Column<int>(type: "integer", nullable: false),
                    is_unlocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    unlocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievement_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_achievement_progress_achievement_definitions_achievement_de~",
                        column: x => x.achievement_definition_id,
                        principalTable: "achievement_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_achievement_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_messages_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "education_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    quizzes_completed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    quizzes_total = table.Column<int>(type: "integer", nullable: false),
                    total_xp_earned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_education_progress_education_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "education_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_education_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "family_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_family_invitations_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_family_invitations_users_used_by_user_id",
                        column: x => x.used_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "money_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    response_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_money_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_money_requests_users_kid_id",
                        column: x => x.kid_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_money_requests_users_parent_id",
                        column: x => x.parent_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    jwt_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spending_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kid_id = table.Column<Guid>(type: "uuid", nullable: false),
                    set_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    period = table.Column<int>(type: "integer", nullable: false),
                    period_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spending_limits", x => x.id);
                    table.ForeignKey(
                        name: "FK_spending_limits_users_kid_id",
                        column: x => x.kid_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_spending_limits_users_set_by_id",
                        column: x => x.set_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "task_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reward_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    proof_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_task_assignments_users_assigned_to_id",
                        column: x => x.assigned_to_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_task_assignments_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    target_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    current_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    target_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlist_goals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_accounts_destination_account_id",
                        column: x => x.destination_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_accounts_source_account_id",
                        column: x => x.source_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "virtual_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    card_holder_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cvv = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_frozen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_virtual_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_virtual_cards_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_user_id",
                table: "accounts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_user_type",
                table: "accounts",
                columns: new[] { "user_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_achievement_definitions_category",
                table: "achievement_definitions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_achievement_definitions_code",
                table: "achievement_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_achievement_progress_achievement_definition_id",
                table: "achievement_progress",
                column: "achievement_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_achievement_progress_user_achievement",
                table: "achievement_progress",
                columns: new[] { "user_id", "achievement_definition_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_achievement_progress_user_id",
                table: "achievement_progress",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_family_date",
                table: "chat_messages",
                columns: new[] { "family_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_family_id",
                table: "chat_messages",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_recipient_id",
                table: "chat_messages",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_sender_id",
                table: "chat_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "ix_education_modules_order",
                table: "education_modules",
                column: "order_index");

            migrationBuilder.CreateIndex(
                name: "ix_education_modules_published",
                table: "education_modules",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "IX_education_progress_module_id",
                table: "education_progress",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_education_progress_user_id",
                table: "education_progress",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_education_progress_user_module",
                table: "education_progress",
                columns: new[] { "user_id", "module_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_family_id",
                table: "family_invitations",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_invitations_token",
                table: "family_invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_used_by_user_id",
                table: "family_invitations",
                column: "used_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_money_requests_kid_id",
                table: "money_requests",
                column: "kid_id");

            migrationBuilder.CreateIndex(
                name: "ix_money_requests_parent_id",
                table: "money_requests",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_money_requests_parent_status",
                table: "money_requests",
                columns: new[] { "parent_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_module_id",
                table: "quizzes",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_active",
                table: "refresh_tokens",
                columns: new[] { "user_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_spending_categories_family_id",
                table: "spending_categories",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_spending_limits_kid_id",
                table: "spending_limits",
                column: "kid_id");

            migrationBuilder.CreateIndex(
                name: "ix_spending_limits_kid_period_active",
                table: "spending_limits",
                columns: new[] { "kid_id", "period", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_spending_limits_set_by_id",
                table: "spending_limits",
                column: "set_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_assigned_status",
                table: "task_assignments",
                columns: new[] { "assigned_to_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_assigned_to_id",
                table: "task_assignments",
                column: "assigned_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_created_by_id",
                table: "task_assignments",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_created_at",
                table: "transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_destination_account_id",
                table: "transactions",
                column: "destination_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_destination_date",
                table: "transactions",
                columns: new[] { "destination_account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_source_account_id",
                table: "transactions",
                column: "source_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_source_date",
                table: "transactions",
                columns: new[] { "source_account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_family_id",
                table: "users",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_family_role",
                table: "users",
                columns: new[] { "family_id", "role" });

            migrationBuilder.CreateIndex(
                name: "ix_virtual_cards_account_id",
                table: "virtual_cards",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_wishlist_goals_user_id",
                table: "wishlist_goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wishlist_goals_user_status",
                table: "wishlist_goals",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "achievement_progress");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "education_progress");

            migrationBuilder.DropTable(
                name: "family_invitations");

            migrationBuilder.DropTable(
                name: "money_requests");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "spending_categories");

            migrationBuilder.DropTable(
                name: "spending_limits");

            migrationBuilder.DropTable(
                name: "task_assignments");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "virtual_cards");

            migrationBuilder.DropTable(
                name: "wishlist_goals");

            migrationBuilder.DropTable(
                name: "achievement_definitions");

            migrationBuilder.DropTable(
                name: "education_modules");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "families");
        }
    }
}
