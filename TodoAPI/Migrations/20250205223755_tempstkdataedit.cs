using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoAPI.Migrations
{
    /// <inheritdoc />
    public partial class tempstkdataedit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountReference",
                table: "TempSTKData");

            migrationBuilder.DropColumn(
                name: "passkey",
                table: "TempSTKData");

            migrationBuilder.RenameColumn(
                name: "AmtTime",
                table: "TempSTKData",
                newName: "merchantID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "merchantID",
                table: "TempSTKData",
                newName: "AmtTime");

            migrationBuilder.AddColumn<string>(
                name: "AccountReference",
                table: "TempSTKData",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "passkey",
                table: "TempSTKData",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
