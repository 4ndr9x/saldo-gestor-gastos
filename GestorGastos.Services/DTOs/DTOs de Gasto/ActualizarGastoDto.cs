namespace GestorGastos.Services.DTOs.DTOs_de_Gasto;

public class ActualizarGastoDto
{
    public string Concepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public long CategoriaId { get; set; }
    public long MetodoPagoId { get; set; }
}