using Microsoft.EntityFrameworkCore;
using ProductosApiNet8.Models;

namespace ProductosApiNet8.Data;

// Represento la sesión de Entity Framework Core con MySQL.
public class ProductosDbContext(DbContextOptions<ProductosDbContext> options) : DbContext(options)
{
    // Expongo la colección que representa la tabla de productos.
    public DbSet<Producto> Productos => Set<Producto>();

    // Configuro nombres, restricciones y precisión de las columnas.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Defino el mapeo relacional de la entidad Producto.
        modelBuilder.Entity<Producto>(entity =>
        {
            // Utilizo la tabla productos en la base de datos.
            entity.ToTable("productos");
            // Declaro Id como clave primaria.
            entity.HasKey(producto => producto.Id);
            // Limito el nombre a 120 caracteres y lo hago obligatorio.
            entity.Property(producto => producto.Nombre).HasMaxLength(120).IsRequired();
            // Limito la descripción a 500 caracteres.
            entity.Property(producto => producto.Descripcion).HasMaxLength(500);
            // Defino diez dígitos y dos decimales para el precio.
            entity.Property(producto => producto.Precio).HasPrecision(10, 2).IsRequired();
        });
    }
}