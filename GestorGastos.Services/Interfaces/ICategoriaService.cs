using GestorGastos.Services.DTOs.DTOs_de_Categoria;

namespace GestorGastos.Services.Interfaces;

public interface ICategoriaService
{
    Task<RespuestaCategoriaDto> CrearCategoriaAsync(string? idUsuario, CrearCategoriaDto categoriaRecibida);
    Task<IEnumerable<RespuestaCategoriaDto>> ObtenerCategoriasAsync(string? idUsuario);
    Task<RespuestaCategoriaDto> ObtenerCategoriaPorIdAsync(long id, string? idUsuario);
    Task EliminarCategoriaAsync(long idCategoria, string? idUsuario);
    Task ActualizarCategoriaAsync(long idCategoria, string? idUsuario, ActualizarCategoriaDto categoriaRecibida);
    Task RestaurarCategoriaAsync(long idCategoria, string? idUsuario);

}