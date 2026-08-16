using GestorGastos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Data.Context;

public class DbSistemaGastosContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Gasto> Gastos { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    
    public DbSistemaGastosContext(DbContextOptions<DbSistemaGastosContext> db) : base(db)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("GestorGastos");
        
        // Modelado de Usuario
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Usuario>()
            .Property(u => u.PasswordHash)
            .HasMaxLength(200);
        
        modelBuilder.Entity<Usuario>()
            .Property(u => u.MonedaUsada)
            .IsRequired()
            .HasMaxLength(4);
        
        // Modelado de Gasto
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Concepto)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Descripcion)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoFinal)
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
        
        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoOriginal)
            .HasPrecision(18, 2); 
        
        modelBuilder.Entity<Gasto>()
            .Property(g => g.TasaCambio)
            .HasPrecision(18, 10); 
        
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

            entidad.Property(m => m.Icono)
                .HasMaxLength(300);
        });
        
        // Modelado de los presupuestos
        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.MontoMaximo)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Presupuesto>(entidad =>
        {
            entidad.ToTable("Presupuestos");

            entidad.HasKey(p => p.Id);

            entidad.HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            entidad.HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

}