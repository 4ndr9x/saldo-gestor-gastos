using System.Text.Json;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Models;

namespace GestorGastos.API.Middlewares;

public class GestorErroresMiddleware
{
    private readonly RequestDelegate _siguiente;
    
    public GestorErroresMiddleware(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception e)
        {
            await ProcesarErrorAsync(contexto, e);
        }
    }

    private static async Task ProcesarErrorAsync(HttpContext contexto, Exception excepcion)
    {
        
        if (contexto.Response.HasStarted)
        {
            Console.WriteLine($"La respuesta ya inicio. No se pudo aplicar el formato uniforme: {excepcion}");
            return; 
        }
        //contexto.Response.Clear();
        
        MensajeError mensajeError;
        contexto.Response.ContentType = "application/json";
        string requestId = contexto.Items["X-Request-Id"]?.ToString() ?? "RequestID no encontrado";
        
        if (excepcion is ErrorApi apiException)
        {
            mensajeError = new MensajeError(apiException.CodigoHttp, apiException.Message, apiException.Detalles, requestId);
        }
        else
        {
            contexto.Response.StatusCode = 500;
            
            mensajeError = new MensajeError(contexto.Response.StatusCode, "Paso un error inesperado en el servidor", new List<string>(), requestId);
            Console.WriteLine(excepcion);
        }

        JsonSerializerOptions formato = new JsonSerializerOptions { WriteIndented = true };

        string mensajeSerializado = JsonSerializer.Serialize(mensajeError, formato);
        await contexto.Response.WriteAsync(mensajeSerializado);


    }

}