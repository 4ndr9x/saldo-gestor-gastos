using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class ActualizarPerfilDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
}