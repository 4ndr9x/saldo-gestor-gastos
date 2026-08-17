namespace GestorGastos.Services.DTOs.DTOs_de_Usuario;

public class RespuestaAuthDto
{
    public string Token { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MonedaUsada { get; set; } = string.Empty;
}