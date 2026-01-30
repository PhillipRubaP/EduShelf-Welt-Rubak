using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmSeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 9, 44, 18, 242, DateTimeKind.Utc).AddTicks(968), true, "$2a$11$O42q9YrdN9BfG7i8XAfxD.xtdqam9dDTHDA5itVl1wsGyFCgIjNA." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 9, 44, 18, 361, DateTimeKind.Utc).AddTicks(5793), true, "$2a$11$wqNs7tm0Ilp3PX85d7PcT.rEXp93yZtMdW8UZ3nGOvImfyff.7b0e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(1099));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(1101));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(1229));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(1232));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(1334));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 8, 12, 55, 7, DateTimeKind.Utc).AddTicks(1220), false, "$2a$11$LcH0ebVpJwf7jWgmWekFC.q7/dGH3ZIZnAUZRnMgMJo3JsYRnGEDm" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(177), false, "$2a$11$RG/2yV/6SmktZl1RX9QMbuIoJO7NgAZtMftM2hQROZLvd/uYQDyGi" });
        }
    }
}
