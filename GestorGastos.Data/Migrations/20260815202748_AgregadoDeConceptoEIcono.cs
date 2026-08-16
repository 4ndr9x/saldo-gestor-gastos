using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorGastos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoDeConceptoEIcono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icono",
                schema: "GestorGastos",
                table: "MetodosPago",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                schema: "GestorGastos",
                table: "Gastos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icono",
                schema: "GestorGastos",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "Concepto",
                schema: "GestorGastos",
                table: "Gastos");
        }
    }
}
