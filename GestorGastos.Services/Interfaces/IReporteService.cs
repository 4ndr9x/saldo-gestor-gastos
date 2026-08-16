using GestorGastos.Services.DTOs.DTOs_de_Reporte;

namespace GestorGastos.Services.Interfaces;

public interface IReporteService
{
    Task<ReporteMensualDto> GenerarReporteMensualAsync(long idUsuario, int month, int year);
}