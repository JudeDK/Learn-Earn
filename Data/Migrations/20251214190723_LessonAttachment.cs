using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn_Earn.Data.Migrations
{
    /// <inheritdoc />
    public partial class LessonAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Lessons");
        }
    }
}
