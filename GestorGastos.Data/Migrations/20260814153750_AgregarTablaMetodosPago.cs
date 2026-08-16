using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorGastos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaMetodosPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_MetodoPagos_MetodoPagoId",
                schema: "GestorGastos",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_MetodoPagos_Usuarios_UsuarioId",
                schema: "GestorGastos",
                table: "MetodoPagos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetodoPagos",
                schema: "GestorGastos",
                table: "MetodoPagos");

            migrationBuilder.DropColumn(
                name: "Icono",
                schema: "GestorGastos",
                table: "MetodoPagos");

            migrationBuilder.RenameTable(
                name: "MetodoPagos",
                schema: "GestorGastos",
                newName: "MetodosPago",
                newSchema: "GestorGastos");

            migrationBuilder.RenameIndex(
                name: "IX_MetodoPagos_UsuarioId",
                schema: "GestorGastos",
                table: "MetodosPago",
                newName: "IX_MetodosPago_UsuarioId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "GestorGastos",
                table: "MetodosPago",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                schema: "GestorGastos",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetodosPago",
                schema: "GestorGastos",
                table: "MetodosPago",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_MetodosPago_MetodoPagoId",
                schema: "GestorGastos",
                table: "Gastos",
                column: "MetodoPagoId",
                principalSchema: "GestorGastos",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetodosPago_Usuarios_UsuarioId",
                schema: "GestorGastos",
                table: "MetodosPago",
                column: "UsuarioId",
                principalSchema: "GestorGastos",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_MetodosPago_MetodoPagoId",
                schema: "GestorGastos",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_MetodosPago_Usuarios_UsuarioId",
                schema: "GestorGastos",
                table: "MetodosPago");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetodosPago",
                schema: "GestorGastos",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "Activo",
                schema: "GestorGastos",
                table: "MetodosPago");

            migrationBuilder.RenameTable(
                name: "MetodosPago",
                schema: "GestorGastos",
                newName: "MetodoPagos",
                newSchema: "GestorGastos");

            migrationBuilder.RenameIndex(
                name: "IX_MetodosPago_UsuarioId",
                schema: "GestorGastos",
                table: "MetodoPagos",
                newName: "IX_MetodoPagos_UsuarioId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "GestorGastos",
                table: "MetodoPagos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Icono",
                schema: "GestorGastos",
                table: "MetodoPagos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetodoPagos",
                schema: "GestorGastos",
                table: "MetodoPagos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_MetodoPagos_MetodoPagoId",
                schema: "GestorGastos",
                table: "Gastos",
                column: "MetodoPagoId",
                principalSchema: "GestorGastos",
                principalTable: "MetodoPagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetodoPagos_Usuarios_UsuarioId",
                schema: "GestorGastos",
                table: "MetodoPagos",
                column: "UsuarioId",
                principalSchema: "GestorGastos",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
