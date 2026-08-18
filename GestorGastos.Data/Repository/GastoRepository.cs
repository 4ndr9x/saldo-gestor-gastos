using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Repository;

public class GastoRepository : IGastoRepository
{
    private readonly DbSistemaGastosContext _contexto;

    public GastoRepository(DbSistemaGastosContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarGastoAsync(Gasto gastoRecibido)
    {
        _contexto.Gastos.Add(gastoRecibido);
        await _contexto.SaveChangesAsync();
    }

    public async Task AgregarRangoDeGastosAsync(IEnumerable<Gasto> gastosRecibidos)
    {
        await _contexto.Gastos.AddRangeAsync(gastosRecibidos);
        await _contexto.SaveChangesAsync();
    }

    public async Task<Gasto?> BuscarPorIdAsync(long idGasto, long idUsuario)
    {
        return await _contexto.Gastos
            .Include(g => g.Categoria)
            .Include(g => g.MetodoPago)
            .FirstOrDefaultAsync(g =>
                g.Id == idGasto &&
                g.UsuarioId == idUsuario &&
                g.Activo);
    }

    public async Task<IEnumerable<Gasto>> ObtenerPorUsuarioAsync(long idUsuario, bool incluirEliminados = false)
    {
        IQueryable<Gasto> consulta = _contexto.Gastos
            .Where(g => g.UsuarioId == idUsuario)
            .Include(g => g.Categoria)
            .Include(g => g.MetodoPago);

        if (!incluirEliminados)
        {
            consulta = consulta.Where(g => g.Activo == true);
        }

        return await consulta.ToListAsync();
    }

    public async Task<decimal> ObtenerTotalGastadoPorMesAsync(long idUsuario, int month, int year)
    {
        decimal? total = await _contexto.Gastos
            .Where(g =>
                g.UsuarioId == idUsuario &&
                g.Fecha.Month == month &&
                g.Fecha.Year == year &&
                g.Activo)
            .SumAsync(g => (decimal?)g.MontoFinal);

        return total ?? 0m;
    }

    public async Task<decimal> ObtenerTotalGastadoPorCategoriaYMesAsync(long idUsuario, long idCategoria, int month, int year)
    {
        decimal? total = await _contexto.Gastos
            .Where(g =>
                g.UsuarioId == idUsuario &&
                g.CategoriaId == idCategoria &&
                g.Fecha.Month == month &&
                g.Fecha.Year == year &&
                g.Activo)
            .SumAsync(g => (decimal?)g.MontoFinal);
        
        return total ?? 0m;
    }

    public async Task<List<(string NombreCategoria, decimal TotalGastado)>> ObtenerTopCategoriasDelMesAsync(long idUsuario, int month, int year, int cantidad = 5)
    {
        var resultado = await _contexto.Gastos
            .AsNoTracking()
            .Where(g => g.UsuarioId == idUsuario && g.Fecha.Month == month && g.Fecha.Year == year && g.Activo)
            .GroupBy(g => new { g.CategoriaId, g.Categoria.Nombre })
            .Select(g => new 
            {
                NombreCategoria = g.Key.Nombre,
                TotalGastado = g.Sum(x => x.MontoFinal)
            })
            .OrderByDescending(c => c.TotalGastado)
            .Take(cantidad)
            .ToListAsync();
        
        return resultado.Select(r => (r.NombreCategoria, r.TotalGastado)).ToList();
    }

    public async Task ActualizarGastoAsync(Gasto gastoRecibido)
    {
        _contexto.Gastos.Update(gastoRecibido);
        await _contexto.SaveChangesAsync();
    }
    
    public async Task ActualizarGastosMasivoAsync(IEnumerable<Gasto> gastosRecibidos)
    {
        _contexto.Gastos.UpdateRange(gastosRecibidos);
        await _contexto.SaveChangesAsync();
    }
    
}