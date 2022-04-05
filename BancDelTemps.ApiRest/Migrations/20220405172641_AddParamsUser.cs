using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancDelTemps.ApiRest.Migrations
{
    public partial class AddParamsUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDate",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ValidatedRegister",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ValidatedRegister",
                table: "Users");
        }
    }
}
