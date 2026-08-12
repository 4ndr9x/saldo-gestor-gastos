namespace GestorGastos.Domain.Exceptions;

public class SinAutorizacionExcepcion : ErrorApi
{
    public SinAutorizacionExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 401;
        Detalles = new List<string>();
    }
}