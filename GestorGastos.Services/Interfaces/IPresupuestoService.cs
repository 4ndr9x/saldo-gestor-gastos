using GestorGastos.Services.DTOs.DTOs_de_Presupuesto;

namespace GestorGastos.Services.Interfaces;

public interface IPresupuestoService
{
    Task<RespuestaPresupuestoDto> CrearPresupuestoAsync(long idUsuario, CrearPresupuestoDto presupuestoRecibido);
    Task<IEnumerable<RespuestaPresupuestoDto>> ObtenerResumenPresupuestosDelMesAsync(long idUsuario, int month, int year);
    Task ActualizarPresupuestoAsync(long idPresupuesto, long idUsuario, ActualizarPresupuestoDto presupuestoRecibido);
    Task EliminarPresupuestoAsync(long idPresupuesto, long idUsuario);
    Task<RespuestaPresupuestoDto> ObtenerPresupuestoPorIdAsync(long idPresupuesto, long idUsuario);
}