using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_Gasto;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class GastoService : IGastoService
{
    private readonly IGastoRepository _gastoRepositorio;
    private readonly ICategoriaRepository _categoriaRepositorio;
    private readonly IMetodoPagoRepository _metodoPagoRepositorio;
    
    public GastoService(IGastoRepository gastoRepositorio, ICategoriaRepository categoriaRepositorio, IMetodoPagoRepository metodoPagoRepositorio)
    {
        _gastoRepositorio = gastoRepositorio;
        _categoriaRepositorio = categoriaRepositorio;
        _metodoPagoRepositorio = metodoPagoRepositorio;
    }
    
    public async Task<RespuestaGastoDto> CrearGastoAsync(string? idUsuario, CrearGastoDto gastoRecibido)
    {
        long idConvertido = ValidarUsuario(idUsuario);

        Categoria validacionCategoria = await ValidarCategoria(gastoRecibido.CategoriaId, idConvertido);
        MetodoPago validacionMetodoPago = await ValidarMetodoPago(gastoRecibido.MetodoPagoId, idConvertido);

        Gasto gastoNuevo = new Gasto(gastoRecibido.Descripcion, gastoRecibido.Monto, gastoRecibido.Fecha,
            gastoRecibido.CategoriaId, gastoRecibido.MetodoPagoId, idConvertido);
        
        await _gastoRepositorio.AgregarGastoAsync(gastoNuevo);
        
        return new RespuestaGastoDto
        {
            Id = gastoNuevo.Id,
            Descripcion = gastoNuevo.Descripcion,
            Monto = gastoNuevo.Monto,
            Fecha = gastoNuevo.Fecha,
            CategoriaId = gastoNuevo.CategoriaId,
            MetodoPagoId = gastoNuevo.MetodoPagoId,
            
            Categoria = validacionCategoria.Nombre,
            MetodoPago = validacionMetodoPago.Nombre
        };
    }

    public async Task<IEnumerable<RespuestaGastoDto>> ObtenerTodosGastosAsync(string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        
        IEnumerable<Gasto> gastosDb = await _gastoRepositorio.ObtenerPorUsuarioAsync(idConvertido);

        return gastosDb.Select(g => new RespuestaGastoDto
        {
            Id = g.Id,
            Descripcion = g.Descripcion,
            Monto = g.Monto,
            Fecha = g.Fecha,
            CategoriaId = g.CategoriaId,
            MetodoPagoId = g.MetodoPagoId,
            
            Categoria = g.Categoria.Nombre, 
            MetodoPago = g.MetodoPago.Nombre
        });
    }

    public async Task<RespuestaGastoDto> ObtenerGastoPorIdAsync(long idGasto, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);

        Gasto gastoDb = await ValidarGasto(idGasto, idConvertido);
        
        return new RespuestaGastoDto
        {
            Id = gastoDb.Id,
            Descripcion = gastoDb.Descripcion,
            Monto = gastoDb.Monto,
            Fecha = gastoDb.Fecha,
            CategoriaId = gastoDb.CategoriaId,
            MetodoPagoId = gastoDb.MetodoPagoId,
            
            Categoria = gastoDb.Categoria.Nombre,
            MetodoPago = gastoDb.MetodoPago.Nombre
        };
    }

    public async Task ActualizarGastoAsync(long idGasto, string? idUsuario, ActualizarGastoDto gastoRecibido)
    {
        long idConvertido = ValidarUsuario(idUsuario);

        Gasto gastoDb = await ValidarGasto(idGasto, idConvertido);
        
        if (gastoDb.CategoriaId != gastoRecibido.CategoriaId)
        {
            await ValidarCategoria(gastoRecibido.CategoriaId, idConvertido);    
        }
        
        if (gastoDb.MetodoPagoId != gastoRecibido.MetodoPagoId)
        {
            await ValidarMetodoPago(gastoRecibido.MetodoPagoId, idConvertido);
        }
        
        gastoDb.ActualizarDetalles(
            gastoRecibido.Descripcion, 
            gastoRecibido.Monto, 
            gastoRecibido.Fecha, 
            gastoRecibido.CategoriaId, 
            gastoRecibido.MetodoPagoId
        );
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }
    
    public async Task EliminarGastoAsync(long idGasto, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto);

        if (gastoDb == null || gastoDb.UsuarioId != idConvertido)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }

        if (!gastoDb.Activo)
        {
            throw new ConflictoExcepcion("El gasto ya estaba inactivo.");
        }
        
        gastoDb.CambiarEstado(false);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }

    public async Task RestaurarGastoAsync(long idGasto, string? idUsuario)
    {
        long idConvertido = ValidarUsuario(idUsuario);
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto);

        if (gastoDb == null || gastoDb.UsuarioId != idConvertido)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }

        if (gastoDb.Activo)
        {
            throw new ConflictoExcepcion("El gasto ya estaba activo.");
        }

        gastoDb.CambiarEstado(true);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }
    
    // METODOS PRIVADOS PARA LA VALIDACION DE LA INFORMACION.
    private long ValidarUsuario(string? idUsuario)
    {
        if (!long.TryParse(idUsuario, out long idConvertido))
        {
            throw new SinAutorizacionExcepcion("Credenciales inválidas.");
        }
        return idConvertido;
    }
    
    private async Task<Categoria> ValidarCategoria(long idCategoria, long idConvertido)
    {
        Categoria? validacionCategoria = await _categoriaRepositorio.BuscarPorIdAsync(idCategoria);

        if (validacionCategoria == null || validacionCategoria.UsuarioId != idConvertido)
        {
            throw new NoEncontradoExcepcion("La categoria que has intentado buscar no existe");
        }
        if (!validacionCategoria.Activo)
        {
            List<string> detalles = new List<string> {"La categoria seleccionada esta inactiva."};
            throw new DatosErroneosExcepcion("Error de seleccion de categoria", detalles);
        }

        return validacionCategoria;
    }

    private async Task<MetodoPago> ValidarMetodoPago(long idMetodoPago, long idConvertido)
    {
        MetodoPago? validacionMetodoPago = await _metodoPagoRepositorio.BuscarPorIdAsync(idMetodoPago);

        if (validacionMetodoPago == null || validacionMetodoPago.UsuarioId != idConvertido)
        {
            throw new NoEncontradoExcepcion("El metodo de pago has intentado buscar no existe");
        }

        if (!validacionMetodoPago.Activo)
        {
            List<string> detalles = new List<string> {"El metodo de pago seleccionado esta inactivo."};
            throw new DatosErroneosExcepcion("Error de seleccion de metodo de pago.", detalles);
        }

        return validacionMetodoPago;
    }

    private async Task<Gasto> ValidarGasto(long idGasto, long idConvertido)
    {
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto);
        
        if (gastoDb == null || gastoDb.UsuarioId != idConvertido)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }
        if (!gastoDb.Activo)
        {
            List<string> detalles = new List<string> {"El gasto seleccionado esta inactivo."};
            throw new DatosErroneosExcepcion("Error de seleccion de gasto", detalles);
        }

        return gastoDb;
    }
}