namespace GestorGastos.Domain.Models;

public class MetodoPago
{
    public long Id { get; private set; }
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }
    public string Icono { get; private set; } = string.Empty;
    public long UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; }

    protected MetodoPago() { }

    public MetodoPago(string nombre, long usuarioId)
    {
        Nombre = nombre;
        Activo = true;
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

    public void AsignarIcono(string linkIcono)
    {
        Icono = linkIcono;
    }
}