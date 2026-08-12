using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<List<Usuario>> BuscarTodosLosUsuarios();
}