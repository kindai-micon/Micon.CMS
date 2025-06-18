using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class fixmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplates_ComponentRelations_ComponentRelationId",
                table: "PageTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "ComponentRelationId",
                table: "PageTemplates",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplates_ComponentRelations_ComponentRelationId",
                table: "PageTemplates",
                column: "ComponentRelationId",
                principalTable: "ComponentRelations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplates_ComponentRelations_ComponentRelationId",
                table: "PageTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "ComponentRelationId",
                table: "PageTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplates_ComponentRelations_ComponentRelationId",
                table: "PageTemplates",
                column: "ComponentRelationId",
                principalTable: "ComponentRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
