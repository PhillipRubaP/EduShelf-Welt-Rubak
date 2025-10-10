using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeededPasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(8899));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(8906));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(9095));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(9098));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(9278));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 33, 27, 213, DateTimeKind.Utc).AddTicks(237), "$2a$11$Q7kr//TjDHc.LoUTKBg0YOrpy3j8zx/wGd2zPPlXWDHVDA8yiMvwK" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 33, 27, 482, DateTimeKind.Utc).AddTicks(7824), "$2a$11$bJoSn7/88XfaMkTXJyauVOYMo8gfTMI.jRxKHntRm9NnEigWZdbBm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
