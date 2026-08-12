namespace GestorGastos.Domain.Models;

public class MensajeError
{
    public int Codigo { get; private set; }
    public string Mensaje { get; private set; }
    public List<string> Detalles { get; private set; }
    public string RequestId { get; private set; }

    public MensajeError(int codigo, string mensaje, List<string> detalles, string requestId)
    {
        Codigo = codigo;
        Mensaje = mensaje;
        Detalles = detalles;
        RequestId = requestId;

    }
}