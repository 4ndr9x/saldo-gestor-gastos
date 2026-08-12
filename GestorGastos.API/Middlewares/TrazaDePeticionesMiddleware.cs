using System.Diagnostics;

namespace GestorGastos.API.Middlewares;

public class TrazaDePeticionesMiddleware
{
    private readonly RequestDelegate _siguiente;

    public TrazaDePeticionesMiddleware(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {

        var reloj = Stopwatch.StartNew();

        await _siguiente(contexto);
        
        reloj.Stop();
        
        Console.WriteLine($"[Request {contexto.Request.Method}] | Ruta: {contexto.Request.Path} | Codigo de estado: {contexto.Response.StatusCode} | Duracion: {reloj.ElapsedMilliseconds} ms");

    }
}