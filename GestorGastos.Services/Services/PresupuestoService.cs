using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_Presupuesto;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class PresupuestoService : IPresupuestoService
{
    private readonly IPresupuestoRepository _presupuestoRepositorio;
    private readonly ICategoriaRepository _categoriaRepositorio;
    private readonly IGastoRepository _gastoRepositorio;

    public PresupuestoService(IPresupuestoRepository presupuestoRepository, ICategoriaRepository categoriaRepository, IGastoRepository gastoRepository)
    {
        _presupuestoRepositorio = presupuestoRepository;
        _categoriaRepositorio = categoriaRepository;
        _gastoRepositorio = gastoRepository;
    }
    
    public async Task<RespuestaPresupuestoDto> CrearPresupuestoAsync(long idUsuario, CrearPresupuestoDto presupuestoRecibido)
    {
        var categoria = await _categoriaRepositorio.BuscarPorIdAsync(presupuestoRecibido.CategoriaId, idUsuario);
        if (categoria == null)
        {
            throw new NoEncontradoExcepcion("La categoría que has intentado buscar no existe.");
        }
        
        var presupuestoExistente = await _presupuestoRepositorio
            .ObtenerPresupuestoPorMesYCategoriaAsync(idUsuario, presupuestoRecibido.CategoriaId, presupuestoRecibido.Month, presupuestoRecibido.Year);
            
        if (presupuestoExistente != null)
        {
            List<string> detalles = new List<string> {"Ya existe un presupuesto asignado para esta categoría en este mes."};
            throw new DatosErroneosExcepcion("Ocurrio un error con la informacion suministrada", detalles);
        }

        Presupuesto nuevoPresupuesto = new Presupuesto(idUsuario, presupuestoRecibido.CategoriaId,
            presupuestoRecibido.Month, presupuestoRecibido.Year, presupuestoRecibido.MontoMaximo);
        
        await _presupuestoRepositorio.AgregarPresupuestoAsync(nuevoPresupuesto);
        
        return new RespuestaPresupuestoDto
        {
            Id = nuevoPresupuesto.Id,
            CategoriaId = nuevoPresupuesto.CategoriaId,
            NombreCategoria = categoria.Nombre,
            Month = nuevoPresupuesto.Month,
            Year = nuevoPresupuesto.Year,
            MontoPresupuestado = nuevoPresupuesto.MontoMaximo,
            MontoGastado = 0,
            PorcentajeConsumido = 0
        };
    }

    public async Task<IEnumerable<RespuestaPresupuestoDto>> ObtenerResumenPresupuestosDelMesAsync(long idUsuario, int month, int year, bool soloExcedidos)
    {
        var presupuestosDelMes = await _presupuestoRepositorio.ObtenerPresupuestosDelMesAsync(idUsuario, month, year);
        
        var listaRespuesta = new List<RespuestaPresupuestoDto>();

        foreach (var presupuesto in presupuestosDelMes)
        {
            decimal totalGastado = await _gastoRepositorio
                .ObtenerTotalGastadoPorCategoriaYMesAsync(idUsuario, presupuesto.CategoriaId, month, year);
            
            decimal porcentaje = 0;
            if (presupuesto.MontoMaximo > 0)
            {
                porcentaje = (totalGastado / presupuesto.MontoMaximo) * 100;
            }
            
            string nivelAlerta = "Normal";
            if (porcentaje >= 100)
            {
                nivelAlerta = "Excedido (100%+)";
            }
            else if (porcentaje >= 80)
            {
                nivelAlerta = "Peligro (80%+)";
            }
            else if (porcentaje >= 50)
            {
                nivelAlerta = "Precaución (50%+)";
            }
            
            listaRespuesta.Add(new RespuestaPresupuestoDto
            {
                Id = presupuesto.Id,
                CategoriaId = presupuesto.CategoriaId,
                NombreCategoria = presupuesto.Categoria?.Nombre ?? "Sin Categoría",
                Month = presupuesto.Month,
                Year = presupuesto.Year,
                MontoPresupuestado = presupuesto.MontoMaximo,
                MontoGastado = totalGastado,
                PorcentajeConsumido = Math.Round(porcentaje, 2),
                NivelAlerta = nivelAlerta
            });
        }
        
        if (soloExcedidos)
        {
            listaRespuesta = listaRespuesta.Where(p => p.PorcentajeConsumido > 100).ToList();
        }
        
        return listaRespuesta;
    }

    public async Task<RespuestaPresupuestoDto> ObtenerPresupuestoPorIdAsync(long idPresupuesto, long idUsuario)
    {
        Presupuesto presupuesto = await ValidarPresupuesto(idPresupuesto, idUsuario);
        
        decimal totalGastado = await _gastoRepositorio
            .ObtenerTotalGastadoPorCategoriaYMesAsync(idUsuario, presupuesto.CategoriaId, presupuesto.Month, presupuesto.Year);

        decimal porcentaje = 0;
        if (presupuesto.MontoMaximo > 0)
        {
            porcentaje = (totalGastado / presupuesto.MontoMaximo) * 100;
        }
        
        string nivelAlerta = "Normal";
        if (porcentaje >= 100)
        {
            nivelAlerta = "Excedido (100%+)";
        }
        else if (porcentaje >= 80)
        {
            nivelAlerta = "Peligro (80%+)";
        }
        else if (porcentaje >= 50)
        {
            nivelAlerta = "Precaución (50%+)";
        }

        return new RespuestaPresupuestoDto
        {
            Id = presupuesto.Id,
            CategoriaId = presupuesto.CategoriaId,
            NombreCategoria = presupuesto.Categoria?.Nombre ?? "Sin Categoría",
            Month = presupuesto.Month,
            Year = presupuesto.Year,
            MontoPresupuestado = presupuesto.MontoMaximo,
            MontoGastado = totalGastado,
            PorcentajeConsumido = Math.Round(porcentaje, 2),
            NivelAlerta = nivelAlerta
        };
    }

    public async Task ActualizarPresupuestoAsync(long idPresupuesto, long idUsuario, ActualizarPresupuestoDto presupuestoRecibido)
    {
        Presupuesto presupuesto = await ValidarPresupuesto(idPresupuesto, idUsuario);

        presupuesto.CambiarMontoMaximo(presupuestoRecibido.MontoMaximo);

        await _presupuestoRepositorio.ActualizarPresupuestoAsync(presupuesto);
    }

    public async Task EliminarPresupuestoAsync(long idPresupuesto, long idUsuario)
    {
        Presupuesto presupuesto = await ValidarPresupuesto(idPresupuesto, idUsuario);
        
        presupuesto.CambiarEstado(false);

        await _presupuestoRepositorio.ActualizarPresupuestoAsync(presupuesto);
    }
    
    private async Task<Presupuesto> ValidarPresupuesto(long idPresupuesto, long idUsuario)
    {
        Presupuesto? presupuestoDb = await _presupuestoRepositorio.BuscarPorIdAsync(idPresupuesto, idUsuario);

        if (presupuestoDb == null)
        {
            throw new NoEncontradoExcepcion("El presupuesto que se ha intentado buscar no existe.");
        }

        return presupuestoDb;
    }
    
}