namespace GestorGastos.Domain.Models;

public class MetodoPago
{
    public long Id { get; private set; }
    public string Nombre { get; private set; }
    public string Icono { get; private set; }
    
    public long UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; }

    protected MetodoPago() { }

    public MetodoPago(string nombre, string icono, long usuarioId)
    {
        Nombre = nombre;
        Icono = icono;
        UsuarioId = usuarioId;
    }
}