namespace GestorGastos.Domain.Models;

public class Presupuesto
{
    public long Id { get; private set; }
    public long UsuarioId { get; private set; }
    public long CategoriaId { get; private set; }
    
    public int Month { get; private set; }
    public int Year { get; private set; }
    public decimal MontoMaximo { get; private set; }
    
    public bool Activo { get; private set; } = true;
    
    public virtual Categoria? Categoria { get; private set; }
    public virtual Usuario? Usuario { get; private set; }

    public Presupuesto(long usuarioId, long categoriaId, int month, int year, decimal montoMaximo)
    {
        UsuarioId = usuarioId;
        CategoriaId = categoriaId;
        Month = month;
        Year = year;
        MontoMaximo = montoMaximo;
    }

    public void CambiarEstado(bool estado)
    {
        Activo = estado;
    }

    public void CambiarMontoMaximo(decimal montoMaximo)
    {
        MontoMaximo = montoMaximo;
    }
} 