using GestorGastos.Services.DTOs;
using GestorGastos.Services.DTOs.DTOs_de_Usuario;

namespace GestorGastos.Services.Interfaces;

public interface IUsuarioService
{
    Task<long> RegistrarUsuarioAsync(RegistroDto usuarioRecibido);
    Task<RespuestaAuthDto> AutenticarUsuarioAsync(InicioSesionDto usuarioRecibido);
    Task ActualizarPerfilAsync(string? idUsuario, ActualizarPerfilDto usuarioRecibido);
    Task ActualizarPasswordAsync(string? idUsuario, CambiarPasswordDto usuarioRecibido);
    Task EliminarCuentaAsync(string? idUsuario);
}