using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_MetodoPago;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MetodosPagoController : ControllerBase
{
    private readonly IMetodoPagoService _metodoPagoService;

    public MetodosPagoController(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CrearMetodoPago([FromBody] CrearMetodoPagoDto dtoRecibido)
    {
        string? idUsuario = ObtenerIdUsuario();

        RespuestaMetodoPagoDto respuesta = await _metodoPagoService.CrearMetodoPagoAsync(idUsuario, dtoRecibido);
        
        return CreatedAtAction(nameof(ObtenerMetodoPagoPorId), new { idMetodoPago = respuesta.Id }, respuesta);
    }
    
    [HttpGet("{idMetodoPago:long}")]
    public async Task<IActionResult> ObtenerMetodoPagoPorId([FromRoute] long idMetodoPago)
    {
        string? idUsuario = ObtenerIdUsuario();

        var metodoPago = await _metodoPagoService.ObtenerMetodoPagoPorIdAsync(idMetodoPago, idUsuario);

        return Ok(metodoPago);
    }
    
    [HttpGet]
    public async Task<IActionResult> ObtenerMisMetodosPago()
    {
        string? idUsuario = ObtenerIdUsuario();

        var metodos = await _metodoPagoService.ObtenerMetodosPagoAsync(idUsuario);

        return Ok(metodos);
    }

    [HttpDelete("{idMetodoPago:long}")]
    public async Task<IActionResult> EliminarMetodoPago([FromRoute] long idMetodoPago)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _metodoPagoService.EliminarMetodoPagoAsync(idMetodoPago, idUsuario);

        return NoContent();
    }

    [HttpPut("{idMetodoPago:long}")]
    public async Task<IActionResult> ActualizarNombreMetodoPago([FromRoute] long idMetodoPago, [FromBody] ActualizarMetodoPagoDto dtoRecibido)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _metodoPagoService.ActualizarMetodoPagoAsync(idMetodoPago, idUsuario, dtoRecibido);

        return NoContent();
    }

    [HttpPatch("{idMetodoPago:long}/restaurar")]
    public async Task<IActionResult> RestaurarMetodoPago([FromRoute] long idMetodoPago)
    {
        string? idUsuario = ObtenerIdUsuario();

        await _metodoPagoService.RestaurarMetodoPagoAsync(idMetodoPago, idUsuario);

        return NoContent();
    }
    
    private string? ObtenerIdUsuario()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}