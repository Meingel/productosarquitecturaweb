using System.Net.Http.Json;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Productos.Grpc.Contratos;

// Ejecuto pruebas reales contra las dos aplicaciones y elimino únicamente los productos que creo aquí.
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5053"), Timeout = TimeSpan.FromSeconds(15) };
using var channel = GrpcChannel.ForAddress("http://localhost:5054");
var grpc = new ProductosService.ProductosServiceClient(channel);
var graphqlId = 0;
var grpcId = 0;

void Comprobar(bool cumple, string mensaje)
{
    if (!cumple) throw new Exception("FALLÓ: " + mensaje);
    Console.WriteLine("OK: " + mensaje);
}

async Task<JsonElement> GraphQL(string query)
{
    using var response = await http.PostAsJsonAsync("/graphql", new { query });
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
    return json;
}

async Task EsperarError(Func<Task> accion, StatusCode codigo)
{
    try { await accion(); }
    catch (RpcException ex) when (ex.StatusCode == codigo)
    { Console.WriteLine("OK: gRPC " + codigo); return; }
    throw new Exception("No se recibió el error gRPC " + codigo);
}

async Task Evento(AsyncServerStreamingCall<ProductoEvento> stream, string operacion, int id)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    Comprobar(await stream.ResponseStream.MoveNext(timeout.Token), "Se recibió un evento de streaming");
    Comprobar(stream.ResponseStream.Current.Operacion == operacion && stream.ResponseStream.Current.Id == id,
        "Streaming " + operacion);
}

try
{
    var creado = await GraphQL("mutation { crearProducto(input: { nombre: \"Prueba GraphQL temporal\", descripcion: \"Prueba automática\", precio: 19.90 }) { id nombre precio } }");
    graphqlId = creado.GetProperty("data").GetProperty("crearProducto").GetProperty("id").GetInt32();
    Comprobar(graphqlId > 0, "GraphQL crea y persiste un producto");
    var lectura = await GraphQL($"{{ productoPorId(id: {graphqlId}) {{ nombre precio }} productos {{ id }} }}");
    Comprobar(!lectura.TryGetProperty("errors", out _), "GraphQL consulta varios campos raíz sin conflictos de DbContext");
    Comprobar(!lectura.GetProperty("data").GetProperty("productoPorId").TryGetProperty("id", out _), "GraphQL devuelve únicamente los campos seleccionados");
    var actualizado = await GraphQL($"mutation {{ actualizarProducto(id: {graphqlId}, input: {{ nombre: \"Actualizado GraphQL\", precio: 25.50 }}) }}");
    Comprobar(actualizado.GetProperty("data").GetProperty("actualizarProducto").GetBoolean(), "GraphQL actualiza");
    var compartido = await grpc.ObtenerProductoAsync(new ProductoId { Id = graphqlId });
    Comprobar(compartido.Precio == "25.50", "gRPC lee el cambio persistido por GraphQL en MySQL");
    var invalido = await GraphQL("mutation { crearProducto(input: { nombre: \" \" precio: -1 }) { id } }");
    Comprobar(invalido.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString() == "BAD_USER_INPUT", "GraphQL rechaza datos inválidos");
    var borrado = await GraphQL($"mutation {{ eliminarProducto(id: {graphqlId}) }}");
    Comprobar(borrado.GetProperty("data").GetProperty("eliminarProducto").GetBoolean(), "GraphQL elimina");
    var ausente = await GraphQL($"{{ productoPorId(id: {graphqlId}) {{ id }} }}");
    Comprobar(ausente.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString() == "NOT_FOUND", "GraphQL informa producto inexistente");
    graphqlId = 0;

    using var stream = grpc.ObservarCambios(new Vacio(), deadline: DateTime.UtcNow.AddSeconds(45));
    await Evento(stream, "CONECTADO", 0);
    var entrada = new ProductoEntrada { Nombre = "Prueba gRPC temporal", Descripcion = "Prueba automática", Precio = "12.34" };
    var producto = await grpc.CrearProductoAsync(entrada);
    grpcId = producto.Id;
    Comprobar(grpcId > 0 && producto.Precio == "12.34", "gRPC crea con precisión decimal");
    await Evento(stream, "CREADO", grpcId);
    var lista = await grpc.ListarProductosAsync(new Vacio());
    Comprobar(lista.Productos.Any(p => p.Id == grpcId), "gRPC lista productos");
    entrada.Nombre = "Actualizado gRPC";
    entrada.Precio = "56.78";
    await grpc.ActualizarProductoAsync(new ActualizarProductoEntrada { Id = grpcId, Producto = entrada });
    var consultado = await grpc.ObtenerProductoAsync(new ProductoId { Id = grpcId });
    Comprobar(consultado.Nombre == entrada.Nombre && consultado.Precio == entrada.Precio, "gRPC actualiza y consulta los valores persistidos");
    await Evento(stream, "ACTUALIZADO", grpcId);
    await EsperarError(async () => { await grpc.CrearProductoAsync(new ProductoEntrada { Nombre = "Inválido", Precio = "-1" }); }, StatusCode.InvalidArgument);
    await EsperarError(async () => { await grpc.CrearProductoAsync(new ProductoEntrada { Nombre = "Inválido", Precio = "1.234" }); }, StatusCode.InvalidArgument);
    await EsperarError(async () => { await grpc.ActualizarProductoAsync(new ActualizarProductoEntrada { Id = grpcId }); }, StatusCode.InvalidArgument);
    var eliminado = await grpc.EliminarProductoAsync(new ProductoId { Id = grpcId });
    Comprobar(eliminado.Exitoso, "gRPC elimina");
    await Evento(stream, "ELIMINADO", grpcId);
    await EsperarError(async () => { await grpc.ObtenerProductoAsync(new ProductoId { Id = grpcId }); }, StatusCode.NotFound);
    grpcId = 0;
    Console.WriteLine("\nPRUEBAS COMPLETADAS: GraphQL, gRPC, MySQL, validaciones y streaming.");
}
finally
{
    // Incluso ante una prueba fallida intento retirar solamente los registros temporales identificados.
    if (graphqlId > 0) await GraphQL($"mutation {{ eliminarProducto(id: {graphqlId}) }}");
    if (grpcId > 0)
    {
        try { await grpc.EliminarProductoAsync(new ProductoId { Id = grpcId }); }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound) { }
    }
}
