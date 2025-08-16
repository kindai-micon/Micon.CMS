using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentRelations_Tenants_TenantId",
                table: "ComponentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Components_Tenants_TenantId",
                table: "Components");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_Tenants_TenantId",
                table: "ComponentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_PageCategories_Tenants_TenantId",
                table: "PageCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_PageHistories_Tenants_TenantId",
                table: "PageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Tenants_TenantId",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplateHistories_Tenants_TenantId",
                table: "PageTemplateHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplates_Tenants_TenantId",
                table: "PageTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageTemplateHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Pages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageCategories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "ComponentSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Components",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "ComponentRelations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                table: "ApplicationUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Modified",
                table: "ApplicationUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentRelations_Tenants_TenantId",
                table: "ComponentRelations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Tenants_TenantId",
                table: "Components",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_Tenants_TenantId",
                table: "ComponentSettings",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageCategories_Tenants_TenantId",
                table: "PageCategories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageHistories_Tenants_TenantId",
                table: "PageHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Tenants_TenantId",
                table: "Pages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplateHistories_Tenants_TenantId",
                table: "PageTemplateHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplates_Tenants_TenantId",
                table: "PageTemplates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentRelations_Tenants_TenantId",
                table: "ComponentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Components_Tenants_TenantId",
                table: "Components");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentSettings_Tenants_TenantId",
                table: "ComponentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_PageCategories_Tenants_TenantId",
                table: "PageCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_PageHistories_Tenants_TenantId",
                table: "PageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Tenants_TenantId",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplateHistories_Tenants_TenantId",
                table: "PageTemplateHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTemplates_Tenants_TenantId",
                table: "PageTemplates");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "ApplicationUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageTemplates",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageTemplateHistories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Pages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageHistories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "PageCategories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "ComponentSettings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Components",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "ComponentRelations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentRelations_Tenants_TenantId",
                table: "ComponentRelations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Tenants_TenantId",
                table: "Components",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentSettings_Tenants_TenantId",
                table: "ComponentSettings",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PageCategories_Tenants_TenantId",
                table: "PageCategories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PageHistories_Tenants_TenantId",
                table: "PageHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Tenants_TenantId",
                table: "Pages",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplateHistories_Tenants_TenantId",
                table: "PageTemplateHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PageTemplates_Tenants_TenantId",
                table: "PageTemplates",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }
    }
}
