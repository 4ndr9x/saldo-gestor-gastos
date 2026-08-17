using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GestorGastos.API.Middlewares;
using GestorGastos.Data.Context;
using GestorGastos.Data.Repository;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Services.Interfaces;
using GestorGastos.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(opciones => opciones.LowercaseUrls = true);
builder.Services.AddOpenApi();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(opciones =>
{
    opciones.SuppressMapClientErrors = true;
    
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = contexto.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
            .ToList();
        
        var respuesta = new
        {
            codigo = 400,
            mensaje = "Ocurrió un error con la información enviada.",
            detalles = errores,
            traceId = contexto.HttpContext.TraceIdentifier
        };
        
        return new BadRequestObjectResult(respuesta);
    };
});
builder.Services.AddSqlServer<DbSistemaGastosContext>(builder.Configuration.GetConnectionString("AppConnection"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Inyecciones de dependencias
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();
builder.Services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IExportacionService, ExportacionService>();

builder.Services.AddHttpClient<ITasaCambioService, TasaCambioService>();

// == 

// Configuracion del JWT
string llaveSecreta = builder.Configuration["JwtSettings:SecretKey"]!;

builder.Services.AddAuthentication(opciones =>
{
    opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
}).AddJwtBearer(opciones =>
{
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(llaveSecreta))
    };
    
    opciones.Events = new JwtBearerEvents
    {
        
        OnChallenge = contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.StatusCode = 401;
            contexto.Response.ContentType = "application/json";

            var respuesta = new
            {
                codigo = 401,
                mensaje = "No estás autorizado.",
                detalles = new[] { "Debes iniciar sesión. El token no fue enviado, expiró o es inválido." },
                traceId = contexto.HttpContext.TraceIdentifier
            };

            return contexto.Response.WriteAsJsonAsync(respuesta);
        },
        
        OnForbidden = contexto =>
        {
            contexto.Response.StatusCode = 403;
            contexto.Response.ContentType = "application/json";

            var respuesta = new
            {
                codigo = 403,
                mensaje = "Acceso denegado.",
                detalles = new[] { "Tu usuario no tiene los permisos suficientes para realizar esta acción." },
                traceId = contexto.HttpContext.TraceIdentifier
            };

            return contexto.Response.WriteAsJsonAsync(respuesta);
        }
    };
    
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DbSistemaGastosContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error crítico al aplicar las migraciones a la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    
    if (response.StatusCode >= 400 && !response.HasStarted && 
        (string.IsNullOrEmpty(response.ContentType) || !response.ContentType.Contains("json")))
    {
        response.ContentType = "application/json";

        var mensajeGenerico = response.StatusCode switch
        {
            404 => "El endpoint o recurso solicitado no existe.",
            405 => "El método HTTP (GET/POST/PUT/DELETE) no está permitido en esta ruta.",
            415 => "Formato no soportado. Verifica que el 'Content-Type' (JSON o Archivo) sea el correcto para esta acción.",
            _ => "Ocurrió un error inesperado a nivel de servidor."
        };

        var respuesta = new
        {
            codigo = response.StatusCode,
            mensaje = mensajeGenerico,
            detalles = new List<string>(),
            traceId = context.HttpContext.TraceIdentifier
        };

        await response.WriteAsJsonAsync(respuesta);
    }
});

// Inyeccion de middlewares
app.UseMiddleware<TrazaDePeticionesMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<GestorErroresMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();