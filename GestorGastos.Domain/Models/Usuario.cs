namespace GestorGastos.Domain.Models;

public class Usuario
{
    public long Id { get; private set; }
    public string Nombre { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool Activo { get; private set; } = true;
    public string MonedaUsada { get; private set; }
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<MetodoPago> MetodosPago { get; set; } = new List<MetodoPago>();
    
    protected Usuario() { }

    public Usuario(string nombre, string email, string passwordHash)
    {
        Nombre = nombre;
        Email = email;
        PasswordHash = passwordHash;
    }

    public void CambiarNombre(string nuevoNombre)
    {
        Nombre = nuevoNombre;
    }

    public void CambiarPasswordHash(string passwordHashNuevo)
    {
        PasswordHash = passwordHashNuevo;
    }
    
    public void CambiarEstado(bool estado)
    {
        Activo = estado;
    }

    public void CambiarMonedaUsada(string monedaUsada)
    {
        MonedaUsada = monedaUsada;
    }

}