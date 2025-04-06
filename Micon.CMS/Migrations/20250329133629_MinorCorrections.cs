using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class MinorCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "Pages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PageDisplayId",
                table: "PageHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "ComponentSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PageTemplateId",
                table: "ComponentSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ComponentSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "ComponentSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Components",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPriority",
                table: "ComponentRelations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "ApplicationUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentSettings_PageTemplateId",
                table: "ComponentSettings",
                column: "PageTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_PageTemplates_PageTemplateId",
                table: "ComponentSettings",
                column: "PageTemplateId",
                principalTable: "PageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_PageTemplates_PageTemplateId",
                table: "ComponentSettings");

            migrationBuilder.DropIndex(
                name: "IX_ComponentSettings_PageTemplateId",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageDisplayId",
                table: "PageHistories");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "PageTemplateId",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "IsPriority",
                table: "ComponentRelations");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "ApplicationUsers");
        }
    }
}
