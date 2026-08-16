using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Repository;

public class PresupuestoRepository : IPresupuestoRepository
{
    private readonly DbSistemaGastosContext _contexto;

    public PresupuestoRepository(DbSistemaGastosContext contexto)
    {
        _contexto = contexto;
    }
    
    public async Task AgregarPresupuestoAsync(Presupuesto presupuesto)
    {
        _contexto.Presupuestos.Add(presupuesto);
        await _contexto.SaveChangesAsync();
    }

    public async Task ActualizarPresupuestoAsync(Presupuesto presupuesto)
    {
        _contexto.Presupuestos.Update(presupuesto);
        await _contexto.SaveChangesAsync();
    }

    public async Task<Presupuesto?> BuscarPorIdAsync(long idPresupuesto, long idUsuario)
    {
        return await _contexto.Presupuestos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == idPresupuesto && p.UsuarioId == idUsuario && p.Activo);
    }

    public async Task<Presupuesto?> ObtenerPresupuestoPorMesYCategoriaAsync(long idUsuario, long idCategoria, int month, int year)
    {
        return await _contexto.Presupuestos
            .FirstOrDefaultAsync(p =>
                p.UsuarioId == idUsuario &&
                p.CategoriaId == idCategoria &&
                p.Month == month &&
                p.Year == year &&
                p.Activo); 
    }

    public async Task<IEnumerable<Presupuesto>> ObtenerPresupuestosDelMesAsync(long idUsuario, int month, int year)
    {
        return await _contexto.Presupuestos
            .Include(p => p.Categoria)
            .Where(p =>
                p.UsuarioId == idUsuario &&
                p.Month == month &&
                p.Year == year &&
                p.Activo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Presupuesto>> ObtenerTodosPorIdAsync(long idUsuario)
    {
        return await _contexto.Presupuestos
            .Where(p =>
                p.UsuarioId == idUsuario &&
                p.Activo)
            .ToListAsync();
    }
}