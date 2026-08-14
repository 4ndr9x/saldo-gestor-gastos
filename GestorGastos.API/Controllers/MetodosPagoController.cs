using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_MetodoPago;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

public class MetodoPagoController : ControllerBase
{
    private readonly IMetodoPagoService _metodoPagoService;

    public MetodosPagoController(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CrearMetodoPago([FromBody] CrearMetodoPagoDto dtoRecibido)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        RespuestaMetodoPagoDto respuesta = await _metodoPagoService.CrearMetodoPagoAsync(idString, dtoRecibido);

        // Apuntamos correctamente al nombre del método GET y a su parámetro idMetodoPago
        return CreatedAtAction(nameof(ObtenerMetodoPagoPorId), new { idMetodoPago = respuesta.Id }, respuesta);
    }
    
    [HttpGet("{idMetodoPago:long}")]
    public async Task<IActionResult> ObtenerMetodoPagoPorId([FromRoute] long idMetodoPago)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var metodoPago = await _metodoPagoService.ObtenerMetodoPagoPorIdAsync(idMetodoPago, idString);

        return Ok(metodoPago);
    }
    
    [HttpGet("mis-metodos")]
    public async Task<IActionResult> ObtenerMisMetodosPago()
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var metodos = await _metodoPagoService.ObtenerMetodosPagoAsync(idString);

        return Ok(metodos);
    }

    [HttpDelete("{idMetodoPago:long}")]
    public async Task<IActionResult> EliminarMetodoPago([FromRoute] long idMetodoPago)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _metodoPagoService.EliminarMetodoPagoAsync(idMetodoPago, idString);

        return NoContent();
    }

    [HttpPut("{idMetodoPago:long}")]
    public async Task<IActionResult> ActualizarNombreMetodoPago([FromRoute] long idMetodoPago, [FromBody] ActualizarMetodoPagoDto dtoRecibido)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _metodoPagoService.ActualizarMetodoPagoAsync(idMetodoPago, idString, dtoRecibido);

        return NoContent();
    }

    [HttpPut("{idMetodoPago:long}/restaurar")]
    public async Task<IActionResult> RestaurarMetodoPago([FromRoute] long idMetodoPago)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _metodoPagoService.RestaurarMetodoPagoAsync(idMetodoPago, idString);

        return NoContent();
    }
}