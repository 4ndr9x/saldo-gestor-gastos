namespace GestorGastos.Services.DTOs.DTOs_de_Presupuesto;

public class RespuestaPresupuestoDto
{
    public long Id { get; set; }
    public long CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal MontoPresupuestado { get; set; }
    public decimal MontoGastado { get; set; }
    public decimal PorcentajeConsumido { get; set; }

    public string NivelAlerta { get; set; } = "Bajo";

}