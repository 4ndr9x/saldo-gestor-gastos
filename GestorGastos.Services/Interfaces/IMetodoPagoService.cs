using GestorGastos.Services.DTOs.DTOs_de_MetodoPago;

namespace GestorGastos.Services.Interfaces;

public interface IMetodoPagoService
{
    Task<RespuestaMetodoPagoDto> CrearMetodoPagoAsync(long idUsuario, CrearMetodoPagoDto metodoPagoRecibido);
    Task<IEnumerable<RespuestaMetodoPagoDto>> ObtenerMetodosPagoAsync(long idUsuario);
    Task<RespuestaMetodoPagoDto> ObtenerMetodoPagoPorIdAsync(long idMetodoPago, long idUsuario);
    Task ActualizarMetodoPagoAsync(long idMetodoPago, long idUsuario, ActualizarMetodoPagoDto metodoPagoRecibido);
    Task EliminarMetodoPagoAsync(long idMetodoPago, long idUsuario);
    Task RestaurarMetodoPagoAsync(long idMetodoPago, long idUsuario);
}