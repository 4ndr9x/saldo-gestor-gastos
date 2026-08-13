namespace GestorGastos.Domain.Exceptions;

public class CredencialesInvalidasExcepcion : ErrorApi
{
    public CredencialesInvalidasExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 401;
    }
}