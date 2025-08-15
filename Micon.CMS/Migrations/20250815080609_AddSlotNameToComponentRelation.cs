using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Micon.CMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotNameToComponentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlotName",
                table: "ComponentRelations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotName",
                table: "ComponentRelations");
        }
    }
}
