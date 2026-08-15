using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IGastoRepository
{
    Task AgregarGastoAsync(Gasto gastoRecibido);
    Task AgregarRangoDeGastosAsync(IEnumerable<Gasto> gastosRecibidos);
    Task<Gasto?> BuscarPorIdAsync(long idGasto, long idUsuario);
    Task<IEnumerable<Gasto>> ObtenerPorUsuarioAsync(long idUsuario);
    Task ActualizarGastoAsync(Gasto gastoRecibido);
    Task<decimal> ObtenerTotalGastadoPorCategoriaYMesAsync(long idUsuario, long idCategoria, int month, int year);
}