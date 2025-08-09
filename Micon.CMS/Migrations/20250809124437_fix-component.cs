using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class fixcomponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_ComponentRelations_ComponentRelationId",
                table: "ComponentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_PageTemplates_PageTemplateId",
                table: "ComponentSettings");

            migrationBuilder.DropIndex(
                name: "IX_ComponentSettings_ComponentRelationId",
                table: "ComponentSettings");

            migrationBuilder.DropColumn(
                name: "ComponentRelationId",
                table: "ComponentSettings");

            migrationBuilder.RenameColumn(
                name: "PageTemplateId",
                table: "ComponentSettings",
                newName: "ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_ComponentSettings_PageTemplateId",
                table: "ComponentSettings",
                newName: "IX_ComponentSettings_ComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_Components_ComponentId",
                table: "ComponentSettings",
                column: "ComponentId",
                principalTable: "Components",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_Components_ComponentId",
                table: "ComponentSettings");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "ComponentSettings",
                newName: "PageTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ComponentSettings_ComponentId",
                table: "ComponentSettings",
                newName: "IX_ComponentSettings_PageTemplateId");

            migrationBuilder.AddColumn<Guid>(
                name: "ComponentRelationId",
                table: "ComponentSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ComponentSettings_ComponentRelationId",
                table: "ComponentSettings",
                column: "ComponentRelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_ComponentRelations_ComponentRelationId",
                table: "ComponentSettings",
                column: "ComponentRelationId",
                principalTable: "ComponentRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_PageTemplates_PageTemplateId",
                table: "ComponentSettings",
                column: "PageTemplateId",
                principalTable: "PageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
