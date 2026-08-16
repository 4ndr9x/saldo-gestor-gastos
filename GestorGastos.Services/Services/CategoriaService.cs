using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_Categoria;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repositorio;

    public CategoriaService(ICategoriaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<RespuestaCategoriaDto> CrearCategoriaAsync(long idUsuario, CrearCategoriaDto categoriaRecibida)
    {
        bool estaCreada = await _repositorio.ExisteCategoriaPorNombreAsync(idUsuario, categoriaRecibida.Nombre);

        if (estaCreada)
        {
            throw new ConflictoExcepcion($"Ya tienes una categoría activa llamada '{categoriaRecibida.Nombre}'.");
        }
        
        Categoria categoriaNueva = new Categoria(categoriaRecibida.Nombre, idUsuario);
        await _repositorio.AgregarCategoriaAsync(categoriaNueva);

        RespuestaCategoriaDto respuestaCategoria = new RespuestaCategoriaDto();
        respuestaCategoria.Nombre = categoriaNueva.Nombre;
        respuestaCategoria.Id = categoriaNueva.Id;

        return respuestaCategoria;
    }

    public async Task<IEnumerable<RespuestaCategoriaDto>> ObtenerCategoriasAsync(long idUsuario)
    {
        IEnumerable<Categoria> categorias = await _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        return categorias.Select(c => new RespuestaCategoriaDto { Id = c.Id, Nombre = c.Nombre });
    }

    public async Task<RespuestaCategoriaDto> ObtenerCategoriaPorIdAsync(long idCategoria, long idUsuario)
    {
        Categoria? categoria = await _repositorio.BuscarPorIdAsync(idCategoria, idUsuario);
        
        if (categoria == null)
        {
            throw new NoEncontradoExcepcion("La categoria que estas intentando buscar no existe.");
        }

        RespuestaCategoriaDto respuestaCategoria = new RespuestaCategoriaDto();
        respuestaCategoria.Nombre = categoria.Nombre;
        respuestaCategoria.Id = categoria.Id;

        return respuestaCategoria;
    }

    public async Task EliminarCategoriaAsync(long idCategoria, long idUsuario)
    {
        Categoria? categoria = await _repositorio.BuscarPorIdAsync(idCategoria, idUsuario);
        
        if (categoria == null)
        { 
            throw new NoEncontradoExcepcion("La categoría que intentas eliminar no existe.");
        }
        
        categoria.CambiarEstado(false);

        await _repositorio.ActualizarCategoriaAsync(categoria);
    }

    public async Task RestaurarCategoriaAsync(long idCategoria, long idUsuario)
    {
        Categoria? categoria = await _repositorio.BuscarPorIdAsync(idCategoria, idUsuario);
        
        if (categoria == null)
        { 
            throw new NoEncontradoExcepcion("La categoría que intentas reactivar no existe.");
        }
        
        categoria.CambiarEstado(true);

        await _repositorio.ActualizarCategoriaAsync(categoria);
    }
    
    public async Task ActualizarCategoriaAsync(long idCategoria, long idUsuario, ActualizarCategoriaDto categoriaRecibida)
    {
        
        Categoria? categoria = await _repositorio.BuscarPorIdAsync(idCategoria, idUsuario);
        
        if (categoria == null)
        { 
            throw new NoEncontradoExcepcion("La categoría que intentas actualizar no existe.");
        }
        
        categoria.ActualizarNombre(categoriaRecibida.Nombre);

        await _repositorio.ActualizarCategoriaAsync(categoria);
    }
    
}