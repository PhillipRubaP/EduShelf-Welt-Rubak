using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizQuestionsAndAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9191));

            migrationBuilder.UpdateData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9194));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9264));

            migrationBuilder.UpdateData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9267));

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "QuizId", "Text" },
                values: new object[] { 1, 1, "What is 2 + 2?" });

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9305));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9061));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 26, 6, 208, DateTimeKind.Utc).AddTicks(9163));

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "IsCorrect", "QuestionId", "Text" },
                values: new object[,]
                {
                    { 1, true, 1, "4" },
                    { 2, false, 1, "3" },
                    { 3, false, 1, "5" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Answers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Answers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Answers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

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
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8919));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8743));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 8, 7, 53, 151, DateTimeKind.Utc).AddTicks(8813));
        }
    }
}
