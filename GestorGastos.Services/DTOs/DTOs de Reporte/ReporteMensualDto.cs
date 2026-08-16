namespace GestorGastos.Services.DTOs.DTOs_de_Reporte;

public class ReporteMensualDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalGastado { get; set; }
    public decimal TotalMesAnterior { get; set; }
    public decimal Diferencia { get; set; }
    public string MensajeComparacion { get; set; } = string.Empty;
    public List<CategoriaReporteDto> TopCategorias { get; set; } = new List<CategoriaReporteDto>();
    public string NombreUsuario { get; set; } = string.Empty;
    public string Moneda { get; set; } = string.Empty;
}