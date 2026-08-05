using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLauncher.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Projects",
                newName: "ModifiedDateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Projects",
                newName: "CreatedDateTime");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDateTime",
                table: "ProjectStreaks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProjectStreaks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDateTime",
                table: "ProjectStreaks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDateTime",
                table: "GitHubRepositories",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "GitHubRepositories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDateTime",
                table: "GitHubRepositories",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "ProjectStreaks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProjectStreaks");

            migrationBuilder.DropColumn(
                name: "ModifiedDateTime",
                table: "ProjectStreaks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "GitHubRepositories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "GitHubRepositories");

            migrationBuilder.DropColumn(
                name: "ModifiedDateTime",
                table: "GitHubRepositories");

            migrationBuilder.RenameColumn(
                name: "ModifiedDateTime",
                table: "Projects",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDateTime",
                table: "Projects",
                newName: "CreatedAt");
        }
    }
}
