using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarUsuarioPorEmail(string email);
    Task RegistrarUsuarioAsync(Usuario usuarioRecibido);
    Task<Usuario?> BuscarUsuarioPorId(long idUsuario);
    Task ActualizarUsuarioAsync(Usuario usuarioRecibido);
}