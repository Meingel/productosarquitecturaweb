using ProductosApiNet8.DTOs;
using ProductosApiNet8.Services;

namespace ProductosApiNet8.GraphQL;

/// <summary>
/// Define las operaciones de modificación disponibles mediante GraphQL.
///
/// En GraphQL, las operaciones que crean, actualizan o eliminan datos
/// se implementan mediante Mutations.
/// </summary>
public class Mutation
{
    /// <summary>
    /// Crea un nuevo producto utilizando la capa de servicios existente.
    /// </summary>
    /// <param name="input">
    /// Datos necesarios para crear el producto.
    /// </param>
    /// <param name="productoService">
    /// Servicio encargado de ejecutar la lógica de negocio.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar la operación si la solicitud es interrumpida.
    /// </param>
    /// <returns>
    /// Producto creado con su identificador asignado.
    /// </returns>
    public async Task<ProductoResponse> CrearProducto(
        ProductoRequest input,
        [Service] IProductoService productoService,
        CancellationToken cancellationToken)
    {
        // Delego la creación a la capa de servicios para evitar
        // que GraphQL acceda directamente a la base de datos.
        return await productoService.CrearAsync(input, cancellationToken);
    }

    /// <summary>
    /// Actualiza un producto existente a partir de su identificador.
    /// </summary>
    /// <param name="id">
    /// Identificador del producto que se desea actualizar.
    /// </param>
    /// <param name="input">
    /// Nuevos datos que serán asignados al producto.
    /// </param>
    /// <param name="productoService">
    /// Servicio encargado de ejecutar la lógica de negocio.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar la operación si la solicitud es interrumpida.
    /// </param>
    /// <returns>
    /// true si el producto fue actualizado; false si no existía.
    /// </returns>
    public async Task<bool> ActualizarProducto(
        int id,
        ProductoRequest input,
        [Service] IProductoService productoService,
        CancellationToken cancellationToken)
    {
        // El servicio devuelve true cuando encuentra y actualiza
        // el producto solicitado.
        return await productoService.ActualizarAsync(
            id,
            input,
            cancellationToken
        );
    }

    /// <summary>
    /// Elimina un producto existente a partir de su identificador.
    /// </summary>
    /// <param name="id">
    /// Identificador del producto que se desea eliminar.
    /// </param>
    /// <param name="productoService">
    /// Servicio encargado de ejecutar la lógica de negocio.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar la operación si la solicitud es interrumpida.
    /// </param>
    /// <returns>
    /// true si el producto fue eliminado; false si no existía.
    /// </returns>
    public async Task<bool> EliminarProducto(
        int id,
        [Service] IProductoService productoService,
        CancellationToken cancellationToken)
    {
        // La eliminación también se delega a la capa de servicios.
        return await productoService.EliminarAsync(
            id,
            cancellationToken
        );
    }
}