namespace GestorGastos.Domain.Models;

public class Categoria
{
    public long Id { get; private set; }
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }
    
    // Relación obligatoria con el Usuario
    public long UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; }

    protected Categoria() { }

    public Categoria(string nombre, long usuarioId)
    {
        Nombre = nombre;
        Activo = true; // El estado inicial siempre es activo
        UsuarioId = usuarioId;
    }

    public void ActualizarNombre(string nuevoNombre)
    {
        Nombre = nuevoNombre;
    }

    public void CambiarEstado(bool estado)
    {
        Activo = estado;
    }
}