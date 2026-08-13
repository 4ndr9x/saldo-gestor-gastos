namespace GestorGastos.Domain.Exceptions;

public class DatosErroneosExcepcion : ErrorApi
{
    public DatosErroneosExcepcion(string msg, List<string> detalles) : base(msg)
    {
        CodigoHttp = 400;
        Detalles = detalles;
    }
}