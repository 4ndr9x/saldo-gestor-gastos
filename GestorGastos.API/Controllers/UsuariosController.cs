using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
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

    [HttpGet]
    public async Task<List<Usuario>> ObtenerTodos()
    {
        return await _usuarioService.ObtenerTodosUsuariosAsync();
    }
}