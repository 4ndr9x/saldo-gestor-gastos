using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs.DTOs_de_Presupuesto;
using GestorGastos.Services.Interfaces;

namespace GestorGastos.Services.Services;

public class PresupuestoService : IPresupuestoService
{
    private readonly IPresupuestoRepository _presupuestoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IGastoRepository _gastoRepository;

    public PresupuestoService(IPresupuestoRepository presupuestoRepository, ICategoriaRepository categoriaRepository, IGastoRepository gastoRepository)
    {
        _presupuestoRepository = presupuestoRepository;
        _categoriaRepository = categoriaRepository;
        _gastoRepository = gastoRepository;
    }
    
    public async Task<RespuestaPresupuestoDto> CrearPresupuestoAsync(long idUsuario, CrearPresupuestoDto presupuestoRecibido)
    {
        var categoria = await _categoriaRepository.BuscarPorIdAsync(presupuestoRecibido.CategoriaId, idUsuario);
        if (categoria == null)
        {
            throw new NoEncontradoExcepcion("La categoría que has intentado buscar no existe.");
        }
        
        var presupuestoExistente = await _presupuestoRepository
            .ObtenerPresupuestoPorMesYCategoriaAsync(idUsuario, presupuestoRecibido.CategoriaId, presupuestoRecibido.Month, presupuestoRecibido.Year);
            
        if (presupuestoExistente != null)
        {
            List<string> detalles = new List<string> {"Ya existe un presupuesto asignado para esta categoría en este mes."};
            throw new DatosErroneosExcepcion("Ocurrio un error con la informacion suministrada", detalles);
        }

        Presupuesto nuevoPresupuesto = new Presupuesto(idUsuario, presupuestoRecibido.CategoriaId,
            presupuestoRecibido.Month, presupuestoRecibido.Year, presupuestoRecibido.MontoMaximo);
        
        await _presupuestoRepository.AgregarPresupuestoAsync(nuevoPresupuesto);
        
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

    public async Task<IEnumerable<RespuestaPresupuestoDto>> ObtenerResumenPresupuestosDelMesAsync(long idUsuario, int month, int year)
    {
        var presupuestosDelMes = await _presupuestoRepository.ObtenerPresupuestosDelMesAsync(idUsuario, month, year);
        
        var listaRespuesta = new List<RespuestaPresupuestoDto>();

        foreach (var presupuesto in presupuestosDelMes)
        {
            decimal totalGastado = await _gastoRepository
                .ObtenerTotalGastadoPorCategoriaYMesAsync(idUsuario, presupuesto.CategoriaId, month, year);
            
            decimal porcentaje = 0;
            if (presupuesto.MontoMaximo > 0)
            {
                porcentaje = (totalGastado / presupuesto.MontoMaximo) * 100;
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
                PorcentajeConsumido = Math.Round(porcentaje, 2)
            });
        }

        return listaRespuesta;
    }

    public async Task ActualizarPresupuestoAsync(long idPresupuesto, long idUsuario, ActualizarPresupuestoDto presupuestoRecibido)
    {
        Presupuesto? presupuesto = await _presupuestoRepository.BuscarPorIdAsync(idPresupuesto, idUsuario);
    
        if (presupuesto == null)
        {
            throw new NoEncontradoExcepcion("El presupuesto que estás intentando buscar no existe.");
        }

        presupuesto.CambiarMontoMaximo(presupuestoRecibido.MontoMaximo);

        await _presupuestoRepository.ActualizarPresupuestoAsync(presupuesto);
    }

    public async Task EliminarPresupuestoAsync(long idPresupuesto, long idUsuario)
    {
        Presupuesto? presupuesto = await _presupuestoRepository.BuscarPorIdAsync(idPresupuesto, idUsuario);
    
        if (presupuesto == null)
        {
            throw new NoEncontradoExcepcion("El presupuesto no existe o ya fue eliminado.");
        }
        
        presupuesto.CambiarEstado(false);

        await _presupuestoRepository.ActualizarPresupuestoAsync(presupuesto);
    }

    public async Task<RespuestaPresupuestoDto> ObtenerPresupuestoPorIdAsync(long idPresupuesto, long idUsuario)
    {
        Presupuesto? presupuesto = await _presupuestoRepository.BuscarPorIdAsync(idPresupuesto, idUsuario);
    
        if (presupuesto == null)
        {
            throw new NoEncontradoExcepcion("El presupuesto no existe o no te pertenece.");
        }
        
        decimal totalGastado = await _gastoRepository
            .ObtenerTotalGastadoPorCategoriaYMesAsync(idUsuario, presupuesto.CategoriaId, presupuesto.Month, presupuesto.Year);

        decimal porcentaje = 0;
        if (presupuesto.MontoMaximo > 0)
        {
            porcentaje = (totalGastado / presupuesto.MontoMaximo) * 100;
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
            PorcentajeConsumido = Math.Round(porcentaje, 2)
        };
    }
}