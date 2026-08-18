using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs;
using GestorGastos.Services.DTOs.DTOs_de_Usuario;

namespace GestorGastos.Services.Interfaces;

public interface IUsuarioService
{
    Task<long> RegistrarUsuarioAsync(RegistroDto usuarioRecibido);
    Task<RespuestaAuthDto> AutenticarUsuarioAsync(InicioSesionDto usuarioRecibido);
    Task ActualizarMonedaUsadaAsync(long idUsuario, ActualizarMonedaUsadaDto monedaUsada);
    Task ActualizarNombreAsync(long idUsuario, ActualizarPerfilDto monedaRecibida);
    Task ActualizarPasswordAsync(long idUsuario, CambiarPasswordDto usuarioRecibido);
    Task EliminarCuentaAsync(long idUsuario);
    Task<RespuestaPerfilDto> ObtenerMiPerfil(long idUsuario);
}