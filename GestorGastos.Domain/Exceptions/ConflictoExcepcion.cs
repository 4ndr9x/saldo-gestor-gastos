namespace GestorGastos.Domain.Exceptions;

public class ConflictoExcepcion : ErrorApi
{
    public ConflictoExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 409;
    }
}