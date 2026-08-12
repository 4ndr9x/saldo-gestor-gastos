namespace GestorGastos.Domain.Exceptions;

public abstract class ErrorApi : Exception
{
    public int CodigoHttp { get; protected set; }
    public List<string> Detalles { get; protected set; } = new List<string>();

    protected ErrorApi(string msg) : base(msg)
    {
        
    }
}