using GestorGastos.Services.DTOs.DTOs_de_MetodoPago;

namespace GestorGastos.Services.Interfaces;

public interface IMetodoPagoService
{
    Task<RespuestaMetodoPagoDto> CrearMetodoPagoAsync(string? idUsuario, CrearMetodoPagoDto metodoPagoRecibido);
    Task<IEnumerable<RespuestaMetodoPagoDto>> ObtenerMetodosPagoAsync(string? idUsuario);
    Task<RespuestaMetodoPagoDto> ObtenerMetodoPagoPorIdAsync(long idMetodoPago, string? idUsuario);
    Task ActualizarMetodoPagoAsync(long idMetodoPago, string? idUsuario, ActualizarMetodoPagoDto metodoPagoRecibido);
    Task EliminarMetodoPagoAsync(long idMetodoPago, string? idUsuario);
    Task RestaurarMetodoPagoAsync(long idMetodoPago, string? idUsuario);
}