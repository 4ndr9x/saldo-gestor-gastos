using GestorGastos.API.Middlewares;
using GestorGastos.Data.Context;
using GestorGastos.Data.Repository;
using GestorGastos.Domain.Interfaces;
using GestorGastos.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSqlServer<DbSistemaGastosContext>(builder.Configuration.GetConnectionString("AppConnection"));

// Inyecciones de dependencias
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.MapSwagger();
    app.MapSwaggerUI();
}

// Inyeccion de middlewares
app.UseMiddleware<TrazaDePeticionesMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<GestorErroresMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();