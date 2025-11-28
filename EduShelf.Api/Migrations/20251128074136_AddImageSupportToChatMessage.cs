using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageSupportToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageDescription",
                table: "ChatMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ChatMessages",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(5388));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(5392));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(5579));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(5583));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(5856));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 11, 28, 7, 41, 35, 481, DateTimeKind.Utc).AddTicks(5992), "$2a$11$bmqys4H6fj.OQ0ftI0V/JO9oCbo5hiyeB13OkuKGVgE1eg7QTCiIG" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 11, 28, 7, 41, 35, 744, DateTimeKind.Utc).AddTicks(4329), "$2a$11$EZvUHeab2PrIfo.VLJ4Kq.u..zxGY4DSAN5Sdp27b0APpXrCtL26C" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageDescription",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ImagePath",
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
