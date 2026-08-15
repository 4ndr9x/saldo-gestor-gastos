using GestorGastos.Services.DTOs.DTOs_de_Gasto;

namespace GestorGastos.Services.Interfaces;

public interface IGastoService
{
    Task<RespuestaGastoDto> CrearGastoAsync(string? idUsuario, CrearGastoDto gastoRecibido);
    Task<IEnumerable<RespuestaGastoDto>> ObtenerTodosGastosAsync(string? idUsuario);
    Task<RespuestaGastoDto> ObtenerGastoPorIdAsync(long idGasto, string? idUsuario);
    Task EliminarGastoAsync(long idGasto, string? idUsuario);
    Task ActualizarGastoAsync(long idGasto, string? idUsuario, ActualizarGastoDto gastoRecibido);
    Task RestaurarGastoAsync(long idGasto, string? idUsuario);
}
