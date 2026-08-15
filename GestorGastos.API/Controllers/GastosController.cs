using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_Gasto;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GastosController : ControllerBase
{
    private readonly IGastoService _gastoService;

    public GastosController(IGastoService gastoService)
    {
        _gastoService = gastoService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearGasto([FromBody] CrearGastoDto gastoRecibido)
    {
        string? idUsuario = ObtenerIdUsuario();
        RespuestaGastoDto respuesta = await _gastoService.CrearGastoAsync(idUsuario, gastoRecibido);
        
        return CreatedAtAction(nameof(ObtenerGastoPorId), new { idGasto = respuesta.Id }, respuesta);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodosLosGastos()
    {
        string? idUsuario = ObtenerIdUsuario();
        IEnumerable<RespuestaGastoDto> gastos = await _gastoService.ObtenerTodosGastosAsync(idUsuario);
        
        return Ok(gastos); // Retorna 200 OK con la lista
    }

    [HttpGet("{idGasto:long}")]
    public async Task<IActionResult> ObtenerGastoPorId([FromRoute] long idGasto)
    {
        string? idUsuario = ObtenerIdUsuario();
        RespuestaGastoDto gasto = await _gastoService.ObtenerGastoPorIdAsync(idGasto, idUsuario);
        
        return Ok(gasto);
    }

    [HttpPut("{idGasto:long}")]
    public async Task<IActionResult> ActualizarGasto([FromRoute] long idGasto, [FromBody] ActualizarGastoDto gastoRecibido)
    {
        string? idUsuario = ObtenerIdUsuario();
        await _gastoService.ActualizarGastoAsync(idGasto, idUsuario, gastoRecibido);
        
        return NoContent();
    }

    [HttpDelete("{idGasto:long}")]
    public async Task<IActionResult> EliminarGasto([FromRoute] long idGasto)
    {
        string? idUsuario = ObtenerIdUsuario();
        await _gastoService.EliminarGastoAsync(idGasto, idUsuario);
        
        return NoContent();
    }

    [HttpPatch("{idGasto:long}/restaurar")]
    public async Task<IActionResult> RestaurarGasto([FromRoute] long idGasto)
    {
        string? idUsuario = ObtenerIdUsuario();
        await _gastoService.RestaurarGastoAsync(idGasto, idUsuario);
        
        return NoContent();
    }
    
    private string? ObtenerIdUsuario()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}