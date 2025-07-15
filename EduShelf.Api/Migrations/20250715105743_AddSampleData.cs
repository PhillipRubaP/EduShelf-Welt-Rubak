using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduShelf.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "CreatedAt", "FileType", "Path", "Title", "UserId" },
                values: new object[] { 1, new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2188), "pdf", "/documents/algebra.pdf", "Algebra Basics", 1 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2077));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "PasswordHash", "Role", "Username" },
                values: new object[] { 2, new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2153), "student@edushelf.com", "placeholder_hash", "Student", "Student User" });

            migrationBuilder.InsertData(
                table: "DocumentTags",
                columns: new[] { "DocumentId", "TagId", "Id" },
                values: new object[] { 1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "CreatedAt", "FileType", "Path", "Title", "UserId" },
                values: new object[] { 2, new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2190), "pdf", "/documents/physics.pdf", "Introduction to Physics", 2 });

            migrationBuilder.InsertData(
                table: "Flashcards",
                columns: new[] { "Id", "Answer", "CreatedAt", "DocumentId", "Question", "UserId" },
                values: new object[,]
                {
                    { 1, "4", new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2248), 1, "What is 2+2?", 1 },
                    { 2, "5", new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2251), 1, "What is x in x+5=10?", 1 }
                });

            migrationBuilder.InsertData(
                table: "DocumentTags",
                columns: new[] { "DocumentId", "TagId", "Id" },
                values: new object[] { 2, 2, 0 });

            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "CreatedAt", "DocumentId", "Score", "UserId" },
                values: new object[] { 1, new DateTime(2025, 7, 15, 10, 57, 43, 164, DateTimeKind.Utc).AddTicks(2319), 2, 85, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentTags",
                keyColumns: new[] { "DocumentId", "TagId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "DocumentTags",
                keyColumns: new[] { "DocumentId", "TagId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Flashcards",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 15, 10, 24, 31, 503, DateTimeKind.Utc).AddTicks(1389));
        }
    }
}
