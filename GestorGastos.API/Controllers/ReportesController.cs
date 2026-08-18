using System.Security.Claims;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Services.DTOs.DTOs_de_Reporte;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;
    private readonly IExportacionService _exportacionService;

    public ReportesController(IReporteService reporteService, IExportacionService exportacionService)
    {
        _reporteService = reporteService;
        _exportacionService = exportacionService;
    }

    [HttpGet("mensual")]
    public async Task<IActionResult> ObtenerReporteMensual([FromQuery] int? month, [FromQuery] int? year)
    {
        var (monthValido, yearValido) = ValidarFecha(month, year);
        
        long idUsuario = ObtenerIdUsuario();
        
        ReporteMensualDto reporteCreado = await _reporteService.GenerarReporteMensualAsync(idUsuario, monthValido, yearValido);

        reporteCreado.MonthString = ConvertirMesATexto(reporteCreado.Month);
        
        return Ok(reporteCreado);
    }
    
    [HttpGet("mensual/exportar/txt")]
    public async Task<IActionResult> ExportarTxt([FromQuery] int? month, [FromQuery] int? year)
    {
        long idUsuario = ObtenerIdUsuario();
        var (monthValido, yearValido) = ValidarFecha(month, year);

        var reporteCreado = await _reporteService.GenerarReporteMensualAsync(idUsuario, monthValido, yearValido);
        
        reporteCreado.MonthString = ConvertirMesATexto(reporteCreado.Month);
        
        var archivoBytes = _exportacionService.GenerarReporteTxt(reporteCreado);

        return File(archivoBytes, "text/plain", $"Reporte_gastos_{monthValido}_{yearValido}.txt");
    }
    
    [HttpGet("mensual/exportar/json")]
    public async Task<IActionResult> ExportarJson([FromQuery] int? month, [FromQuery] int? year)
    {
        long idUsuario = ObtenerIdUsuario();
        var (mesValido, anioValido) = ValidarFecha(month, year);
        
        var reporteCreado = await _reporteService.GenerarReporteMensualAsync(idUsuario, mesValido, anioValido);
        
        reporteCreado.MonthString = ConvertirMesATexto(reporteCreado.Month);
        
        var archivoBytes = _exportacionService.GenerarReporteJson(reporteCreado);

        return File(archivoBytes, "application/json", $"Reporte_{mesValido}_{anioValido}.json");
    }
    
    [HttpGet("mensual/exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] int? month, [FromQuery] int? year)
    {
        long idUsuario = ObtenerIdUsuario();
        var (monthValido, yearValido) = ValidarFecha(month, year);
        
        var reporteCreado = await _reporteService.GenerarReporteMensualAsync(idUsuario, monthValido, yearValido);
        
        reporteCreado.MonthString = ConvertirMesATexto(reporteCreado.Month);
        
        var archivoBytes = _exportacionService.GenerarReporteExcel(reporteCreado);
        
        return File(
            archivoBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_gastos_{monthValido}_{yearValido}.xlsx");
    }

    private long ObtenerIdUsuario()
    {
        string? idString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idString) || !long.TryParse(idString, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("El token no contiene un identificador válido.");
        }

        return idConvertido;
    }

    private (int monthValido, int yearValido) ValidarFecha(int? month, int? year)
    {
        List<string> detalles = new List<string>();
        if (month == null || year == null)
        {
            detalles.Add("No fue introducida ninguna fecha o no se introdujeron correctamente.");
            throw new DatosErroneosExcepcion("Ocurrio un error al intentar obtener el reporte.", detalles);
        }
        
        if (month < 1 || month > 12)
        {
            detalles.Add("El mes debe estar entre 1 y 12.");
        }
    
        if (year < 2000 || year > 2100)
        {
            detalles.Add("El año proporcionado no es válido.");
        }

        if (detalles.Any())
        {
            throw new DatosErroneosExcepcion("Ocurrio un error al intentar obtener el reporte.", detalles);
        }

        return new (month.Value, year.Value);
    }
    
    private string ConvertirMesATexto(int mes)
    {
        switch (mes)
        {
            case 1:
                return "Enero";
            case 2:
                return "Febrero";
            case 3:
                return "Marzo";
            case 4:
                return "Abril";
            case 5:
                return "Mayo";
            case 6:
                return "Junio";
            case 7:
                return "Julio";
            case 8:
                return "Agosto";
            case 9:
                return "Septiembre";
            case 10:
                return "Octubre";
            case 11:
                return "Noviembre";
            case 12:
                return "Diciembre";
            default:
                return "Mes inexistente";
        }
        
    }
}