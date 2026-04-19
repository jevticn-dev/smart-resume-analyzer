using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartResumeAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyNameToAnalysisLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AnalysisLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AnalysisLogs");
        }
    }
}
