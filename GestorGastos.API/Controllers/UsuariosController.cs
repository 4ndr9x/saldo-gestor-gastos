using System.Security.Claims;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs;
using GestorGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    public UsuariosController(IUsuarioService usuarioService)
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

    [HttpGet("{id:long}")]
    public IActionResult ObtenerUsuarioPorId([FromRoute] long id)
    {
        return Ok(new { id = id, mensaje = "Ya se puede implementar la ruta del usuario" });
        // TODO: MEJORAR MAS ADELANTE
    }
    
    //TODO: ELIMINAR ESTE METODO DE PRUEBA
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult ObtenerPerfilSeguro()
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