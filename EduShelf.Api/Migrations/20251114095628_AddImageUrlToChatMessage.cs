using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ChatMessages",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(8069));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(8073));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(8376));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(8380));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(8521));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 11, 14, 9, 56, 27, 476, DateTimeKind.Utc).AddTicks(9718), "$2a$11$llyF2nLBsVtbIhPezJgN3uyAZXi05xhxg3jX3GOB/R7Ug6kI9jYaq" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 11, 14, 9, 56, 27, 746, DateTimeKind.Utc).AddTicks(6881), "$2a$11$yCp3ldnDqBE25nnPFjQJKe.IRnpeROgNwjX5bSRtRtPzzDHkwut1m" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ChatMessages");

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
    }
}
