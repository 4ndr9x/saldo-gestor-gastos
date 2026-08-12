namespace GestorGastos.Domain.Exceptions;

public class NoEncontradoExcepcion : ErrorApi
{
    public NoEncontradoExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 404;
    }
}