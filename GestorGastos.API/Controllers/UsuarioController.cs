using System.Security.Claims;
using GestorGastos.Services.DTOs.DTOs_de_Usuario;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegistroDto usuarioRecibido)
    {
        long idUsuarioRegistrado = await _usuarioService.RegistrarUsuarioAsync(usuarioRecibido);
        return CreatedAtAction(nameof(ObtenerUsuarioPorId), new {id = idUsuarioRegistrado}, new {mensaje = "Usuario registrado"});
    }

    [HttpPost("login")]
    public async Task<IActionResult> IniciarSesion([FromBody] InicioSesionDto usuarioRecibido)
    {
        RespuestaAuthDto token = await _usuarioService.AutenticarUsuarioAsync(usuarioRecibido);
        return Ok(token);
    }

    [Authorize]
    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarNombrePerfil([FromBody] ActualizarPerfilDto usuarioRecibido)
    {

        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _usuarioService.ActualizarPerfilAsync(idString, usuarioRecibido);
        return Ok(new { mensaje = "Tu perfil a sido actualizado correctamente." });

    }

    [Authorize]
    [HttpPut("cambiar-password")]
    public async Task<IActionResult> ActualizarPassword([FromBody] CambiarPasswordDto usuarioRecibido)
    {
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _usuarioService.ActualizarPasswordAsync(idString, usuarioRecibido);
        return Ok(new { mensaje = "Tu contraseña a sido actualizada correctamente." });
    }

    [HttpGet("{id:long}")]
    public IActionResult ObtenerUsuarioPorId([FromRoute] long id)
    {
        return Ok(new { id = id, mensaje = "Ya se puede implementar la ruta del usuario" });
        // TODO: MEJORAR MAS ADELANTE
    }

    [Authorize]
    [HttpDelete("cuenta")]
    public async Task<IActionResult> EliminarUsuario()
    {
        
        string? idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _usuarioService.EliminarCuentaAsync(idString);
        return NoContent();

    }
    
    //TODO: ELIMINAR ESTE METODO DE PRUEBA
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult ObtenerPerfil()
    {
    
        string? idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? correo = User.FindFirst(ClaimTypes.Email)?.Value;
        string? nombre = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new 
        { 
            Mensaje = "¡Pasaste la seguridad con éxito!", 
            UsuarioId = idUsuario, 
            Nombre = nombre,
            Email = correo 
        });
    }

}