using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs;

namespace GestorGastos.Services.Interfaces;

public interface IUsuarioService
{
    Task<long> RegistrarUsuarioAsync(RegistroDto usuarioRecibido);
    Task<RespuestaAuthDto> AutenticarUsuarioAsync(InicioSesionDto usuarioRecibido);
}