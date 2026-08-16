using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarUsuarioPorEmailAsync(string email);
    Task RegistrarUsuarioAsync(Usuario usuarioRecibido);
    Task<Usuario?> BuscarUsuarioPorIdAsync(long idUsuario);
    Task ActualizarUsuarioAsync(Usuario usuarioRecibido);
}