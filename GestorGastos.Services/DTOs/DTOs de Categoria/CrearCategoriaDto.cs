using System.ComponentModel.DataAnnotations;

namespace GestorGastos.Services.DTOs.DTOs_de_Categoria;

public class CrearCategoriaDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
}