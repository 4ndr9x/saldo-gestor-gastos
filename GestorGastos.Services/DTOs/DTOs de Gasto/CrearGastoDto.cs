using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Gasto;

public class CrearGastoDto
{
    [Required(ErrorMessage = "El concepto es obligatorio.")]
    [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string Concepto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime Fecha { get; set; }
    public long CategoriaId { get; set; }
    public long MetodoPagoId { get; set; }
}