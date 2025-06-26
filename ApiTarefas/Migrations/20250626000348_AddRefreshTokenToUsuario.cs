using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTarefas.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "TabelaUsuario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiracao",
                table: "TabelaUsuario",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "TabelaUsuario");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiracao",
                table: "TabelaUsuario");
        }
    }
}
