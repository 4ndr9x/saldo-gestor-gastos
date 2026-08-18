using System.Security.Claims;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Services.DTOs.DTOs_de_Exportacion;
using GestorGastos.Services.DTOs.DTOs_de_Gasto;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GastosController : ControllerBase
{
    private readonly IGastoService _gastoService;
    private readonly IExportacionService _exportacionService;

    public GastosController(IGastoService gastoService, IExportacionService exportacionService)
    {
        _gastoService = gastoService;
        _exportacionService = exportacionService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearGasto([FromBody] CrearGastoDto gastoRecibido)
    {
        long idUsuario = ObtenerIdUsuario();
        RespuestaGastoDto respuesta = await _gastoService.CrearGastoAsync(idUsuario, gastoRecibido);
        
        return CreatedAtAction(nameof(ObtenerGastoPorId), new { idGasto = respuesta.Id }, respuesta);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodosLosGastos([FromQuery] bool incluirEliminados = false)
    {
        long idUsuario = ObtenerIdUsuario();
        IEnumerable<RespuestaGastoDto> gastos = await _gastoService.ObtenerTodosGastosAsync(idUsuario, incluirEliminados);
        
        return Ok(gastos);
    }

    [HttpGet("{idGasto:long}")]
    public async Task<IActionResult> ObtenerGastoPorId([FromRoute] long idGasto)
    {
        long idUsuario = ObtenerIdUsuario();
        RespuestaGastoDto gasto = await _gastoService.ObtenerGastoPorIdAsync(idGasto, idUsuario);
        
        return Ok(gasto);
    }

    [HttpPut("{idGasto:long}")]
    public async Task<IActionResult> ActualizarGasto([FromRoute] long idGasto, [FromBody] ActualizarGastoDto gastoRecibido)
    {
        long idUsuario = ObtenerIdUsuario();
        await _gastoService.ActualizarGastoAsync(idGasto, idUsuario, gastoRecibido);
        
        return NoContent();
    }

    [HttpDelete("{idGasto:long}")]
    public async Task<IActionResult> EliminarGasto([FromRoute] long idGasto)
    {
        long idUsuario = ObtenerIdUsuario();
        await _gastoService.EliminarGastoAsync(idGasto, idUsuario);
        
        return NoContent();
    }

    [HttpPatch("{idGasto:long}/restaurar")]
    public async Task<IActionResult> RestaurarGasto([FromRoute] long idGasto)
    {
        long idUsuario = ObtenerIdUsuario();
        await _gastoService.RestaurarGastoAsync(idGasto, idUsuario);
        
        return NoContent();
    }
    
    [HttpPost("importar")]
    public async Task<IActionResult> ImportarDesdeExcel(IFormFile? archivoExcel)
    {
        long idUsuario = ObtenerIdUsuario();
        IFormFile archivoValidado = ValidarExcelEnviado(archivoExcel);
        
        using Stream stream = archivoValidado.OpenReadStream();
        ResultadoImportacionDto resultado = await _gastoService.ImportarGastosDesdeExcelAsync(idUsuario, stream);
        
        return Ok(resultado);
    }
    
    [HttpGet("plantilla-importacion")]
    public IActionResult DescargarPlantillaImportacion()
    {
        // Como es solo una plantilla estática, no necesitamos el ID del usuario
        var archivoBytes = _exportacionService.GenerarPlantillaImportacionExcel();
    
        return File(
            archivoBytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "Plantilla_Importar_Gastos.xlsx"
        );
    }
    
    // METODOS PRIVADOS PARA VALIDAR ANTES DE ENVIAR DATOS A LOS SERVICIOS.
    private long ObtenerIdUsuario()
    {
        string? idString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idString) || !long.TryParse(idString, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("El token no contiene un identificador válido.");
        }

        return idConvertido;
    }

    private IFormFile ValidarExcelEnviado(IFormFile? archivo)
    {
        List<string> detalles = new List<string>();
        if (archivo == null || archivo.Length == 0)
        {
            detalles.Add("No se envió ningún archivo o el archivo está corrupto (0 bytes).");
            throw new DatosErroneosExcepcion("Ocurrió un error con el archivo enviado.", detalles);
        }
        
        if (Path.GetExtension(archivo.FileName).ToLower() != ".xlsx")
        {
            detalles.Add("El formato del archivo no es válido. Solo se aceptan archivos .xlsx");
        }
        
        const long longitudMaximaArchivo = 10 * 1024 * 1024;
        if (archivo.Length > longitudMaximaArchivo)
        {
            detalles.Add("El archivo es demasiado grande. El límite máximo es 10 MB.");
        }
        
        if (detalles.Any())
        {
            throw new DatosErroneosExcepcion("Ocurrio un error con el archivo enviado.", detalles);
        }
        
        return archivo;
    }
}