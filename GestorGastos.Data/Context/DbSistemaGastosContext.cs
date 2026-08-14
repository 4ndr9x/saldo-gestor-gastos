using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Context;

public class DbSistemaGastosContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Gasto> Gastos { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    
    public DbSistemaGastosContext(DbContextOptions<DbSistemaGastosContext> db) : base(db)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("GestorGastos");
        
        // Modelado de Gasto
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
        
        // Modelado de Categoria
        modelBuilder.Entity<Categoria>(entidad =>
        {
            entidad.ToTable("Categorias"); 
        
            entidad.HasKey(c => c.Id);
        
            entidad.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            entidad.HasOne(c => c.Usuario)
                .WithMany(u => u.Categorias)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Modelado de los metodos de pago
        modelBuilder.Entity<MetodoPago>(entidad =>
        {
            entidad.ToTable("MetodosPago"); 
    
            entidad.HasKey(m => m.Id);
    
            entidad.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entidad.HasOne(m => m.Usuario)
                .WithMany(u => u.MetodosPago)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); 
        });
    }

}