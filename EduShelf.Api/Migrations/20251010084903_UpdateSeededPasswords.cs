using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeededPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(1976));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(1981));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(2359));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(2363));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(2664));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 8, 49, 2, 293, DateTimeKind.Utc).AddTicks(3554), "$2a$11$xGnd9duz1xV4XlqVsBRp7OWGNxARF4ClN9NdXNS8V38U7QoJhPDm." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 8, 49, 2, 556, DateTimeKind.Utc).AddTicks(649), "$2a$11$T8Nlzt6Tw1Blg/Xs2w4Yi.MJslszTmAKEMH71JerGlxRHlLThZfCC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(5224));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(5229));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(5476));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(5702));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 8, 39, 29, 866, DateTimeKind.Utc).AddTicks(7403), "$2a$11$GM9SjJiLHPXQeCX.mbTUMeYfdtBk6qi0p/WVvVHfpORtZ/7pYoDOC" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 8, 39, 30, 209, DateTimeKind.Utc).AddTicks(3704), "$2a$11$KSbrB6yv4pufjAHzPxFWWOCAK9lYWsUaTmrYo.hQH1dkD..yTgi9S" });
        }
    }
}
