using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class InicioSesionDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}