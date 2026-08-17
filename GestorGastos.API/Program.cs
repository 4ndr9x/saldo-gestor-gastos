using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GestorGastos.API.Middlewares;
using GestorGastos.Data.Context;
using GestorGastos.Data.Repository;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Services.Interfaces;
using GestorGastos.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(opciones => opciones.LowercaseUrls = true);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
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

// API usada para obtener la conversion de monedas: fxRatesAPI