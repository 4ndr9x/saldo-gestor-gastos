using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorGastos.Data.Migrations
{
    /// <inheritdoc />
    public partial class CambiarMontoAMontoMaximo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Monto",
                schema: "GestorGastos",
                table: "Presupuestos",
                newName: "MontoMaximo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MontoMaximo",
                schema: "GestorGastos",
                table: "Presupuestos",
                newName: "Monto");
        }
    }
}
