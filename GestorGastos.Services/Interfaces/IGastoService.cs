using GestorGastos.Services.DTOs.DTOs_de_Exportacion;
using GestorGastos.Services.DTOs.DTOs_de_Gasto;


namespace GestorGastos.Services.Interfaces;

public interface IGastoService
{
    Task<RespuestaGastoDto> CrearGastoAsync(long idUsuario, CrearGastoDto gastoRecibido);
    Task<IEnumerable<RespuestaGastoDto>> ObtenerTodosGastosAsync(long idUsuario, bool incluirEliminados = false);
    Task<RespuestaGastoDto> ObtenerGastoPorIdAsync(long idGasto, long idUsuario);
    Task EliminarGastoAsync(long idGasto, long idUsuario);
    Task ActualizarGastoAsync(long idGasto, long idUsuario, ActualizarGastoDto gastoRecibido);
    Task RestaurarGastoAsync(long idGasto, long idUsuario);
    Task<ResultadoImportacionDto> ImportarGastosDesdeExcelAsync(long idUsuario, Stream archivoExcel);
}
