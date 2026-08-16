using System.Text.Json;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Services.DTOs.DTOs_de_Tasa_de_Cambio;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class TasaCambioService : ITasaCambioService
{
    private readonly HttpClient _httpClient;

    public TasaCambioService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> ObtenerTasaCambioAsync(string monedaOrigen, string monedaDestino)
    {
        monedaOrigen = monedaOrigen.ToUpper().Trim();
        monedaDestino = monedaDestino.ToUpper().Trim();

        if (monedaOrigen == monedaDestino) return 1.0m;
        
            var respuesta = await _httpClient.GetAsync("https://api.fxratesapi.com/latest/");
            
            if (!respuesta.IsSuccessStatusCode)
                throw new ErrorConexionApi($"Error en la API externa.");

            var contenidoJson = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var datos = JsonSerializer.Deserialize<RespuestaTasaCambioDto>(contenidoJson, opciones);

            if (datos == null || datos.Rates == null)
            {
                throw new ErrorConexionApi($"Error en la API externa.");
            }
            
            decimal tasaOrigen = 1.0m;
            if (monedaOrigen != datos.Base.ToUpper())
            {
                if (!datos.Rates.TryGetValue(monedaOrigen, out tasaOrigen))
                {
                    Console.WriteLine($"La API no devolvió una tasa para '{monedaOrigen}'.");
                    throw new ErrorConexionApi($"Error en la API externa.");
                }
            }
            
            decimal tasaDestino = 1.0m;
            if (monedaDestino != datos.Base.ToUpper())
            {
                if (!datos.Rates.TryGetValue(monedaDestino, out tasaDestino))
                {
                    Console.WriteLine($"La API no devolvió una tasa para '{monedaDestino}'.");
                    throw new ErrorConexionApi($"Error en la API externa.");
                }
            }
            
            decimal tasaFinal = tasaDestino / tasaOrigen;

            return Math.Round(tasaFinal, 4);
    }
}