using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SplitwiseClone.Migrations
{
    /// <inheritdoc />
    public partial class config_group_and_expense_tbls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_expenses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_expenses_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_expenses_tbl_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "tbl_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_group_members",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsGroupAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_group_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_group_members_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_group_members_tbl_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "tbl_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_expense_payers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_expense_payers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_expense_payers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_expense_payers_tbl_expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "tbl_expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_expense_splits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AmountOwed = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_expense_splits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_expense_splits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_expense_splits_tbl_expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "tbl_expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expense_payers_ExpenseId",
                table: "tbl_expense_payers",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expense_payers_UserId",
                table: "tbl_expense_payers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expense_splits_ExpenseId",
                table: "tbl_expense_splits",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expense_splits_UserId",
                table: "tbl_expense_splits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expenses_CreatedById",
                table: "tbl_expenses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_expenses_GroupId",
                table: "tbl_expenses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_group_members_GroupId",
                table: "tbl_group_members",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_group_members_UserId",
                table: "tbl_group_members",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_expense_payers");

            migrationBuilder.DropTable(
                name: "tbl_expense_splits");

            migrationBuilder.DropTable(
                name: "tbl_group_members");

            migrationBuilder.DropTable(
                name: "tbl_expenses");

            migrationBuilder.DropTable(
                name: "tbl_groups");
        }
    }
}
