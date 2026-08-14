using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class CambiarPasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string PasswordNuevo { get; set; } = string.Empty;
}