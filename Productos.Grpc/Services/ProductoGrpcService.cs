using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Grpc.Core;
using Productos.Grpc.Contratos;
using ProductosApiNet8.DTOs;
using ProductosApiNet8.Services;

namespace Productos.Grpc.Services;

// Implemento el contrato gRPC reutilizando el servicio que realiza el CRUD con Entity Framework.
public sealed class ProductoGrpcService(IProductoService service, ProductoEventos eventos)
    : ProductosService.ProductosServiceBase
{
    public override async Task<ListaProductos> ListarProductos(Vacio request, ServerCallContext context)
    {
        var respuesta = new ListaProductos();
        respuesta.Productos.AddRange((await service.ObtenerTodosAsync(context.CancellationToken)).Select(Convertir));
        return respuesta;
    }

    public override async Task<Producto> ObtenerProducto(ProductoId request, ServerCallContext context)
        => Convertir(await service.ObtenerPorIdAsync(request.Id, context.CancellationToken) ?? throw NoEncontrado());

    public override async Task<Producto> CrearProducto(ProductoEntrada request, ServerCallContext context)
    {
        var producto = await service.CrearAsync(ConvertirEntrada(request), context.CancellationToken);
        eventos.Publicar("CREADO", producto.Id);
        return Convertir(producto);
    }

    public override async Task<Producto> ActualizarProducto(ActualizarProductoEntrada request, ServerCallContext context)
    {
        var entrada = ConvertirEntrada(request.Producto);
        if (!await service.ActualizarAsync(request.Id, entrada, context.CancellationToken)) throw NoEncontrado();
        eventos.Publicar("ACTUALIZADO", request.Id);
        return Convertir(new ProductoResponse(request.Id, entrada.Nombre, entrada.Descripcion, entrada.Precio));
    }

    public override async Task<ResultadoOperacion> EliminarProducto(ProductoId request, ServerCallContext context)
    {
        if (!await service.EliminarAsync(request.Id, context.CancellationToken)) throw NoEncontrado();
        eventos.Publicar("ELIMINADO", request.Id);
        return new ResultadoOperacion { Exitoso = true, Mensaje = "Producto eliminado." };
    }

    public override async Task ObservarCambios(Vacio request, IServerStreamWriter<ProductoEvento> stream, ServerCallContext context)
    {
        var suscripcion = eventos.Suscribir();
        try
        {
            // Confirmo la conexión antes de esperar los eventos de creación, actualización y eliminación.
            await stream.WriteAsync(new ProductoEvento { Operacion = "CONECTADO" });
            await foreach (var evento in suscripcion.Reader.ReadAllAsync(context.CancellationToken))
                await stream.WriteAsync(evento);
        }
        finally { eventos.Retirar(suscripcion.Id); }
    }

    private static RpcException NoEncontrado() => new(new Status(StatusCode.NotFound, "El producto no existe."));

    private static ProductoRequest ConvertirEntrada(ProductoEntrada? entrada)
    {
        if (entrada is null) throw new ValidationException("Debe enviar los datos del producto.");
        if (!decimal.TryParse(entrada.Precio, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out var precio))
            throw new ValidationException("El precio debe ser un número decimal con punto, por ejemplo 249.90.");
        return new ProductoRequest { Nombre = entrada.Nombre, Descripcion = entrada.HasDescripcion ? entrada.Descripcion : null, Precio = precio };
    }

    private static Producto Convertir(ProductoResponse origen)
    {
        var producto = new Producto { Id = origen.Id, Nombre = origen.Nombre, Precio = origen.Precio.ToString("0.00", CultureInfo.InvariantCulture) };
        if (origen.Descripcion is not null) producto.Descripcion = origen.Descripcion;
        return producto;
    }
}
