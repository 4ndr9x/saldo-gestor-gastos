using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestorGastos.Services.DTOs.DTOs_de_Usuario;
using Microsoft.IdentityModel.Tokens;

namespace GestorGastos.Services.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepositorio;
    private readonly IGastoRepository _gastoRepositorio;
    private readonly ITasaCambioService _tasaCambioService;
    private readonly IPresupuestoRepository _presupuestoRepositorio;
    private readonly IConfiguration _configuracion;

    public UsuarioService(IUsuarioRepository usuarioRepositorio, IGastoRepository gastoRepositorio,
        ITasaCambioService tasaCambioService, IPresupuestoRepository presupuestoRepositorio, IConfiguration configuracion)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _gastoRepositorio = gastoRepositorio;
        _tasaCambioService = tasaCambioService;
        _presupuestoRepositorio = presupuestoRepositorio;
        _configuracion = configuracion;
    }
    
    public async Task<long> RegistrarUsuarioAsync(RegistroDto usuarioRecibido)
    {
        Usuario? usuarioDb = await _usuarioRepositorio.BuscarUsuarioPorEmailAsync(usuarioRecibido.Email);
        
        if (usuarioDb != null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        string hashPassword = BCrypt.Net.BCrypt.HashPassword(usuarioRecibido.Password);

        Usuario usuarioParaRegistrar = new Usuario(usuarioRecibido.Nombre, usuarioRecibido.Email, hashPassword, usuarioRecibido.MonedaUsada.ToUpper());

        await _usuarioRepositorio.RegistrarUsuarioAsync(usuarioParaRegistrar);

        return usuarioParaRegistrar.Id;

    }

    public async Task<RespuestaAuthDto> AutenticarUsuarioAsync(InicioSesionDto usuarioRecibido)
    {
        Usuario usuarioDb = await ObtenerUsuarioPorEmailAsync(usuarioRecibido.Email);

        bool esValido = BCrypt.Net.BCrypt.Verify(usuarioRecibido.Password, usuarioDb.PasswordHash);

        if (!esValido)
        {
            throw new CredencialesInvalidasExcepcion("Credenciales invalidas.");
        }

        var llaveSecreta = _configuracion["JwtSettings:SecretKey"];
        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(llaveSecreta!));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioDb.Id.ToString()),
            new Claim(ClaimTypes.Email, usuarioDb.Email),
            new Claim(ClaimTypes.Name, usuarioDb.Nombre),
            new Claim("monedaUsada", usuarioDb.MonedaUsada)
        };

        var opcionesToken = new JwtSecurityToken(issuer: _configuracion["JwtSettings:Issuer"],
            audience: _configuracion["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuracion["JwtSettings:DurationInMinutes"])),
            signingCredentials: credenciales);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(opcionesToken);

        return new RespuestaAuthDto {Token = tokenString, NombreUsuario = usuarioDb.Nombre, Email = usuarioDb.Email, MonedaUsada = usuarioDb.MonedaUsada};
    }

    public async Task ActualizarMonedaUsadaAsync(long idUsuario, ActualizarMonedaUsadaDto monedaRecibida)
    {
        Usuario usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        string nuevaMoneda = monedaRecibida.MonedaUsada.Trim().ToUpper();
        
        string monedaAnterior = usuarioDb.MonedaUsada;
        
        if (monedaAnterior.Equals(nuevaMoneda, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictoExcepcion("La moneda a la que intentaste cambiar es la misma ya asignada.");
        }
        
        var gastos = (await _gastoRepositorio.ObtenerPorUsuarioAsync(idUsuario)).ToList();
        var cacheTasas = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var gasto in gastos)
        {
            if (!cacheTasas.TryGetValue(gasto.Moneda, out decimal tasaGastoANueva))
            {
                tasaGastoANueva = await _tasaCambioService.ObtenerTasaCambioAsync(gasto.Moneda, nuevaMoneda);
                cacheTasas[gasto.Moneda] = tasaGastoANueva;
            }
            
            gasto.RecalcularConversionMoneda(tasaGastoANueva);
        }

        await _gastoRepositorio.ActualizarGastosMasivoAsync(gastos);
        
        var presupuestos = (await _presupuestoRepositorio.ObtenerTodosPorIdAsync(idUsuario)).ToList();
        
        decimal tasaViejaANueva = await _tasaCambioService.ObtenerTasaCambioAsync(monedaAnterior, nuevaMoneda);

        foreach (var presupuesto in presupuestos)
        {
            presupuesto.CambiarMontoMaximo(presupuesto.MontoMaximo * tasaViejaANueva);
        }

        await _presupuestoRepositorio.ActualizarPresupuestosMasivoAsync(presupuestos);
        
        usuarioDb.CambiarMonedaUsada(nuevaMoneda);
        await _usuarioRepositorio.ActualizarUsuarioAsync(usuarioDb);
    }

    public async Task ActualizarNombreAsync(long idUsuario, ActualizarPerfilDto usuarioRecibido)
    {
        Usuario usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        
        if (usuarioDb.Nombre.Trim().Equals(usuarioRecibido.Nombre.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DatosErroneosExcepcion(
                "Ocurrió un error con la información enviada.", 
                new List<string> { "El nuevo nombre debe ser diferente al actual." }
            );
        }

        usuarioDb.CambiarNombre(usuarioRecibido.Nombre);
        await _usuarioRepositorio.ActualizarUsuarioAsync(usuarioDb);
    }

    public async Task ActualizarPasswordAsync(long idUsuario, CambiarPasswordDto usuarioRecibido)
    {
        
        Usuario usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);

        bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(usuarioRecibido.PasswordActual, usuarioDb.PasswordHash);

        if (!passwordCorrecta)
        {
            List<string> detalles = new List<string> {"La contraseña actual ingresada es erronea."};
            throw new DatosErroneosExcepcion("La contraseña no es correcta.", detalles);
        }

        if (usuarioRecibido.PasswordActual == usuarioRecibido.PasswordNuevo)
        {
            throw new DatosErroneosExcepcion(
                "Ocurrió un error con la información enviada.",
                new List<string> { "La nueva contraseña no puede ser igual a la actual." }
            );
        }

        string passwordNuevoEncriptado = BCrypt.Net.BCrypt.HashPassword(usuarioRecibido.PasswordNuevo);
        
        usuarioDb.CambiarPasswordHash(passwordNuevoEncriptado);

        await _usuarioRepositorio.ActualizarUsuarioAsync(usuarioDb);

    }
    
    public async Task EliminarCuentaAsync(long idUsuario)
    {
        Usuario usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        
        usuarioDb.CambiarEstado(false);
        await _usuarioRepositorio.ActualizarUsuarioAsync(usuarioDb);
    }

    public async Task<RespuestaPerfilDto> ObtenerMiPerfil(long idUsuario)
    {
        Usuario usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);

        return new RespuestaPerfilDto
        {
            Id = usuarioDb.Id,
            Email = usuarioDb.Email,
            MonedaUsada = usuarioDb.MonedaUsada,
            Nombre = usuarioDb.Nombre
        };

    }

    private async Task<Usuario> ObtenerUsuarioPorIdAsync(long idUsuario)
    {
        Usuario? usuarioDb = await _usuarioRepositorio.BuscarUsuarioPorIdAsync(idUsuario);
        
        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        return usuarioDb;
    }

    private async Task<Usuario> ObtenerUsuarioPorEmailAsync(string email)
    {
        Usuario? usuarioDb = await _usuarioRepositorio.BuscarUsuarioPorEmailAsync(email);
        
        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        return usuarioDb;
    }

}