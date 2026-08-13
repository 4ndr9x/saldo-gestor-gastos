using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorGastos.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarEsquemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "GestorGastos");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Usuarios",
                newSchema: "GestorGastos");

            migrationBuilder.RenameTable(
                name: "MetodoPagos",
                newName: "MetodoPagos",
                newSchema: "GestorGastos");

            migrationBuilder.RenameTable(
                name: "Gastos",
                newName: "Gastos",
                newSchema: "GestorGastos");

            migrationBuilder.RenameTable(
                name: "Categorias",
                newName: "Categorias",
                newSchema: "GestorGastos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Usuarios",
                schema: "GestorGastos",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "MetodoPagos",
                schema: "GestorGastos",
                newName: "MetodoPagos");

            migrationBuilder.RenameTable(
                name: "Gastos",
                schema: "GestorGastos",
                newName: "Gastos");

            migrationBuilder.RenameTable(
                name: "Categorias",
                schema: "GestorGastos",
                newName: "Categorias");
        }
    }
}
