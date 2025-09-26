using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDocumentAndTagModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3424));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3428));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3536));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3540));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3591));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3248), "$2a$11$hP5Ch.WgPqrLbA8xsDe4vOBfyiP9cQxM8Yt6FWkCo2Z.wX2CgyiP6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 26, 6, 43, 22, 586, DateTimeKind.Utc).AddTicks(3379), "$2a$11$UY8JAY3qb1seKMqc4duzd.ygPIwM.vZ1OCRImtEXfC7tIg2ttTVOS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(7988));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(7990));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8041));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8043));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8065));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(7896), "placeholder_hash" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(7963), "placeholder_hash" });
        }
    }
}
