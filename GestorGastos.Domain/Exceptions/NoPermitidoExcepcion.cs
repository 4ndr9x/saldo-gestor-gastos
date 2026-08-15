namespace GestorGastos.Domain.Exceptions;

public class NoPermitidoExcepcion : ErrorApi
{
    public NoPermitidoExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 403;
    }
}