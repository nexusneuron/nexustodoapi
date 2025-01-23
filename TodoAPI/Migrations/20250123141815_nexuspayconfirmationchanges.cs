using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoAPI.Migrations
{
    /// <inheritdoc />
    public partial class nexuspayconfirmationchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NexuspayConfirmation",
                table: "NexuspayConfirmation");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "NexuspayConfirmation");

            migrationBuilder.UpdateData(
                table: "NexuspayConfirmation",
                keyColumn: "TransID",
                keyValue: null,
                column: "TransID",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TransID",
                table: "NexuspayConfirmation",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyTransID",
                table: "NexuspayConfirmation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NexuspayConfirmation",
                table: "NexuspayConfirmation",
                column: "TransID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NexuspayConfirmation",
                table: "NexuspayConfirmation");

            migrationBuilder.DropColumn(
                name: "ThirdPartyTransID",
                table: "NexuspayConfirmation");

            migrationBuilder.AlterColumn<string>(
                name: "TransID",
                table: "NexuspayConfirmation",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ID",
                table: "NexuspayConfirmation",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NexuspayConfirmation",
                table: "NexuspayConfirmation",
                column: "ID");
        }
    }
}
