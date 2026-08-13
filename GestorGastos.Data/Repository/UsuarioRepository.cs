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

    public async Task RegistrarUsuario(Usuario usuarioRecibido)
    {
        _contexto.Usuarios.Add(usuarioRecibido);
        await _contexto.SaveChangesAsync();
    }
}