using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_Categoria;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriaController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto categoriaRecibida)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        RespuestaCategoriaDto respuestaCategoria = await _categoriaService.CrearCategoriaAsync(idString, categoriaRecibida);

        return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { idCategoria = respuestaCategoria.Id }, respuestaCategoria);
    }
    
    [HttpGet("{idCategoria:long}")]
    public async Task<IActionResult> ObtenerCategoriaPorId([FromRoute] long idCategoria)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var categoria = await _categoriaService.ObtenerCategoriaPorIdAsync(idCategoria, idString);

        return Ok(categoria);
    }
    
    [HttpGet("mis-categorias")]
    public async Task<IActionResult> ObtenerMisCategorias()
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var categorias = await _categoriaService.ObtenerCategoriasAsync(idString);

        return Ok(categorias);
    }

    [HttpDelete("{idCategoria:long}")]
    public async Task<IActionResult> EliminarCategoria([FromRoute] long idCategoria)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _categoriaService.EliminarCategoriaAsync(idCategoria, idString);

        return NoContent();
    }

    [HttpPut("{idCategoria:long}")]
    public async Task<IActionResult> ActualizarNombreCategoria([FromRoute] long idCategoria, [FromBody] ActualizarCategoriaDto categoriaRecibida)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _categoriaService.ActualizarCategoriaAsync(idCategoria, idString, categoriaRecibida);

        return NoContent();
    }

    [HttpPut("{idCategoria:long}/restaurar")]
    public async Task<IActionResult> RestaurarCategoria([FromRoute] long idCategoria)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _categoriaService.RestaurarCategoriaAsync(idCategoria, idString);

        return NoContent();
    }
}