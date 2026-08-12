namespace GestorGastos.Domain.Exceptions;

public class TareaCompletadaException : ErrorApi
{
    public TareaCompletadaException(string msg) : base(msg)
    {
        CodigoHttp = 409;
    }
}