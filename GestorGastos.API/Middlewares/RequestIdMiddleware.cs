namespace GestorGastos.API.Middlewares;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _siguiente;
    
    public RequestIdMiddleware(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        string? requestId = contexto.Request.Headers["X-Request-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            requestId = Guid.NewGuid().ToString();
        }

        contexto.Items["X-Request-Id"] = requestId;

        contexto.Response.OnStarting(() =>
        {
            contexto.Response.Headers["X-Request-Id"] = requestId;
            return Task.CompletedTask;

        });
        
        await _siguiente(contexto);
    }
}