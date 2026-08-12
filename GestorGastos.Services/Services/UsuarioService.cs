using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;

namespace GestorGastos.Services.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repositorio;

    public UsuarioService(IUsuarioRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<Usuario>> ObtenerTodosUsuariosAsync()
    {

        return await _repositorio.BuscarTodosLosUsuarios();

    }
}