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

    public async Task<RespuestaMetodoPagoDto> CrearMetodoPagoAsync(string? idUsuario, CrearMetodoPagoDto metodoPagoRecibido)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        bool estaCreado = await _repositorio.ExisteMetodoPagoPorNombreAsync(idConvertido, metodoPagoRecibido.Nombre);

        if (estaCreado)
        {
            throw new ConflictoExcepcion($"Ya tienes un método de pago activo llamado '{metodoPagoRecibido.Nombre}'."); 
        }
        
        MetodoPago metodoPagoNuevo = new MetodoPago(metodoPagoRecibido.Nombre, idConvertido);
        await _repositorio.AgregarMetodoPagoAsync(metodoPagoNuevo);

        return new RespuestaMetodoPagoDto
        {
            Id = metodoPagoNuevo.Id,
            Nombre = metodoPagoNuevo.Nombre
        };
    }

    public async Task<IEnumerable<RespuestaMetodoPagoDto>> ObtenerMetodosPagoAsync(string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        IEnumerable<MetodoPago> metodos = await _repositorio.ObtenerPorUsuarioAsync(idConvertido);
        
        return metodos.Select(m => new RespuestaMetodoPagoDto 
        { 
            Id = m.Id, 
            Nombre = m.Nombre 
        });
    
    }

    public async Task<RespuestaMetodoPagoDto> ObtenerMetodoPagoPorIdAsync(long idMetodoPago, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idConvertido);
        
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

    public async Task ActualizarMetodoPagoAsync(long idMetodoPago, string? idUsuario, ActualizarMetodoPagoDto metodoPagoRecibido)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idConvertido);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas actualizar no existe.");
        }
        
        bool nombreEnUso = await _repositorio.ExisteMetodoPagoPorNombreAsync(idConvertido, metodoPagoRecibido.Nombre);
        if (nombreEnUso && metodo.Nombre != metodoPagoRecibido.Nombre)
        {
            throw new Exception($"Ya tienes un método de pago activo llamado '{metodoPagoRecibido.Nombre}'.");
        }

        metodo.ActualizarNombre(metodoPagoRecibido.Nombre);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }

    public async Task EliminarMetodoPagoAsync(long idMetodoPago, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idConvertido);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas eliminar no existe.");
        }
        
        metodo.CambiarEstado(false);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }

    public async Task RestaurarMetodoPagoAsync(long idMetodoPago, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        MetodoPago? metodo = await _repositorio.BuscarPorIdAsync(idMetodoPago, idConvertido);
        
        if (metodo == null)
        { 
            throw new NoEncontradoExcepcion("El método de pago que intentas reactivar no existe.");
        }
        
        metodo.CambiarEstado(true);
        await _repositorio.ActualizarMetodoPagoAsync(metodo);
    }
    
    private long ValidarUsuario(string? idUsuario)
    {
        if (!long.TryParse(idUsuario, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("Credenciales inválidas.");
        }
        return idConvertido;
    }
}