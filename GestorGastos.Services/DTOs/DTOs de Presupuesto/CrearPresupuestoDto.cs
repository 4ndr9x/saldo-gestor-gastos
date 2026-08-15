namespace GestorGastos.Services.DTOs.DTOs_de_Presupuesto;

public class CrearPresupuestoDto
{
    public long CategoriaId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal MontoMaximo { get; set; }
}