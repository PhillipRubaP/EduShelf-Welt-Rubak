using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardTagSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2699));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2703));

            migrationBuilder.InsertData(
                table: "FlashcardTags",
                columns: new[] { "FlashcardId", "TagId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2817));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2821));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2919));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2500), ".WgPqrLbA8xsDe4vOBfyiP9cQxM8Yt6FWkCo2Z.wX2CgyiP6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 30, 33, 143, DateTimeKind.Utc).AddTicks(2513), ".ygPIwM.vZ1OCRImtEXfC7tIg2ttTVOS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FlashcardTags",
                keyColumns: new[] { "FlashcardId", "TagId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "FlashcardTags",
                keyColumns: new[] { "FlashcardId", "TagId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5696));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5697));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5750));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5752));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5818));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5603), "$2a$11$hP5Ch.WgPqrLbA8xsDe4vOBfyiP9cQxM8Yt6FWkCo2Z.wX2CgyiP6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 3, 10, 6, 27, 261, DateTimeKind.Utc).AddTicks(5605), "$2a$11$UY8JAY3qb1seKMqc4duzd.ygPIwM.vZ1OCRImtEXfC7tIg2ttTVOS" });
        }
    }
}
