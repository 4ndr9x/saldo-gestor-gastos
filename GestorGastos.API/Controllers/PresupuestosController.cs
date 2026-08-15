using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_Presupuesto;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PresupuestosController : ControllerBase
{
    private readonly IPresupuestoService _presupuestoService;

    public PresupuestosController(IPresupuestoService presupuestoService)
    {
        _presupuestoService = presupuestoService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearPresupuesto([FromBody] CrearPresupuestoDto dtoRecibido)
    {
        long idUsuario = ObtenerIdUsuario();

        RespuestaPresupuestoDto respuesta = await _presupuestoService.CrearPresupuestoAsync(idUsuario, dtoRecibido);
        
        return CreatedAtAction(nameof(ObtenerPresupuestoPorId), new { idPresupuesto = respuesta.Id }, respuesta);
    }
    
    [HttpGet]
    public async Task<IActionResult> ObtenerResumenDelMes([FromQuery] int month, [FromQuery] int year)
    {
        long idUsuario = ObtenerIdUsuario();

        IEnumerable<RespuestaPresupuestoDto> resumen = await _presupuestoService.ObtenerResumenPresupuestosDelMesAsync(idUsuario, month, year);

        return Ok(resumen);
    }
    
    [HttpPatch("{idPresupuesto:long}")]
    public async Task<IActionResult> ActualizarPresupuesto([FromRoute] long idPresupuesto, [FromBody] ActualizarPresupuestoDto dtoRecibido)
    {
        long idUsuario = ObtenerIdUsuario();

        await _presupuestoService.ActualizarPresupuestoAsync(idPresupuesto, idUsuario, dtoRecibido);

        return NoContent();
    }

    [HttpDelete("{idPresupuesto:long}")]
    public async Task<IActionResult> EliminarPresupuesto([FromRoute] long idPresupuesto)
    {
        long idUsuario = ObtenerIdUsuario();

        await _presupuestoService.EliminarPresupuestoAsync(idPresupuesto, idUsuario);

        return NoContent();
    }
    
    [HttpGet("{idPresupuesto:long}")]
    public async Task<IActionResult> ObtenerPresupuestoPorId([FromRoute] long idPresupuesto)
    {
        long idUsuario = ObtenerIdUsuario();
    
        RespuestaPresupuestoDto presupuesto = await _presupuestoService.ObtenerPresupuestoPorIdAsync(idPresupuesto, idUsuario);
    
        return Ok(presupuesto);
    }

    private long ObtenerIdUsuario()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(claimId))
        {
            throw new UnauthorizedAccessException("El token no contiene un identificador válido.");
        }
            
        return long.Parse(claimId);
    }
}