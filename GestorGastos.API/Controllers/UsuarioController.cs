using System.Security.Claims;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Services.DTOs.DTOs_de_Usuario;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegistroDto usuarioRecibido)
    {
        long idUsuarioRegistrado = await _usuarioService.RegistrarUsuarioAsync(usuarioRecibido);
        return Created(string.Empty, new { mensaje = "Usuario registrado exitosamente.", id = idUsuarioRegistrado });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> IniciarSesion([FromBody] InicioSesionDto usuarioRecibido)
    {
        RespuestaAuthDto token = await _usuarioService.AutenticarUsuarioAsync(usuarioRecibido);
        return Ok(token);
    }
    
    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarNombrePerfil([FromBody] ActualizarPerfilDto usuarioRecibido)
    {
        long idUsuario = ObtenerIdUsuario();

        await _usuarioService.ActualizarNombreAsync(idUsuario, usuarioRecibido);
        return Ok(new { mensaje = "Tu perfil ha sido actualizado correctamente." });
    }
    
    [HttpPut("cambiar-password")]
    public async Task<IActionResult> ActualizarPassword([FromBody] CambiarPasswordDto usuarioRecibido)
    {
        long idUsuario = ObtenerIdUsuario();

        await _usuarioService.ActualizarPasswordAsync(idUsuario, usuarioRecibido);
        return Ok(new { mensaje = "Tu contraseña ha sido actualizada correctamente." });
    }
    
    [HttpPut("cambiar-moneda")]
    public async Task<IActionResult> ActualizarMoneda([FromBody] ActualizarMonedaUsadaDto usuarioRecibido)
    {
        long idUsuario = ObtenerIdUsuario();

        await _usuarioService.ActualizarMonedaUsadaAsync(idUsuario, usuarioRecibido);
        return Ok(new { mensaje = "Tu moneda ha sido actualizada correctamente." });
    }
    
    [HttpDelete("cuenta")]
    public async Task<IActionResult> EliminarUsuario()
    {
        long idUsuario = ObtenerIdUsuario();

        await _usuarioService.EliminarCuentaAsync(idUsuario);
        return NoContent();
    }
    
    [HttpGet("perfil")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        long idUsuario = ObtenerIdUsuario();

        RespuestaPerfilDto respuestaPerfil = await _usuarioService.ObtenerMiPerfil(idUsuario);

        return Ok(respuestaPerfil);
    }
    
    private long ObtenerIdUsuario()
    {
        string? idString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idString) || !long.TryParse(idString, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("El token no contiene un identificador válido.");
        }

        return idConvertido;
    }

}