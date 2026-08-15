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

    public async Task<IEnumerable<Gasto>> ObtenerPorUsuarioAsync(long idUsuario)
    {
        return await _contexto.Gastos
            .Where(g => g.UsuarioId == idUsuario && g.Activo)
            .Include(g => g.Categoria)
            .Include(g => g.MetodoPago)
            .ToListAsync();
    }
    public async Task<decimal> ObtenerTotalGastadoPorCategoriaYMesAsync(long idUsuario, long idCategoria, int month, int year)
    {
        return await _contexto.Gastos
            .Where(g =>
                g.UsuarioId == idUsuario &&
                g.CategoriaId == idCategoria &&
                g.Fecha.Month == month &&
                g.Fecha.Year == year &&
                g.Activo)
            .SumAsync(g => g.Monto);
    }

    public async Task ActualizarGastoAsync(Gasto gastoRecibido)
    {
        _contexto.Gastos.Update(gastoRecibido);
        await _contexto.SaveChangesAsync();
    }
    
}