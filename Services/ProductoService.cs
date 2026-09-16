using Microsoft.EntityFrameworkCore;
using ProductosApiNet8.Data;
using ProductosApiNet8.DTOs;
using ProductosApiNet8.Models;

namespace ProductosApiNet8.Services;

// Implemento las operaciones CRUD usando Entity Framework Core.
public class ProductoService(ProductosDbContext context) : IProductoService
{
    // Consulto los productos sin rastrear entidades que no voy a modificar.
    public async Task<IReadOnlyList<ProductoResponse>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await context.Productos.AsNoTracking().OrderBy(producto => producto.Id)
            .Select(producto => ToResponse(producto)).ToListAsync(cancellationToken);

    // Busco un producto por id y devuelvo null si no existe.
    public async Task<ProductoResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        ProductoValidacion.ValidarId(id);
        return await context.Productos.AsNoTracking().Where(producto => producto.Id == id)
            .Select(producto => ToResponse(producto)).SingleOrDefaultAsync(cancellationToken);
    }

    // Creo una entidad a partir de los datos validados y la guardo.
    public async Task<ProductoResponse> CrearAsync(ProductoRequest request, CancellationToken cancellationToken)
    {
        ProductoValidacion.Validar(request);
        // Normalizo los textos antes de persistirlos.
        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Precio = request.Precio
        };
        // Marco la entidad para que Entity Framework genere un INSERT.
        context.Productos.Add(producto);
        // Confirmo la operación en MySQL.
        await context.SaveChangesAsync(cancellationToken);
        // Devuelvo la entidad ya identificada por la base de datos.
        return ToResponse(producto);
    }

    // Actualizo los datos de una entidad existente.
    public async Task<bool> ActualizarAsync(int id, ProductoRequest request, CancellationToken cancellationToken)
    {
        ProductoValidacion.ValidarId(id);
        ProductoValidacion.Validar(request);
        // Busco la entidad rastreada que voy a modificar.
        var producto = await context.Productos.FindAsync([id], cancellationToken);
        // Informo al controlador cuando el id no existe.
        if (producto is null) return false;
        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.Precio = request.Precio;
        // Confirmo los cambios de la entidad.
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Elimino una entidad existente de la base de datos.
    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        ProductoValidacion.ValidarId(id);
        // Busco la entidad que voy a eliminar.
        var producto = await context.Productos.FindAsync([id], cancellationToken);
        // Informo al controlador cuando el id no existe.
        if (producto is null) return false;
        // Marco la entidad para generar un DELETE.
        context.Productos.Remove(producto);
        // Confirmo la eliminación en MySQL.
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Convierto la entidad persistente en el DTO de salida.
    private static ProductoResponse ToResponse(Producto producto) =>
        new(producto.Id, producto.Nombre, producto.Descripcion, producto.Precio);
}
