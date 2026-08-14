namespace GestorGastos.Domain.Exceptions;

public class TareaCompletadaExcepcion : ErrorApi
{
    public TareaCompletadaExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 409;
    }
}