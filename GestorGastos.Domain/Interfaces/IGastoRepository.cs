using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IGastoRepository
{
    Task AgregarGastoAsync(Gasto gastoRecibido);
    Task<Gasto?> BuscarPorIdAsync(long idGasto);
    Task<IEnumerable<Gasto>> ObtenerPorUsuarioAsync(long idUsuario);
    Task ActualizarGastoAsync(Gasto gastoRecibido);
}