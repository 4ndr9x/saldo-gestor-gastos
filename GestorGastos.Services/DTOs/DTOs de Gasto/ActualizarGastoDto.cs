namespace GestorGastos.Services.DTOs.DTOs_de_Gasto;

public class ActualizarGastoDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public long CategoriaId { get; set; }
    public long MetodoPagoId { get; set; }
}