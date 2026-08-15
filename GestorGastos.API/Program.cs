using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GestorGastos.API.Middlewares;
using GestorGastos.Data.Context;
using GestorGastos.Data.Repository;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Services.Interfaces;
using GestorGastos.Services.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSqlServer<DbSistemaGastosContext>(builder.Configuration.GetConnectionString("AppConnection"));

// Inyecciones de dependencias
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
builder.Services.AddScoped<IGastoService, GastoService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.MapSwagger();
    //app.MapSwaggerUI();
}

// Inyeccion de middlewares
app.UseMiddleware<TrazaDePeticionesMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<GestorErroresMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
