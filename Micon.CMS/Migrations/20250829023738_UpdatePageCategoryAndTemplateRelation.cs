using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePageCategoryAndTemplateRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplates_PageCategories_PageCategoryId",
                table: "PageTemplates");

            migrationBuilder.DropIndex(
                name: "IX_PageTemplates_PageCategoryId",
                table: "PageTemplates");

            migrationBuilder.DropColumn(
                name: "PageCategoryId",
                table: "PageTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "PageCategoryId",
                table: "Pages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PageTemplateId",
                table: "PageCategories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Pages_PageCategoryId",
                table: "Pages",
                column: "PageCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PageCategories_PageTemplateId",
                table: "PageCategories",
                column: "PageTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageCategories_PageTemplates_PageTemplateId",
                table: "PageCategories",
                column: "PageTemplateId",
                principalTable: "PageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_PageCategories_PageCategoryId",
                table: "Pages",
                column: "PageCategoryId",
                principalTable: "PageCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageCategories_PageTemplates_PageTemplateId",
                table: "PageCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_PageCategories_PageCategoryId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_PageCategoryId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_PageCategories_PageTemplateId",
                table: "PageCategories");

            migrationBuilder.DropColumn(
                name: "PageCategoryId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageTemplateId",
                table: "PageCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "PageCategoryId",
                table: "PageTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PageTemplates_PageCategoryId",
                table: "PageTemplates",
                column: "PageCategoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplates_PageCategories_PageCategoryId",
                table: "PageTemplates",
                column: "PageCategoryId",
                principalTable: "PageCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
