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
    
    // Aquí implementamos la lógica de alertas del requerimiento
    // Editar mensajes más adelante.
    public string Alerta 
    { 
        get 
        {
            if (PorcentajeConsumido >= 100) return "¡Presupuesto excedido (100% o más)!";
            if (PorcentajeConsumido >= 80) return "Precaución: Has consumido el 80% o más de tu presupuesto.";
            if (PorcentajeConsumido >= 50) return "Aviso: Has consumido la mitad (50%) de tu presupuesto.";
            return "Dentro del margen seguro.";
        } 
    }
}