using GestorGastos.Domain.Models;

namespace GestorGastos.Domain.Interfaces;

public interface IMetodoPagoRepository
{
    Task AgregarMetodoPagoAsync(MetodoPago metodoPago);
    Task<MetodoPago?> BuscarPorIdAsync(long idMetodoPago);
    Task<IEnumerable<MetodoPago>> ObtenerPorUsuarioAsync(long idUsuario);
    Task ActualizarMetodoPagoAsync(MetodoPago metodoPago);
    
    Task<bool> ExisteMetodoPagoPorNombreAsync(long idUsuario, string nombreMetodoPago);
}