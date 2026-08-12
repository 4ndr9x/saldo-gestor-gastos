using GestorGastos.Data.Context;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;

namespace GestorGastos.Data.Repository;

public class UsuarioRepository : IUsuarioRepository
{

    private readonly DbSistemaGastosContext Contexto;

    public UsuarioRepository(DbSistemaGastosContext contexto)
    {
        Contexto = contexto;
    }

    public async Task<List<Usuario>> BuscarTodosLosUsuarios()
    {
        return Contexto.Usuarios.ToList();
    }
}