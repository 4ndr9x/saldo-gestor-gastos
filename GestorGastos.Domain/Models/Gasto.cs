using GestorGastos.Domain.Exceptions;

namespace GestorGastos.Domain.Models;

public class Gasto
{
    public long Id { get; private set; }
    public string Concepto { get; private set; }
    public string Descripcion { get; private set; }
    public decimal MontoFinal { get; private set; }
    
    public decimal MontoOriginal { get; private set; }
    public string Moneda { get; private set; } = "USD";
    public decimal TasaCambio { get; private set; } = 1.0m;
    
    public DateTime Fecha { get; private set; }
    public bool Activo { get; private set; } = true;
    public long CategoriaId { get; private set; }
    public virtual Categoria Categoria { get; private set; }
    
    public long MetodoPagoId { get; private set; }
    public virtual MetodoPago MetodoPago { get; private set; }
    
    public long UsuarioId { get; private set; }
    public virtual Usuario Usuario { get; private set; }

    protected Gasto() { }

    public Gasto(string concepto, string descripcion, decimal montoOriginal, 
        string moneda, decimal tasaCambio, DateTime fecha, long categoriaId, long metodoPagoId, long usuarioId)
    {
        if (montoOriginal <= 0) 
            throw new ArgumentException("El monto del gasto debe ser positivo.");

        Concepto = concepto;
        Descripcion = descripcion;
        MontoOriginal = montoOriginal;
        Moneda = moneda.ToUpper().Trim();
        
        TasaCambio = tasaCambio; 
        MontoFinal = MontoOriginal * TasaCambio;

        Fecha = fecha;
        CategoriaId = categoriaId;
        MetodoPagoId = metodoPagoId;
        UsuarioId = usuarioId;
    }
    
    public void ActualizarDetalles(string concepto, string descripcion, decimal montoOriginal, 
        string moneda, decimal tasaCambio, DateTime fecha, long categoriaId, long metodoPagoId)
    {
        Concepto = concepto;
        Descripcion = descripcion;
        MontoOriginal = montoOriginal;
        Moneda = moneda.ToUpper().Trim();
        TasaCambio = tasaCambio;
        MontoFinal = MontoOriginal * TasaCambio;
        Fecha = fecha;
        CategoriaId = categoriaId;
        MetodoPagoId = metodoPagoId;
    }

    public void ActualizarTasaCambio(decimal tasaNueva)
    {
        TasaCambio = tasaNueva;
    }

    public void RecalcularConversionMoneda(decimal nuevaTasa)
    {
        TasaCambio = nuevaTasa;
        MontoFinal = MontoOriginal * nuevaTasa;
    }

    public void CambiarEstado(bool estado)
    {
        Activo = estado;
    }
    
}