namespace GestorGastos.Services.DTOs.DTOs_de_Exportacion;

public class ResultadoImportacionDto
{
    public int TotalFilasProcesadas { get; set; }
    public int GastosImportadosExitosamente { get; set; }
    public int FilasConErrores { get; set; }
    
    public List<DetalleErrorFilaDto> DetallesErrores { get; set; } = new List<DetalleErrorFilaDto>();
}