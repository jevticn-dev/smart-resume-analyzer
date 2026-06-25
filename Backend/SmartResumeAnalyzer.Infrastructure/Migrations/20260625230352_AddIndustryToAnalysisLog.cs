using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartResumeAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryToAnalysisLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "AnalysisLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Industry",
                table: "AnalysisLogs");
        }
    }
}
