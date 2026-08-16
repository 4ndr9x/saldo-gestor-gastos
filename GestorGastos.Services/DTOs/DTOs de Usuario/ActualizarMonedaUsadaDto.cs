using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class ActualizarMonedaUsadaDto
{
    [Required(ErrorMessage = "La moneda es obligatoria.")]
    public string MonedaUsada { get; set; } = string.Empty;
}