using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLauncher.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubDefaultBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultBranch",
                table: "GitHubRepositories",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultBranch",
                table: "GitHubRepositories");
        }
    }
}
