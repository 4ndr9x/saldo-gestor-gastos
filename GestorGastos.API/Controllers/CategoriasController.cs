using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_Categoria;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto categoriaRecibida)
    {
        string? idUsuario = ObtenerIdUsuario();

        RespuestaCategoriaDto respuestaCategoria = await _categoriaService.CrearCategoriaAsync(idUsuario, categoriaRecibida);

        return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { idCategoria = respuestaCategoria.Id }, respuestaCategoria);
    }
    
    [HttpGet("{idCategoria:long}")]
    public async Task<IActionResult> ObtenerCategoriaPorId([FromRoute] long idCategoria)
    {
        string? idUsuario = ObtenerIdUsuario();

        var categoria = await _categoriaService.ObtenerCategoriaPorIdAsync(idCategoria, idUsuario);

        return Ok(categoria);
    }
    
    [HttpGet]
    public async Task<IActionResult> ObtenerMisCategorias()
    {
        string? idUsuario = ObtenerIdUsuario();

        var categorias = await _categoriaService.ObtenerCategoriasAsync(idUsuario);

        return Ok(categorias);
    }

    [HttpDelete("{idCategoria:long}")]
    public async Task<IActionResult> EliminarCategoria([FromRoute] long idCategoria)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _categoriaService.EliminarCategoriaAsync(idCategoria, idUsuario);

        return NoContent();
    }

    [HttpPut("{idCategoria:long}")]
    public async Task<IActionResult> ActualizarNombreCategoria([FromRoute] long idCategoria, [FromBody] ActualizarCategoriaDto categoriaRecibida)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _categoriaService.ActualizarCategoriaAsync(idCategoria, idUsuario, categoriaRecibida);

        return NoContent();
    }

    [HttpPatch("{idCategoria:long}/restaurar")]
    public async Task<IActionResult> RestaurarCategoria([FromRoute] long idCategoria)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _categoriaService.RestaurarCategoriaAsync(idCategoria, idUsuario);

        return NoContent();
    }
    
    private string? ObtenerIdUsuario()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}