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
    
    private readonly IUsuarioRepository _usuarioRepositorio;
    private readonly ITasaCambioService _tasaCambioService;
    public GastoService(IGastoRepository gastoRepositorio, ICategoriaRepository categoriaRepositorio, IMetodoPagoRepository metodoPagoRepositorio, 
        IUsuarioRepository usuarioRepositorio, ITasaCambioService tasaCambioService)
    {
        _gastoRepositorio = gastoRepositorio;
        _categoriaRepositorio = categoriaRepositorio;
        _metodoPagoRepositorio = metodoPagoRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
        _tasaCambioService = tasaCambioService;
    }
    
    public async Task<RespuestaGastoDto> CrearGastoAsync(long idUsuario, CrearGastoDto gastoRecibido)
    {

        if (gastoRecibido.MontoOriginal <= 0)
        {
            List<string> detalles = new List<string> { "El monto del gasto debe ser positivo." };
            throw new DatosErroneosExcepcion("Ha ocurrido un error con los datos enviados.", detalles);
        }
        
        Categoria validacionCategoria = await ValidarCategoria(gastoRecibido.CategoriaId, idUsuario);
        MetodoPago validacionMetodoPago = await ValidarMetodoPago(gastoRecibido.MetodoPagoId, idUsuario);
        Usuario usuarioDb = await ValidarUsuario(idUsuario);
        
        decimal tasaCambioCalculada = await _tasaCambioService.ObtenerTasaCambioAsync(gastoRecibido.Moneda, usuarioDb.MonedaUsada);

        Gasto gastoNuevo = new Gasto(gastoRecibido.Concepto ,gastoRecibido.Descripcion, gastoRecibido.MontoOriginal,
            usuarioDb.MonedaUsada, tasaCambioCalculada, gastoRecibido.Fecha, gastoRecibido.CategoriaId, gastoRecibido.MetodoPagoId, idUsuario);
        
        await _gastoRepositorio.AgregarGastoAsync(gastoNuevo);
        
        return new RespuestaGastoDto
        {
            Id = gastoNuevo.Id,
            Concepto = gastoNuevo.Concepto,
            Descripcion = gastoNuevo.Descripcion,
            MontoFinal = gastoNuevo.MontoFinal,
            Fecha = gastoNuevo.Fecha,
            CategoriaId = gastoNuevo.CategoriaId,
            MetodoPagoId = gastoNuevo.MetodoPagoId,
            Categoria = validacionCategoria.Nombre,
            MetodoPago = validacionMetodoPago.Nombre,
            
            MontoOriginal = gastoNuevo.MontoOriginal,
            Moneda = gastoRecibido.Moneda,
            TasaCambio = gastoNuevo.TasaCambio 
        };
    }

    public async Task<IEnumerable<RespuestaGastoDto>> ObtenerTodosGastosAsync(long idUsuario)
    {
        
        IEnumerable<Gasto> gastosDb = await _gastoRepositorio.ObtenerPorUsuarioAsync(idUsuario);

        return gastosDb.Select(g => new RespuestaGastoDto
        {
            Id = g.Id,
            Concepto = g.Concepto,
            Descripcion = g.Descripcion,
            MontoFinal = g.MontoFinal,
            Fecha = g.Fecha,
            CategoriaId = g.CategoriaId,
            MetodoPagoId = g.MetodoPagoId,
            Categoria = g.Categoria.Nombre, 
            MetodoPago = g.MetodoPago.Nombre,
            MontoOriginal = g.MontoOriginal,
            TasaCambio = Math.Round(g.TasaCambio, 2),
            Moneda = g.Moneda
        });
    }

    public async Task<RespuestaGastoDto> ObtenerGastoPorIdAsync(long idGasto, long idUsuario)
    {
        Gasto gastoDb = await ValidarGasto(idGasto, idUsuario);
        
        return new RespuestaGastoDto
        {
            Id = gastoDb.Id,
            Concepto = gastoDb.Concepto,
            Descripcion = gastoDb.Descripcion,
            MontoFinal = gastoDb.MontoFinal,
            Fecha = gastoDb.Fecha,
            CategoriaId = gastoDb.CategoriaId,
            MetodoPagoId = gastoDb.MetodoPagoId,
            Categoria = gastoDb.Categoria.Nombre,
            MetodoPago = gastoDb.MetodoPago.Nombre,
            MontoOriginal = gastoDb.MontoOriginal,
            Moneda = gastoDb.Moneda,
            TasaCambio = Math.Round(gastoDb.TasaCambio, 2)
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
        
        decimal tasaCambioParaGuardar = gastoDb.TasaCambio;
        string nuevaMoneda = gastoRecibido.Moneda.ToUpper().Trim();
        
        if (gastoDb.Moneda != nuevaMoneda)
        {
            var usuario = await ValidarUsuario(idUsuario);
        
            string monedaUsadaUsuario = usuario.MonedaUsada;
            
            tasaCambioParaGuardar = await _tasaCambioService.ObtenerTasaCambioAsync(nuevaMoneda, monedaUsadaUsuario);
        }
        
        gastoDb.ActualizarDetalles(
            gastoRecibido.Concepto,
            gastoRecibido.Descripcion, 
            gastoRecibido.MontoOriginal,
            nuevaMoneda,
            tasaCambioParaGuardar,
            gastoRecibido.Fecha, 
            gastoRecibido.CategoriaId, 
            gastoRecibido.MetodoPagoId
        );
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }
    
    public async Task EliminarGastoAsync(long idGasto, long idUsuario)
    {
        Gasto gastoDb = await ValidarGasto(idGasto, idUsuario);
        
        gastoDb.CambiarEstado(false);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }

    public async Task RestaurarGastoAsync(long idGasto, long idUsuario)
    {
        Gasto gastoDb = await ValidarGasto(idGasto, idUsuario);
        
        gastoDb.CambiarEstado(true);
        
        await _gastoRepositorio.ActualizarGastoAsync(gastoDb);
    }
    
    // METODOS PARA MANEJAR LA IMPORTACION DEL EXCEL Y LA DESCARGA DE LA PLANTILLA
    public async Task<ResultadoImportacionDto> ImportarGastosDesdeExcelAsync(long idUsuario, Stream archivoExcel)
    {
        var resultado = new ResultadoImportacionDto();
        var gastosValidosParaGuardar = new List<Gasto>();
        
        Usuario usuario = await ValidarUsuario(idUsuario);
        string monedaBaseUsuario = usuario.MonedaUsada ?? "USD";

        var categoriasUsuario = await _categoriaRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        var metodosPagoUsuario = await _metodoPagoRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        var gastosExistentes = await _gastoRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        
        var dictCategorias = categoriasUsuario.ToDictionary(c => c.Nombre.ToLower(), c => c);
        var dictMetodos = metodosPagoUsuario.ToDictionary(m => m.Nombre.ToLower(), m => m);
        
        var firmasExistentes = new HashSet<string>(
            gastosExistentes.Select(g =>
                $"{g.Fecha:yyyyMMddHHmmss}_{g.MontoOriginal}_{g.Moneda}_{g.Descripcion.Trim().ToLower()}_{g.CategoriaId}_{g.MetodoPagoId}")
        );
        
        var firmasEnEsteExcel = new HashSet<string>();
        
        var cacheTasas = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        
        using var workbook = new XLWorkbook(archivoExcel);
        var worksheet = workbook.Worksheet(1);
        var filas = worksheet.RowsUsed().Skip(1); 
        int numeroFilaActual = 2;

        foreach (var fila in filas)
        {
            resultado.TotalFilasProcesadas++;

            var celdaFecha = fila.Cell(1);
            var celdaMonto = fila.Cell(2);
            var celdaDescripcion = fila.Cell(3);
            var celdaCategoriaId = fila.Cell(4);
            var celdaMetodoId = fila.Cell(5);
            var celdaConcepto = fila.Cell(6);
            var celdaMoneda = fila.Cell(7);
            
            string nombreCategoria = celdaCategoriaId.GetString()?.Trim() ?? string.Empty;
            string nombreMetodo = celdaMetodoId.GetString()?.Trim() ?? string.Empty;
            
            List<string> detallesFila = new List<string>();
            
            string concepto = celdaConcepto.GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(concepto))
            {
                detallesFila.Add("El concepto no puede estar vacío.");
            }

            else if (concepto.Length > 100)
            {
                detallesFila.Add("El concepto supera el límite de caracteres permitidos.");
            }
            
            DateTime fecha = DateTime.Now;
            string valorCeldaFecha = celdaFecha.GetString();
            bool fechaLeidaExitosamente = false;
            
            if (celdaFecha.TryGetValue(out DateTime fechaDesdeExcel))
            {
                fecha = fechaDesdeExcel;
                fechaLeidaExitosamente = true;
            }
            
            else if (!string.IsNullOrWhiteSpace(valorCeldaFecha) && DateTime.TryParse(valorCeldaFecha, out DateTime fechaDesdeTexto))
            {
                fecha = fechaDesdeTexto;
                fechaLeidaExitosamente = true;
            }
            else if (double.TryParse(valorCeldaFecha, out double excelDate))
            {
                if (excelDate >= -657435.0 && excelDate <= 2958465.99)
                {
                    fecha = DateTime.FromOADate(excelDate);
                    fechaLeidaExitosamente = true;
                }
            }
            
            if (fechaLeidaExitosamente && fecha.TimeOfDay == TimeSpan.Zero)
            {
                fecha = fecha.AddHours(12);
            }
            
            if (fecha.Date > DateTime.Now.Date)
            {
                detallesFila.Add("No puedes registrar un gasto con una fecha superior al día de hoy.");
            }

            if (!celdaMonto.TryGetValue(out double montoDoble) || montoDoble <= 0)
            {
                detallesFila.Add("El monto debe ser un número válido y mayor a 0.");
            }

            string descripcion = celdaDescripcion.GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                detallesFila.Add("La descripción no puede estar vacía.");
            }

            else if (descripcion.Length > 100)
            {
                detallesFila.Add("La descripción supera el límite de 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(nombreCategoria))
            {
                detallesFila.Add("La categoría no puede estar vacía.");
            }
            else if (nombreCategoria.Length > 100)
            {
                detallesFila.Add("El nombre de la categoría es muy largo.");
            }

            if (string.IsNullOrWhiteSpace(nombreMetodo))
            {
                detallesFila.Add("El método de pago no puede estar vacío.");
            }
            else if (nombreMetodo.Length > 50)
            {
                detallesFila.Add("El nombre del método de pago es muy largo.");
            }

            if (!detallesFila.Any())
            {
                decimal monto = (decimal)celdaMonto.GetDouble();
                long catId = 0;
                long metId = 0;
                
                string catKey = nombreCategoria.ToLower();
                if (dictCategorias.TryGetValue(catKey, out var categoriaExistente))
                {
                    catId = categoriaExistente.Id;
                }
                else
                {
                    var nuevaCategoria = new Categoria(nombreCategoria, idUsuario); 
                    await _categoriaRepositorio.AgregarCategoriaAsync(nuevaCategoria); 
                    catId = nuevaCategoria.Id;
                    dictCategorias[catKey] = nuevaCategoria; 
                }
                
                string metKey = nombreMetodo.ToLower();
                if (dictMetodos.TryGetValue(metKey, out var metodoExistente))
                {
                    metId = metodoExistente.Id;
                }
                else
                {
                    var nuevoMetodo = new MetodoPago(nombreMetodo, idUsuario);
                    await _metodoPagoRepositorio.AgregarMetodoPagoAsync(nuevoMetodo);
                    metId = nuevoMetodo.Id;
                    dictMetodos[metKey] = nuevoMetodo;
                }
            
                string monedaFila = celdaMoneda.GetString() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(monedaFila))
                {
                    monedaFila = monedaBaseUsuario;
                }

                monedaFila = monedaFila.Trim().ToUpper();
                decimal tasaFila = 1.0m;

                try
                {
                    if (!cacheTasas.TryGetValue(monedaFila, out tasaFila))
                    {
                        tasaFila = await _tasaCambioService.ObtenerTasaCambioAsync(monedaFila, monedaBaseUsuario);
                        cacheTasas[monedaFila] = tasaFila;
                    }
                }
                catch (Exception ex)
                {
                    detallesFila.Add($"Error al obtener la tasa de cambio para {monedaFila}.");
                }
                
                if (!detallesFila.Any())
                {
                    string firmaFilaActual = $"{fecha:yyyyMMddHHmmss}_{monto}_{monedaFila}_{descripcion.Trim().ToLower()}_{catId}_{metId}";

                    if (firmasExistentes.Contains(firmaFilaActual))
                    {
                        detallesFila.Add("Este gasto ya se encuentra registrado en el sistema (Duplicado).");
                    }
                    else if (firmasEnEsteExcel.Contains(firmaFilaActual))
                    {
                        detallesFila.Add("Este gasto está duplicado dentro de este mismo archivo Excel.");
                    }
                    else
                    {
                        firmasEnEsteExcel.Add(firmaFilaActual);
                        
                        gastosValidosParaGuardar.Add(new Gasto(
                            concepto ?? "",
                            descripcion,
                            monto,
                            monedaFila,
                            tasaFila,
                            fecha,
                            catId,
                            metId,
                            idUsuario
                        ));
                    }
                }
            }
            
            if (detallesFila.Any())
            {
                resultado.FilasConErrores++;
                resultado.DetallesErrores.Add(new DetalleErrorFilaDto 
                {
                    NumeroFila = numeroFilaActual,
                    MensajeError = string.Join(" | ", detallesFila) 
                });
            }

            numeroFilaActual++;
        }

        if (gastosValidosParaGuardar.Any())
        {
            await _gastoRepositorio.AgregarRangoDeGastosAsync(gastosValidosParaGuardar);
            resultado.GastosImportadosExitosamente = gastosValidosParaGuardar.Count;
        }
        
        if (resultado.FilasConErrores > 0)
        {
            List<string> listaErroresFormateados = resultado.DetallesErrores
                .Select(e => $"Fila {e.NumeroFila}: {e.MensajeError}")
                .ToList();

            throw new DatosErroneosExcepcion(
                $"El archivo contiene errores en {resultado.FilasConErrores} fila(s) y la importación fue abortada.", 
                listaErroresFormateados
            );
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

    private async Task<MetodoPago> ValidarMetodoPago(long idMetodoPago, long idUsuario)
    {
        MetodoPago? validacionMetodoPago = await _metodoPagoRepositorio.BuscarPorIdAsync(idMetodoPago, idUsuario);

        if (validacionMetodoPago == null)
        {
            throw new NoEncontradoExcepcion("El metodo de pago has intentado buscar no existe");
        }

        return validacionMetodoPago;
    }

    private async Task<Gasto> ValidarGasto(long idGasto, long idUsuario)
    {
        Gasto? gastoDb = await _gastoRepositorio.BuscarPorIdAsync(idGasto, idUsuario);
        
        if (gastoDb == null)
        {
            throw new NoEncontradoExcepcion("El gasto que has intentado buscar no existe.");
        }

        return gastoDb;
    }

    private async Task<Usuario> ValidarUsuario(long idUsuario)
    {
        Usuario? usuarioDb = await _usuarioRepositorio.BuscarUsuarioPorIdAsync(idUsuario);

        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        return usuarioDb;
    }
}