using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(7128));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(7134));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(7338));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(7343));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(7577));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 10, 13, 52, 8, 456, DateTimeKind.Utc).AddTicks(2844), "$2a$11$074zhP09rGGUPP8iiv5qdO293e4zxCRkvr8YvTe/x6xLBPKIJ1012" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 10, 13, 52, 8, 747, DateTimeKind.Utc).AddTicks(5832), "$2a$11$Yl0Y7iY6OX1WZdTxsrqoEeF/t/cMNopXUf3GTxQPkRnw.ND1iHXaW" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(6411));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(6414));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(6536));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(6539));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(6638));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 9, 44, 18, 242, DateTimeKind.Utc).AddTicks(968), "$2a$11$O42q9YrdN9BfG7i8XAfxD.xtdqam9dDTHDA5itVl1wsGyFCgIjNA." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(5793), "$2a$11$wqNs7tm0Ilp3PX85d7PcT.rEXp93yZtMdW8UZ3nGOvImfyff.7b0e" });
        }
    }
}
