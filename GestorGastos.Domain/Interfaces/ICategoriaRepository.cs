using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task AgregarCategoriaAsync(Categoria categoriaRecibida);
    Task<Categoria?> BuscarPorIdAsync(long idCategoria, long idUsuario);
    Task<IEnumerable<Categoria>> ObtenerPorUsuarioAsync(long idUsuario);
    Task ActualizarCategoriaAsync(Categoria categoriaRecibida);
    Task<bool> ExisteCategoriaPorNombreAsync(long idUsuario, string nombreCategoria);
}