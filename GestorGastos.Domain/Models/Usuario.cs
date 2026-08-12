namespace GestorGastos.Domain.Models;

public class Usuario
{
    public long Id { get; private set; }
    public string Nombre { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    
    protected Usuario() { }

    public Usuario(string nombre, string email, string passwordHash)
    {
        Nombre = nombre;
        Email = email;
        PasswordHash = passwordHash;
    }
    
}