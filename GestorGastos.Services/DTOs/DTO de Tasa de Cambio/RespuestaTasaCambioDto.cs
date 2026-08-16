namespace GestorGastos.Services.DTOs.DTOs_de_Tasa_de_Cambio;

public class RespuestaTasaCambioDto
{
    public string Base { get; set; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; set; } = new();
}