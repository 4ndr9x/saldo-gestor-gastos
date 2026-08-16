using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_MetodoPago;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class MetodoPagoService : IMetodoPagoService
{

    private readonly IMetodoPagoRepository _repositorio;

    public MetodoPagoService(IMetodoPagoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<RespuestaMetodoPagoDto> CrearMetodoPagoAsync(long idUsuario, CrearMetodoPagoDto metodoPagoRecibido)
    {
        bool estaCreado = await _repositorio.ExisteMetodoPagoPorNombreAsync(idUsuario, metodoPagoRecibido.Nombre);

        if (estaCreado)
        {
            throw new ConflictoExcepcion($"Ya tienes un método de pago activo llamado '{metodoPagoRecibido.Nombre}'."); 
        }
        
        MetodoPago metodoPagoNuevo = new MetodoPago(metodoPagoRecibido.Nombre, idUsuario);
        await _repositorio.AgregarMetodoPagoAsync(metodoPagoNuevo);

        return new RespuestaMetodoPagoDto
        {
            Id = metodoPagoNuevo.Id,
            Nombre = metodoPagoNuevo.Nombre
        };
    }

    public async Task<IEnumerable<RespuestaMetodoPagoDto>> ObtenerMetodosPagoAsync(long idUsuario)
    {
        IEnumerable<MetodoPago> metodos = await _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        
        return metodos.Select(m => new RespuestaMetodoPagoDto 
        { 
            Id = m.Id, 
            Nombre = m.Nombre 
        });
    
    }

    public async Task<RespuestaMetodoPagoDto> ObtenerMetodoPagoPorIdAsync(long idMetodoPago, long idUsuario)
    {
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idUsuario);
        
        if (metodo == null)
        {
            throw new NoEncontradoExcepcion("El método de pago que estás intentando buscar no existe.");
        }

        return new RespuestaMetodoPagoDto
        {
            Id = metodo.Id,
            Nombre = metodo.Nombre
        };
    }

    public async Task ActualizarMetodoPagoAsync(long idMetodoPago, long idUsuario,
        ActualizarMetodoPagoDto metodoPagoRecibido)
    {
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idUsuario);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas actualizar no existe.");
        }
        
        bool nombreEnUso = await _repositorio.ExisteMetodoPagoPorNombreAsync(idUsuario, metodoPagoRecibido.Nombre);
        if (nombreEnUso && metodo.Nombre != metodoPagoRecibido.Nombre)
        {
            throw new Exception($"Ya tienes un método de pago activo llamado '{metodoPagoRecibido.Nombre}'.");
        }

        metodo.ActualizarNombre(metodoPagoRecibido.Nombre);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }

    public async Task EliminarMetodoPagoAsync(long idMetodoPago, long idUsuario)
    {
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idUsuario);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas eliminar no existe.");
        }
        
        metodo.CambiarEstado(false);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }

    public async Task RestaurarMetodoPagoAsync(long idMetodoPago, long idUsuario)
    {
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idUsuario);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas reactivar no existe.");
        }
        
        metodo.CambiarEstado(true);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }
}