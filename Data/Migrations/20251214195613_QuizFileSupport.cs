using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn_Earn.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuizFileSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentSubmissionContentType",
                table: "QuizAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentSubmissionFileName",
                table: "QuizAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentSubmissionPath",
                table: "QuizAttempts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "StudentSubmissionContentType",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "StudentSubmissionFileName",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "StudentSubmissionPath",
                table: "QuizAttempts");
        }
    }
}
