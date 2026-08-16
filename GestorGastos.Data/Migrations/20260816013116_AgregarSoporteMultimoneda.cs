using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorGastos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSoporteMultimoneda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Monto",
                schema: "GestorGastos",
                table: "Gastos",
                newName: "MontoOriginal");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MonedaUsada",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                schema: "GestorGastos",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFinal",
                schema: "GestorGastos",
                table: "Gastos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaCambio",
                schema: "GestorGastos",
                table: "Gastos",
                type: "decimal(18,10)",
                precision: 18,
                scale: 10,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonedaUsada",
                schema: "GestorGastos",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Moneda",
                schema: "GestorGastos",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "MontoFinal",
                schema: "GestorGastos",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "TasaCambio",
                schema: "GestorGastos",
                table: "Gastos");

            migrationBuilder.RenameColumn(
                name: "MontoOriginal",
                schema: "GestorGastos",
                table: "Gastos",
                newName: "Monto");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "GestorGastos",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
