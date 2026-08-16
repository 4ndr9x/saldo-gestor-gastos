using GestorGastos.Services.DTOs.DTOs_de_Reporte;

namespace GestorGastos.Services.Interfaces;

public interface IExportacionService
{
    byte[] GenerarReporteTxt(ReporteMensualDto reporteRecibido);
    byte[] GenerarReporteJson(ReporteMensualDto reporteRecibido);
    byte[] GenerarReporteExcel(ReporteMensualDto reporteRecibido);
    byte[] GenerarPlantillaImportacionExcel();
}