using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarUsuarioPorEmail(string email);
    Task RegistrarUsuario(Usuario usuarioRecibido);
}