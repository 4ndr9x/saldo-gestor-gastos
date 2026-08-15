using ClosedXML.Excel;
using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_Exportacion;
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
    
    public async Task<RespuestaGastoDto> CrearGastoAsync(long idUsuario, CrearGastoDto gastoRecibido)
    {

        Categoria validacionCategoria = await ValidarCategoria(gastoRecibido.CategoriaId, idUsuario);
        MetodoPago validacionMetodoPago = await ValidarMetodoPago(gastoRecibido.MetodoPagoId, idUsuario);

        Gasto gastoNuevo = new Gasto(gastoRecibido.Descripcion, gastoRecibido.Monto, gastoRecibido.Fecha,
            gastoRecibido.CategoriaId, gastoRecibido.MetodoPagoId, idUsuario);
        
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

    public async Task<IEnumerable<RespuestaGastoDto>> ObtenerTodosGastosAsync(long idUsuario)
    {
        
        IEnumerable<Gasto> gastosDb = await _gastoRepositorio.ObtenerPorUsuarioAsync(idUsuario);

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

    public async Task<RespuestaGastoDto> ObtenerGastoPorIdAsync(long idGasto, long idUsuario)
    {
        Gasto gastoDb = await ValidarGasto(idGasto, idUsuario);
        
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

    public async Task ActualizarGastoAsync(long idGasto, long idUsuario, ActualizarGastoDto gastoRecibido)
    {
        
        Gasto gastoDb = await ValidarGasto(idGasto, idUsuario);
        
        if (gastoDb.CategoriaId != gastoRecibido.CategoriaId)
        {
            await ValidarCategoria(gastoRecibido.CategoriaId, idUsuario);    
        }
        
        if (gastoDb.MetodoPagoId != gastoRecibido.MetodoPagoId)
        {
            await ValidarMetodoPago(gastoRecibido.MetodoPagoId, idUsuario);
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
    
    public async Task EliminarGastoAsync(long idGasto, long idUsuario)
    {
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto, idUsuario);

        if (gastoDb == null)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }
        
        gastoDb.CambiarEstado(false);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }

    public async Task RestaurarGastoAsync(long idGasto, long idUsuario)
    {
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto, idUsuario);

        if (gastoDb == null)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }
        
        gastoDb.CambiarEstado(true);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }
    
    public async Task<ResultadoImportacionDto> ImportarGastosDesdeExcelAsync(long idUsuario, Stream archivoExcel)
    {
        
        var resultado = new ResultadoImportacionDto();
            
        var gastosValidosParaGuardar = new List<Gasto>();
        
        var categoriasUsuario = await _categoriaRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        var metodosPagoUsuario = await _metodoPagoRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        
        
        var dictCategorias = categoriasUsuario.ToDictionary(c => c.Id);
        var dictMetodos = metodosPagoUsuario.ToDictionary(m => m.Id);
        
        using var workbook = new XLWorkbook(archivoExcel);
        var worksheet = workbook.Worksheet(1);
        
        var filas = worksheet.RowsUsed().Skip(1); 
        
        int numeroFilaActual = 2;
        
        foreach (var fila in filas)
        {
            resultado.TotalFilasProcesadas++;
            
            try 
            {
                var celdaFecha = fila.Cell(1);
                var celdaMonto = fila.Cell(2);
                var celdaDescripcion = fila.Cell(3);
                var celdaCategoriaId = fila.Cell(4);
                var celdaMetodoId = fila.Cell(5);
                
                if (!celdaFecha.TryGetValue(out DateTime fecha)) 
                    throw new Exception("El formato de la fecha es inválido.");
                
                if (!celdaMonto.TryGetValue(out double montoDoble) || montoDoble <= 0) 
                    throw new Exception("El monto debe ser un número válido y mayor a 0.");
                decimal monto = (decimal)montoDoble;
                
                string descripcion = celdaDescripcion.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(descripcion))
                    throw new Exception("La descripción no puede estar vacía.");
                if (descripcion.Length > 100)
                    throw new Exception("La descripción supera el límite de 100 caracteres.");
                
                if (!celdaCategoriaId.TryGetValue(out double catIdDoble) || !dictCategorias.ContainsKey((long)catIdDoble))
                    throw new Exception("La categoría especificada no existe o no te pertenece.");
                long catId = (long)catIdDoble;

                if (!celdaMetodoId.TryGetValue(out double metIdDoble) || !dictMetodos.ContainsKey((long)metIdDoble))
                    throw new Exception("El método de pago especificado no existe o no te pertenece.");
                long metId = (long)metIdDoble;
                
                gastosValidosParaGuardar.Add(new Gasto(
                    descripcion,
                    monto,
                    fecha,
                    catId,
                    metId,
                    idUsuario
                ));
            }
            catch (Exception ex)
            {
                resultado.FilasConErrores++;
                resultado.DetallesErrores.Add(new DetalleErrorFilaDto 
                {
                    NumeroFila = numeroFilaActual,
                    MensajeError = ex.Message
                });
            }
            numeroFilaActual++;
        }
        
        if (gastosValidosParaGuardar.Any())
        {
            await _gastoRepositorio.AgregarRangoDeGastosAsync(gastosValidosParaGuardar);
            resultado.GastosImportadosExitosamente = gastosValidosParaGuardar.Count;
        }

        return resultado;
    }
    

    // METODOS PRIVADOS PARA LA VALIDACION DE LA INFORMACIÓN.
    
    private async Task<Categoria> ValidarCategoria(long idCategoria, long idConvertido)
    {
        Categoria? validacionCategoria = await _categoriaRepositorio.BuscarPorIdAsync(idCategoria, idConvertido);

        if (validacionCategoria == null)
        {
            throw new NoEncontradoExcepcion("La categoria que has intentado buscar no existe");
        }

        return validacionCategoria;
    }

    private async Task<MetodoPago> ValidarMetodoPago(long idMetodoPago, long idConvertido)
    {
        MetodoPago? validacionMetodoPago = await _metodoPagoRepositorio.BuscarPorIdAsync(idMetodoPago, idConvertido);

        if (validacionMetodoPago == null)
        {
            throw new NoEncontradoExcepcion("El metodo de pago has intentado buscar no existe");
        }

        return validacionMetodoPago;
    }

    private async Task<Gasto> ValidarGasto(long idGasto, long idConvertido)
    {
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto, idConvertido);
        
        if (gastoDb == null)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }

        return gastoDb;
    }
}