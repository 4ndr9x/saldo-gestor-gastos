using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using GestorGastos.Services.DTOs.DTOs_de_Reporte;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class ExportacionService : IExportacionService
{
    public byte[] GenerarReporteTxt(ReporteMensualDto reporte)
    {
        var sb = new StringBuilder();

        sb.AppendLine("========================================");
        sb.AppendLine($" REPORTE DE GASTOS: MES {reporte.Month} - AÑO {reporte.Year}");
        sb.AppendLine("========================================");
        sb.AppendLine($"Total Gastado: {reporte.TotalGastado:C}");
        sb.AppendLine($"Total Mes Anterior: {reporte.TotalMesAnterior:C}");
        sb.AppendLine($"Diferencia: {reporte.MensajeComparacion}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("TOP CATEGORÍAS:");
        
        foreach (var cat in reporte.TopCategorias)
        {
            sb.AppendLine($"- {cat.NombreCategoria}: {cat.TotalGastado:C}");
        }
        sb.AppendLine("========================================");
        
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
    
    public byte[] GenerarReporteJson(ReporteMensualDto reporte)
    {
        var opciones = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };
        
        string jsonString = JsonSerializer.Serialize(reporte, opciones);
        
        return Encoding.UTF8.GetBytes(jsonString);
    }
    
    public byte[] GenerarReporteExcel(ReporteMensualDto reporte)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Reporte {reporte.Month}-{reporte.Year}");
        
        worksheet.Cell(1, 1).Value = "Resumen Mensual de Gastos";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 2).Merge();
        
        // informacion general
        worksheet.Cell(3, 1).Value = "Mes:";
        worksheet.Cell(3, 2).Value = reporte.Month;
        worksheet.Cell(3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        worksheet.Cell(4, 1).Value = "Año:";
        worksheet.Cell(4, 2).Value = reporte.Year;
        worksheet.Cell(4, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        
        worksheet.Cell(5, 1).Value = "Total Gastado:";
        worksheet.Cell(5, 2).Value = reporte.TotalGastado;
        worksheet.Cell(5, 2).Style.NumberFormat.Format = "$ #,##0.00";
        worksheet.Cell(5, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        
        // comparacion
        worksheet.Cell(6, 1).Value = "Comparación:";
        worksheet.Cell(6, 2).Value = reporte.MensajeComparacion;
        
        // categorias
        worksheet.Cell(8, 1).Value = "Categoría";
        worksheet.Cell(8, 2).Value = "Total Gastado";
        worksheet.Range(8, 1, 8, 2).Style.Font.Bold = true;
        worksheet.Range(8, 1, 8, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        int filaActual = 9;
        foreach (var categoria in reporte.TopCategorias)
        {
            worksheet.Cell(filaActual, 1).Value = categoria.NombreCategoria;
            worksheet.Cell(filaActual, 2).Value = categoria.TotalGastado;
            worksheet.Cell(filaActual, 2).Style.NumberFormat.Format = "$ #,##0.00";
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
        worksheet.Cell(1, 4).Value = "ID Categoría";
        worksheet.Cell(1, 5).Value = "ID Método Pago";
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