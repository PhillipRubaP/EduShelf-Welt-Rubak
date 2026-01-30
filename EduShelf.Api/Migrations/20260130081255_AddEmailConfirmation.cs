using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                columns: new[] { "CreatedAt", "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 8, 12, 55, 7, DateTimeKind.Utc).AddTicks(1220), null, null, false, "$2a$11$LcH0ebVpJwf7jWgmWekFC.q7/dGH3ZIZnAUZRnMgMJo3JsYRnGEDm" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "IsEmailConfirmed", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 8, 12, 55, 151, DateTimeKind.Utc).AddTicks(177), null, null, false, "$2a$11$RG/2yV/6SmktZl1RX9QMbuIoJO7NgAZtMftM2hQROZLvd/uYQDyGi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(3848));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(3850));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(3972));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(4088));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 7, 24, 15, 713, DateTimeKind.Utc).AddTicks(6576), "$2a$11$Oi86TQvsgDW1PW/51FmvmeBTu2KxvfYlAWrhNsR2qpMrcYBML8k4S" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 30, 7, 24, 15, 837, DateTimeKind.Utc).AddTicks(3029), "$2a$11$vF8RLEGPpP/KEgralMOvveMMUJ3kiShSOVp5oasV4mXr/pTQkMW76" });
        }
    }
}
