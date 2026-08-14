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

    public async Task<Usuario?> BuscarUsuarioPorEmail(string email)
    {
        return _contexto.Usuarios.FirstOrDefault(u => u.Email == email);
    }

    public async Task<Usuario?> BuscarUsuarioPorId(long idUsuario)
    {
        return _contexto.Usuarios.FirstOrDefault(u => u.Id == idUsuario);
    }

    public async Task ActualizarUsuarioAsync(Usuario usuario)
    {
        _contexto.Usuarios.Update(usuario);
        await _contexto.SaveChangesAsync();
    }

    public async Task RegistrarUsuarioAsync(Usuario usuarioRecibido)
    {
        _contexto.Usuarios.Add(usuarioRecibido);
        await _contexto.SaveChangesAsync();
    }

    public async Task EliminarUsuarioAsync(Usuario usuarioRecibido)
    {
        _contexto.Usuarios.Remove(usuarioRecibido);
        await _contexto.SaveChangesAsync();
    }
}