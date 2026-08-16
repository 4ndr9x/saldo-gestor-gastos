namespace GestorGastos.Services.Interfaces;

public interface ITasaCambioService
{
    Task<decimal> ObtenerTasaCambioAsync(string monedaOrigen, string monedaDestino);
}