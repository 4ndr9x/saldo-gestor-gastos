namespace GestorGastos.Domain.Exceptions;

public class CredencialesInvalidasException : ErrorApi
{
    public CredencialesInvalidasException(string msg) : base(msg)
    {
        CodigoHttp = 401;
    }
}