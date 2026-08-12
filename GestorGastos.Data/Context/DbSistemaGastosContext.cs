using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Context;

public class DbSistemaGastosContext : DbContext
{

    public DbSistemaGastosContext(DbContextOptions<DbSistemaGastosContext> db) : base(db)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Monto)
            .HasPrecision(18, 2); 
        
        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.Categoria)
            .WithMany()
            .HasForeignKey(g => g.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.MetodoPago)
            .WithMany()
            .HasForeignKey(g => g.MetodoPagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Gasto> Gastos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<MetodoPago> MetodoPagos { get; set; }

}