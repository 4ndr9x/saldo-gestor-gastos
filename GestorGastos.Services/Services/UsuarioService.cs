using GestorGastos.Domain.Exceptions;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Domain.Models;
using GestorGastos.Services.DTOs;
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
    private readonly IUsuarioRepository _repositorio;
    private readonly IConfiguration _configuracion;

    public UsuarioService(IUsuarioRepository repositorio, IConfiguration configuracion)
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
    }
    
    public async Task<long> RegistrarUsuarioAsync(RegistroDto usuarioRecibido)
    {
        Usuario? usuarioDb = await ObtenerUsuarioPorEmailAsync(usuarioRecibido.Email);

        if (usuarioDb != null)
        {
            throw new ConflictoExcepcion("El email introducido ya esta siendo usado.");
        }

        string hashPassword = BCrypt.Net.BCrypt.HashPassword(usuarioRecibido.Password);

        Usuario usuarioParaRegistrar = new Usuario(usuarioRecibido.Nombre, usuarioRecibido.Email, hashPassword);

        await _repositorio.RegistrarUsuarioAsync(usuarioParaRegistrar);

        return usuarioParaRegistrar.Id;

    }

    public async Task<RespuestaAuthDto> AutenticarUsuarioAsync(InicioSesionDto usuarioRecibido)
    {
        Usuario? usuarioDb = await ObtenerUsuarioPorEmailAsync(usuarioRecibido.Email);

        if (usuarioDb == null)
        {
            throw new CredencialesInvalidasExcepcion("Credenciales invalidas.");
        }

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
            new Claim(ClaimTypes.Name, usuarioDb.Nombre)
        };

        var opcionesToken = new JwtSecurityToken(issuer: _configuracion["JwtSettings:Issuer"],
            audience: _configuracion["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuracion["JwtSettings:DurationInMinutes"])),
            signingCredentials: credenciales);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(opcionesToken);

        return new RespuestaAuthDto {Token = tokenString, NombreUsuario = usuarioDb.Nombre, Email = usuarioDb.Email};
    }

    public async Task ActualizarMonedaUsadaAsync(long idUsuario, ActualizarMonedaUsadaDto usuarioRecibido)
    {
        Usuario? usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        
        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }
        
        usuarioDb.CambiarMonedaUsada(usuarioRecibido.MonedaUsada);
        await _repositorio.ActualizarUsuarioAsync(usuarioDb);
    }

    public async Task ActualizarPerfilAsync(long idUsuario, ActualizarPerfilDto usuarioRecibido)
    {
        
        Usuario? usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);

        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        usuarioDb.CambiarNombre(usuarioRecibido.Nombre);
        await _repositorio.ActualizarUsuarioAsync(usuarioDb);

    }

    public async Task ActualizarPasswordAsync(long idUsuario, CambiarPasswordDto usuarioRecibido)
    {
        
        Usuario? usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        
        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }

        bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(usuarioRecibido.PasswordActual, usuarioDb.PasswordHash);

        if (!passwordCorrecta)
        {
            List<string> detalles = new List<string> {"La contraseña actual ingresada es erronea."};
            throw new DatosErroneosExcepcion("La contraseña no es correcta.", detalles);
        }

        string passwordNuevoEncriptado = BCrypt.Net.BCrypt.HashPassword(usuarioRecibido.PasswordNuevo);
        
        usuarioDb.CambiarPasswordHash(passwordNuevoEncriptado);

        await _repositorio.ActualizarUsuarioAsync(usuarioDb);

    }
    
    public async Task EliminarCuentaAsync(long idUsuario)
    {
        
        Usuario? usuarioDb = await ObtenerUsuarioPorIdAsync(idUsuario);
        
        if (usuarioDb == null)
        {
            throw new NoEncontradoExcepcion("El usuario que se ha intentado buscar no existe.");
        }
        
        usuarioDb.CambiarEstado(false);
        await _repositorio.ActualizarUsuarioAsync(usuarioDb);
    }
    
    private async Task<Usuario?> ObtenerUsuarioPorIdAsync(long idUsuario)
    {
        return await _repositorio.BuscarUsuarioPorId(idUsuario);
    }

    private async Task<Usuario?> ObtenerUsuarioPorEmailAsync(string email)
    {
        return await _repositorio.BuscarUsuarioPorEmail(email);
    }

}