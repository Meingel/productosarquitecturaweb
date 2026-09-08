using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductosApiNet8.Data;
using ProductosApiNet8.Services;
using System.Security.Cryptography;
using System.Text;
// Importo las clases relacionada con la configuración y las operaciones disponibles en GraphQL
using ProductosApiNet8.GraphQL;



// Creo el constructor de la aplicación y cargo la configuración disponible.
var builder = WebApplication.CreateBuilder(args);

// Registro los controladores que exponen los endpoints REST.
builder.Services.AddControllers();
// Habilito la generación de metadatos para Swagger.
builder.Services.AddEndpointsApiExplorer();
// Documento la API Key para que pueda probarse desde Swagger.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Clave de acceso requerida para los endpoints de productos."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Obtengo la conexión definida en appsettings o en User Secrets.
var connectionString = builder.Configuration.GetConnectionString("ProductosDb")
    ?? throw new InvalidOperationException("No se configuró la conexión ProductosDb.");

// Configuro Entity Framework Core para trabajar con MySQL.
builder.Services.AddDbContext<ProductosDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// Registro el servicio de productos para inyectarlo en el controlador.
builder.Services.AddScoped<IProductoService, ProductoService>();

// CONFIGURACIÓN DE GRAPHQL
// Registra Hot Chocolate como servidor GraphQL.
//
// Query contiene las operaciones de lectura.
//
// Mutation contiene las operaciones que modifican información:
// crear, actualizar y eliminar productos.
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();


// Construyo la aplicación con los servicios registrados.
var app = builder.Build();

// Expongo Swagger únicamente durante el desarrollo local.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Recupero la API Key configurada sin incluirla directamente en el código.
var apiKey = app.Configuration["ApiKey"]
    ?? throw new InvalidOperationException("No se configuró ApiKey.");

// Protejo los endpoints de productos mediante una clave enviada en X-API-Key.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/productos"))
    {
        var providedApiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        var expectedBytes = Encoding.UTF8.GetBytes(apiKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedApiKey ?? string.Empty);

        if (expectedBytes.Length != providedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { mensaje = "API Key ausente o inválida." });
            return;
        }
    }

    await next();
});

// Activo el middleware estándar de autorización de ASP.NET Core.
app.UseAuthorization();

// Mapeo los controladores a las rutas HTTP de la aplicación.
app.MapControllers();

// Creo la tabla si todavía no existe en la base de datos local.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductosDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// Publico el endpoint principal de GraphQL en la ruta /graphql.
// Hot Chocolate utilizara por defecto la rfuta /graphql
// Desde esta ruta los clientes pofrán ejecutar consultas declarativas sobre la información expuesta por el esquema GraphQL
app.MapGraphQL();

// Inicio la aplicación y mantengo disponible el servidor HTTP.
await app.RunAsync();

