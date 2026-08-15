using System.Security.Claims;
using GestorGastos.Domain.Exceptions;
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
        return Created(string.Empty, new { mensaje = "Usuario registrado exitosamente.", id = idUsuarioRegistrado });
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
        long idUsuario = ValidarUsuario();

        await _usuarioService.ActualizarPerfilAsync(idUsuario, usuarioRecibido);
        return Ok(new { mensaje = "Tu perfil ha sido actualizado correctamente." });
    }

    [Authorize]
    [HttpPut("cambiar-password")]
    public async Task<IActionResult> ActualizarPassword([FromBody] CambiarPasswordDto usuarioRecibido)
    {
        long idUsuario = ValidarUsuario();

        await _usuarioService.ActualizarPasswordAsync(idUsuario, usuarioRecibido);
        return Ok(new { mensaje = "Tu contraseña ha sido actualizada correctamente." });
    }

    [Authorize]
    [HttpDelete("cuenta")]
    public async Task<IActionResult> EliminarUsuario()
    {
        long idUsuario = ValidarUsuario();

        await _usuarioService.EliminarCuentaAsync(idUsuario);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult ObtenerMiPerfil()
    {
        long idUsuario = ValidarUsuario();
        string? correo = User.FindFirstValue(ClaimTypes.Email); 
        string? nombre = User.FindFirstValue(ClaimTypes.Name);
        
        RespuestaPerfilDto respuestaPerfil = new RespuestaPerfilDto();
        respuestaPerfil.Id = idUsuario;
        respuestaPerfil.Email = correo;
        respuestaPerfil.Nombre = nombre;

        return Ok(respuestaPerfil);
    }
    
    private long ValidarUsuario()
    {
        string? idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!long.TryParse(idString, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("Credenciales inválidas.");
        }

        return idConvertido;
    }

}