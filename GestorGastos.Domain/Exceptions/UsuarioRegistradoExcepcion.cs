namespace GestorGastos.Domain.Exceptions;

public class UsuarioRegistradoExcepcion : ErrorApi
{
    public UsuarioRegistradoExcepcion(string msg) : base(msg)
    {
        CodigoHttp = 409;
    }
}