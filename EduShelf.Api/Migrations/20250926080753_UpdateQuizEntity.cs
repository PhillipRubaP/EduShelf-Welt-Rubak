using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Documents_DocumentId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_DocumentId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_DocumentId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Flashcards");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Quizzes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8836));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8838));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8893));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8896));

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Title" },
                values: new object[] { new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8919), "Math Quiz" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8743), "$2a$11$hP5Ch.WgPqrLbA8xsDe4vOBfyiP9cQxM8Yt6FWkCo2Z.wX2CgyiP6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8813), "$2a$11$UY8JAY3qb1seKMqc4duzd.ygPIwM.vZ1OCRImtEXfC7tIg2ttTVOS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Flashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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
                columns: new[] { "CreatedAt", "DocumentId" },
                values: new object[] { new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8041), 1 });

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DocumentId" },
                values: new object[] { new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8043), 1 });

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DocumentId" },
                values: new object[] { new DateTime(2025, 9, 5, 8, 9, 12, 626, DateTimeKind.Utc).AddTicks(8065), 2 });

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

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_DocumentId",
                table: "Quizzes",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_DocumentId",
                table: "Flashcards",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_Documents_DocumentId",
                table: "Flashcards",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Documents_DocumentId",
                table: "Quizzes",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
