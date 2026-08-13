namespace GestorGastos.Domain.Exceptions;

public class DatosErroneosException : ErrorApi
{
    public DatosErroneosException(string msg, List<string> detalles) : base(msg)
    {
        CodigoHttp = 400;
        Detalles = detalles;
    }
}