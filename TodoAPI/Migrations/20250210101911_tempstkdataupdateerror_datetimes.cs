using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoAPI.Migrations
{
    /// <inheritdoc />
    public partial class tempstkdataupdateerror_datetimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TempSTKData",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Errorcode",
                table: "TempSTKData",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Errordesc",
                table: "TempSTKData",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "SktCallback",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "NexuspayConfirmation",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "TempSTKData");

            migrationBuilder.DropColumn(
                name: "Errorcode",
                table: "TempSTKData");

            migrationBuilder.DropColumn(
                name: "Errordesc",
                table: "TempSTKData");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SktCallback");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "NexuspayConfirmation");
        }
    }
}
