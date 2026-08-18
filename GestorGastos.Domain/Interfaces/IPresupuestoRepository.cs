using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IPresupuestoRepository
{
    Task AgregarPresupuestoAsync(Presupuesto presupuesto);
    Task ActualizarPresupuestoAsync(Presupuesto presupuesto);
    Task<Presupuesto?> BuscarPorIdAsync(long idPresupuesto, long idUsuario);
    Task<Presupuesto?> ObtenerPresupuestoPorMesYCategoriaAsync(long idUsuario, long idCategoria, int month, int year);
    Task<IEnumerable<Presupuesto>> ObtenerPresupuestosDelMesAsync(long idUsuario, int month, int year);
    Task<IEnumerable<Presupuesto>> ObtenerTodosPorIdAsync(long idUsuario);
    Task ActualizarPresupuestosMasivoAsync(IEnumerable<Presupuesto> presupuestosRecibidos);
}
