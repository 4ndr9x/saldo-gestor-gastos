using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Repository;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly DbSistemaGastosContext _contexto;
    
    public CategoriaRepository(DbSistemaGastosContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarCategoriaAsync(Categoria categoriaRecibida)
    {
        _contexto.Categorias.Add(categoriaRecibida);
        await _contexto.SaveChangesAsync();
    }

    public async Task<Categoria?> BuscarPorIdAsync(long idCategoria)
    {
        return await _contexto.Categorias.FindAsync(idCategoria);
    }

    public async Task<IEnumerable<Categoria>> ObtenerPorUsuarioAsync(long idUsuario)
    {
        return await _contexto.Categorias
            .Where(c => c.UsuarioId == idUsuario && c.Activo == true)
            .ToListAsync();
    }

    public async Task ActualizarCategoriaAsync(Categoria categoriaRecibida)
    {
        _contexto.Categorias.Update(categoriaRecibida);
        await _contexto.SaveChangesAsync();
    }

    public async Task<bool> ExisteCategoriaPorNombreAsync(long idUsuario, string nombreCategoria)
    {
        return await _contexto.Categorias
            .AnyAsync(c => 
                c.UsuarioId == idUsuario &&
                c.Nombre == nombreCategoria &&
                c.Activo);
        
    }
}