namespace GestorGastos.Domain.Models;

public class Gasto
{
    public long Id { get; private set; }
    public string Descripcion { get; private set; }
    public decimal Monto { get; private set; }
    public DateTime Fecha { get; private set; }
    public bool Activo { get; private set; } = true;
    public long CategoriaId { get; private set; }
    public virtual Categoria Categoria { get; private set; }
    
    public long MetodoPagoId { get; private set; }
    public virtual MetodoPago MetodoPago { get; private set; }
    
    public long UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; }

    protected Gasto() { }

    public Gasto(string descripcion, decimal monto, DateTime fecha, long categoriaId, long metodoPagoId, long usuarioId)
    {
        if (monto <= 0) 
            throw new ArgumentException("El monto del gasto debe ser positivo.");

        Monto = monto;
        Fecha = fecha;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        MetodoPagoId = metodoPagoId;
        UsuarioId = usuarioId;
    }
    
    public void ActualizarDetalles(string descripcion, decimal monto, DateTime fecha, long categoriaId, long metodoPagoId)
    {
        if (monto <= 0) 
            throw new ArgumentException("El monto del gasto debe ser positivo.");
        
        Descripcion = descripcion;
        Monto = monto;
        Fecha = fecha;
        CategoriaId = categoriaId;
        MetodoPagoId = metodoPagoId;
    }

    public void CambiarEstado(bool estado)
    {
        Activo = estado;
    }
    
}