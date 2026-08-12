using GestorGastos.Domain.Models;
namespace GestorGastos.Domain.Interfaces;

public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerTodosUsuariosAsync();
}