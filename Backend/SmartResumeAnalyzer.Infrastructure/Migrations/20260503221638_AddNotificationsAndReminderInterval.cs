using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartResumeAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAndReminderInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReminderIntervalDays",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderIntervalDays",
                table: "Users");
        }
    }
}
