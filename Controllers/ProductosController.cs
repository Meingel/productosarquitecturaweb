using Microsoft.AspNetCore.Mvc;
using ProductosApiNet8.DTOs;
using ProductosApiNet8.Services;

namespace ProductosApiNet8.Controllers;

// Marco la clase como controlador de API con validación automática.
[ApiController]
// Defino la ruta base para todas las operaciones de productos.
[Route("api/productos")]
// Coordino las solicitudes HTTP mediante el servicio de productos.
public class ProductosController(IProductoService service) : ControllerBase
{
    // Expongo la operación para consultar todos los productos.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoResponse>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok(await service.ObtenerTodosAsync(cancellationToken));

    // Expongo la operación para consultar un producto específico.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var producto = await service.ObtenerPorIdAsync(id, cancellationToken);
        return producto is null ? NotFound(new { mensaje = "El producto no existe." }) : Ok(producto);
    }

    // Expongo la operación para crear un producto nuevo.
    [HttpPost]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoResponse>> Crear(ProductoRequest request, CancellationToken cancellationToken)
    {
        var producto = await service.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    // Expongo la operación para actualizar un producto existente.
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, ProductoRequest request, CancellationToken cancellationToken)
    {
        var actualizado = await service.ActualizarAsync(id, request, cancellationToken);
        return actualizado ? NoContent() : NotFound(new { mensaje = "El producto no existe." });
    }

    // Expongo la operación para eliminar un producto existente.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var eliminado = await service.EliminarAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound(new { mensaje = "El producto no existe." });
    }
}