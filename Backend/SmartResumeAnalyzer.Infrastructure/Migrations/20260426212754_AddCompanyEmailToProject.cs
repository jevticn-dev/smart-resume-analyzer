using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartResumeAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyEmailToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Projects");
        }
    }
}
