using ProductosApiNet8.DTOs;

namespace ProductosApiNet8.Services;

// Defino las operaciones de negocio disponibles para productos.
public interface IProductoService
{
    // Consulto todos los productos de forma asíncrona.
    Task<IReadOnlyList<ProductoResponse>> ObtenerTodosAsync(CancellationToken cancellationToken);
    // Consulto un producto por su identificador.
    Task<ProductoResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    // Creo un producto y devuelvo su representación pública.
    Task<ProductoResponse> CrearAsync(ProductoRequest request, CancellationToken cancellationToken);
    // Actualizo un producto y comunico si existía.
    Task<bool> ActualizarAsync(int id, ProductoRequest request, CancellationToken cancellationToken);
    // Elimino un producto y comunico si existía.
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
}