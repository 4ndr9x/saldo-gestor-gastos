using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;

namespace GestorGastos.Data.Repository;

public class UsuarioRepository : IUsuarioRepository
{

    private readonly DbSistemaGastosContext _contexto;

    public UsuarioRepository(DbSistemaGastosContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Usuario?> BuscarUsuarioPorEmailAsync(string email)
    {
        return _contexto.Usuarios.FirstOrDefault(u => u.Email == email && u.Activo);
    }

    public async Task<Usuario?> BuscarUsuarioPorIdAsync(long idUsuario)
    {
        return _contexto.Usuarios.FirstOrDefault(u => u.Id == idUsuario && u.Activo);
    }

    public async Task ActualizarUsuarioAsync(Usuario usuarioRecibido)
    {
        _contexto.Usuarios.Update(usuarioRecibido);
        await _contexto.SaveChangesAsync();
    }

    public async Task RegistrarUsuarioAsync(Usuario usuarioRecibido)
    {
        _contexto.Usuarios.Add(usuarioRecibido);
        await _contexto.SaveChangesAsync();
    }
}