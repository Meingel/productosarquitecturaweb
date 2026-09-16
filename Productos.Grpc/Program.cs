using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProductosApiNet8.Data;
using ProductosApiNet8.Services;
using Productos.Grpc.Services;

// Cargo la misma configuración que utiliza GraphQL, copiada al directorio de salida.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Reservo un puerto HTTP/2 local para gRPC; en Postman se utiliza sin TLS.
builder.WebHost.ConfigureKestrel(options =>
    options.ListenLocalhost(5054, endpoint => endpoint.Protocols = HttpProtocols.Http2));
var connectionString = builder.Configuration.GetConnectionString("ProductosDb")
    ?? throw new InvalidOperationException("No se configuró la conexión ProductosDb.");
builder.Services.AddDbContext<ProductosDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddSingleton<ProductoEventos>();
builder.Services.AddGrpc(options => options.Interceptors.Add<ErroresInterceptor>());
builder.Services.AddGrpcReflection();
var app = builder.Build();

// El ORM crea las tablas a partir del modelo; no ejecuto SQL manual para el CRUD.
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<ProductosDbContext>().Database.EnsureCreatedAsync();

app.MapGrpcService<ProductoGrpcService>();
// La reflexión permite que Postman descubra automáticamente el contrato educativo.
app.MapGrpcReflectionService();
await app.RunAsync();
