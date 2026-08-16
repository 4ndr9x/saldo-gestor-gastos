using GestorGastos.Services.DTOs.DTOs_de_Categoria;

namespace GestorGastos.Services.Interfaces;

public interface ICategoriaService
{
    Task<RespuestaCategoriaDto> CrearCategoriaAsync(long idUsuario, CrearCategoriaDto categoriaRecibida);
    Task<IEnumerable<RespuestaCategoriaDto>> ObtenerCategoriasAsync(long idUsuario);
    Task<RespuestaCategoriaDto> ObtenerCategoriaPorIdAsync(long id, long idUsuario);
    Task EliminarCategoriaAsync(long idCategoria, long idUsuario);
    Task ActualizarCategoriaAsync(long idCategoria, long idUsuario, ActualizarCategoriaDto categoriaRecibida);
    Task RestaurarCategoriaAsync(long idCategoria, long idUsuario);

}