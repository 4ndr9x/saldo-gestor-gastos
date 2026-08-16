namespace GestorGastos.Domain.Exceptions;

public class ErrorConexionApi : ErrorApi
{
    public ErrorConexionApi(string msg) : base(msg)
    {
        CodigoHttp = 500;
    }
}