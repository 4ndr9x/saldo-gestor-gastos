namespace GestorGastos.Services.DTOs.DTOs_de_Gasto;

public class RespuestaGastoDto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    
    public long CategoriaId { get; set; }
    public long MetodoPagoId { get; set; }
    
    public string Categoria { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
}