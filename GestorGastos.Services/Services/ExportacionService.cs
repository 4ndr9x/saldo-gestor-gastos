using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using GestorGastos.Services.DTOs.DTOs_de_Reporte;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class ExportacionService : IExportacionService
{

    public byte[] GenerarReporteTxt(ReporteMensualDto reporteRecibido)
    {
        var sb = new StringBuilder();
        

        sb.AppendLine($"Generado por: {reporteRecibido.NombreUsuario}");
        sb.AppendLine($"Moneda del reporte: {reporteRecibido.Moneda}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("========================================");
        sb.AppendLine($" REPORTE DE GASTOS: MES {reporteRecibido.Month} - AÑO {reporteRecibido.Year}");
        sb.AppendLine("========================================");
        sb.AppendLine($"Total Gastado: {reporteRecibido.TotalGastado:C}");
        sb.AppendLine($"Total Mes Anterior: {reporteRecibido.TotalMesAnterior:C}");
        sb.AppendLine($"Diferencia: {reporteRecibido.MensajeComparacion}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("TOP CATEGORÍAS:");
        
        foreach (var categoria in reporteRecibido.TopCategorias)
        {
            sb.AppendLine($"- {categoria.NombreCategoria}: {categoria.TotalGastado:C}");
        }
        sb.AppendLine("========================================");
        
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
    
    public byte[] GenerarReporteJson(ReporteMensualDto reporteRecibido)
    {
        var opciones = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };
        
        string jsonString = JsonSerializer.Serialize(reporteRecibido, opciones);
        
        return Encoding.UTF8.GetBytes(jsonString);
    }
    
    public byte[] GenerarReporteExcel(ReporteMensualDto reporteRecibido)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Reporte {reporteRecibido.Month}-{reporteRecibido.Year}");
        
        worksheet.Cell(1, 1).Value = "Resumen Mensual de Gastos";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 2).Merge();
        
        // informacion general
        worksheet.Cell(3, 1).Value = "Generado por:";
        worksheet.Cell(3, 2).Value = reporteRecibido.NombreUsuario; 
        
        worksheet.Cell(4, 1).Value = "Moneda del reporte:";
        worksheet.Cell(4, 2).Value = reporteRecibido.Moneda;
        
        worksheet.Cell(6, 1).Value = "Mes:";
        worksheet.Cell(6, 2).Value = reporteRecibido.Month;
        worksheet.Cell(6, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        worksheet.Cell(7, 1).Value = "Año:";
        worksheet.Cell(7, 2).Value = reporteRecibido.Year;
        worksheet.Cell(7, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        
        worksheet.Cell(8, 1).Value = "Total Gastado:";
        worksheet.Cell(8, 2).Value = reporteRecibido.TotalGastado;
        
        worksheet.Cell(8, 2).Style.NumberFormat.Format = $"_(\"{reporteRecibido.Moneda}\"* #,##0.00_)"; 
        worksheet.Cell(8, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        
        // comparacion
        worksheet.Cell(9, 1).Value = "Comparación:";
        worksheet.Cell(9, 2).Value = reporteRecibido.MensajeComparacion;
        
        // categorias
        worksheet.Cell(11, 1).Value = "Categoría";
        worksheet.Cell(11, 2).Value = "Total Gastado";
        worksheet.Range(11, 1, 11, 2).Style.Font.Bold = true;
        worksheet.Range(11, 1, 11, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        int filaActual = 12;
        foreach (var categoria in reporteRecibido.TopCategorias)
        {
            worksheet.Cell(filaActual, 1).Value = categoria.NombreCategoria;
            worksheet.Cell(filaActual, 2).Value = categoria.TotalGastado;
            
            worksheet.Cell(filaActual, 2).Style.NumberFormat.Format = $"_(\"{reporteRecibido.Moneda}\"* #,##0.00_)";
            worksheet.Cell(filaActual, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            
            filaActual++;
        }

        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarPlantillaImportacionExcel()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Plantilla_Importacion");
        
        worksheet.Cell(1, 1).Value = "Fecha (YYYY-MM-DD)";
        worksheet.Cell(1, 2).Value = "Monto";
        worksheet.Cell(1, 3).Value = "Descripción";
        worksheet.Cell(1, 4).Value = "Categoría";
        worksheet.Cell(1, 5).Value = "Método Pago";
        worksheet.Cell(1, 6).Value = "Concepto";
        worksheet.Cell(1, 7).Value = "Moneda (Ej: DOP, USD)";
        
        var rangoEncabezados = worksheet.Range("A1:G1");
        rangoEncabezados.Style.Font.Bold = true;
        rangoEncabezados.Style.Font.FontColor = XLColor.White;
        rangoEncabezados.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        rangoEncabezados.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}