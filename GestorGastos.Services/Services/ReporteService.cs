using GestorGastos.Domain.Interfaces;
using GestorGastos.Services.DTOs.DTOs_de_Reporte;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class ReporteService : IReporteService
{
    private readonly IGastoRepository _gastoRepositorio;

    public ReporteService(IGastoRepository gastoRepositorio)
    {
        _gastoRepositorio = gastoRepositorio;
    }
    
    public async Task<ReporteMensualDto> GenerarReporteMensualAsync(long idUsuario, int month, int year)
    {
        DateTime fechaActual = new DateTime(year, month, 1);
        DateTime fechaAnterior = fechaActual.AddMonths(-1);
        int monthAnterior = fechaAnterior.Month;
        int yearAnterior = fechaAnterior.Year;
        
        decimal totalMesActual = await _gastoRepositorio.ObtenerTotalGastadoPorMesAsync(idUsuario, month, year);
        decimal totalMesAnterior = await _gastoRepositorio.ObtenerTotalGastadoPorMesAsync(idUsuario, monthAnterior, yearAnterior);
        
        var categoriasTupla = await _gastoRepositorio.ObtenerTopCategoriasDelMesAsync(idUsuario, month, year, 5);
        
        var topCategorias = categoriasTupla.Select(c => new CategoriaReporteDto
        {
            NombreCategoria = c.NombreCategoria,
            TotalGastado = c.TotalGastado
        }).ToList();
        
        decimal diferencia = totalMesActual - totalMesAnterior;
        string mensaje;
        
        if (diferencia > 0) 
        {
            mensaje = $"Has gastado {Math.Abs(diferencia):C} más que el mes anterior.";
        }
        else if (diferencia == 0)
        {
            mensaje = "No hubo diferencia en comparación con el mes anterior.";
        }
        else
        {
            mensaje = $"Ahorraste {Math.Abs(diferencia):C} en comparación con el mes anterior.";
        }

        return new ReporteMensualDto
        {
            Month = month,
            Year = year,
            TotalGastado = totalMesActual,
            TotalMesAnterior = totalMesAnterior,
            Diferencia = diferencia,
            MensajeComparacion = mensaje,
            TopCategorias = topCategorias
        };
    }
}