using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class RegistroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La seleccion de moneda es obligatoria.")]
    public string MonedaUsada { get; set; } = string.Empty;
}