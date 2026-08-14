using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Repository;

public class MetodoPagoRepository : IMetodoPagoRepository
{
    private readonly DbSistemaGastosContext _contexto;

    public MetodoPagoRepository(DbSistemaGastosContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarMetodoPagoAsync(MetodoPago metodoPago)
    {
        _contexto.MetodosPago.Add(metodoPago);
        await _contexto.SaveChangesAsync();
    }

    public async Task<MetodoPago?> BuscarPorIdAsync(long idMetodoPago)
    {
        return await _contexto.MetodosPago.FindAsync(idMetodoPago);
    }

    public async Task<IEnumerable<MetodoPago>> ObtenerPorUsuarioAsync(long idUsuario)
    {
        return await _contexto.MetodosPago
            .Where(m => m.UsuarioId == idUsuario && m.Activo)
            .ToListAsync();
    }

    public async Task ActualizarMetodoPagoAsync(MetodoPago metodoPago)
    {
        _contexto.MetodosPago.Update(metodoPago);
        await _contexto.SaveChangesAsync();
    }

    public async Task<bool> ExisteMetodoPagoPorNombreAsync(long idUsuario, string nombreMetodoPago)
    {
        return await _contexto.MetodosPago.AnyAsync(m =>
            m.UsuarioId == idUsuario &&
            m.Nombre == nombreMetodoPago &&
            m.Activo);
    }
}