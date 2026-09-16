# Pruebas de las aplicaciones GraphQL y gRPC

La solución contiene dos aplicaciones ASP.NET Core 8 independientes. `ProductosApiNet8.csproj` expone GraphQL y conserva REST; `Productos.Grpc/Productos.Grpc.csproj` expone gRPC. Ambas utilizan la biblioteca `Productos.Core`, Entity Framework Core y la conexión MySQL existente. No se modificaron las credenciales.

## 1. Iniciar las dos aplicaciones

Mantener MySQL en ejecución. Desde la raíz del repositorio, abrir dos terminales.

Terminal 1, GraphQL:

```powershell
dotnet run --project ProductosApiNet8.csproj --launch-profile http
```

Terminal 2, gRPC:

```powershell
dotnet run --project Productos.Grpc/Productos.Grpc.csproj
```

GraphQL: `http://localhost:5053/graphql`. REST: `http://localhost:5053/swagger`. gRPC: `localhost:5054`, sin TLS y con HTTP/2. Si una aplicación ya está ejecutándose, no abrir otra instancia en el mismo puerto. En Visual Studio se pueden configurar ambos proyectos como proyectos de inicio; para gRPC se utiliza el proyecto directamente, no IIS Express.

En una instalación nueva, restaurar los paquetes desde NuGet antes de iniciar:

```powershell
dotnet restore ProductosApiNet8.csproj --source https://api.nuget.org/v3/index.json
dotnet restore Productos.Grpc/Productos.Grpc.csproj --source https://api.nuget.org/v3/index.json
dotnet build ProductosApiNet8.slnx --no-restore -m:1
```

La base y el usuario se configuran como se describe en el README original. Las tablas y todas las operaciones CRUD se gestionan mediante EF Core. `EnsureCreatedAsync()` sirve para inicializar este esquema educativo; no actualiza tablas existentes cuando cambia el modelo.

## 2. Probar GraphQL en Postman

Crear una solicitud GraphQL con URL `http://localhost:5053/graphql`. Alternativamente, usar una solicitud HTTP POST, seleccionar Body → GraphQL y pegar cada operación. El archivo `Ejemplos/productos.graphql` contiene las operaciones. Ejecutar una por vez y reemplazar el identificador por el devuelto al crear.

1. Ejecutar `crearProducto`; guardar el `id` de la respuesta.
2. Ejecutar `productos`, solicitando solamente `nombre` y `precio` para demostrar la consulta declarativa.
3. Ejecutar `productoPorId` con el identificador creado.
4. Ejecutar `actualizarProducto` y consultar nuevamente para comprobar la persistencia.
5. Ejecutar `eliminarProducto` y volver a consultar: se obtiene `errors` con código `NOT_FOUND`.
6. Intentar crear con precio negativo o nombre vacío: se obtiene `BAD_USER_INPUT`.

Los errores GraphQL se comprueban en la colección `errors` del cuerpo, aunque la respuesta HTTP pueda tener estado 200. Los errores internos se registran en el servidor y se devuelve un mensaje controlado.

## 3. Probar gRPC en Postman

1. Usar Postman de escritorio y crear una solicitud **gRPC**.
2. Escribir `localhost:5054` y desactivar TLS si aparece habilitado.
3. Cargar la definición mediante **server reflection**. Como alternativa, importar `Productos.Grpc/Protos/productos.proto`.
4. Seleccionar el servicio `productos.ProductosService` y el método que se desea invocar.

No utilizar una solicitud HTTP normal ni Swagger para probar gRPC. Los cuerpos JSON siguientes son la representación que muestra Postman; el intercambio gRPC utiliza Protocol Buffers.

**CrearProducto**:

```json
{
  "nombre": "Teclado gRPC",
  "descripcion": "Producto creado mediante Protocol Buffers",
  "precio": "249.90"
}
```

Guardar el `id` recibido. El precio se envía como texto decimal con punto para preservar la precisión monetaria; dentro del modelo y la base es `decimal(10,2)`. No usar comas, símbolos monetarios ni más de dos decimales.

**ListarProductos**:

```json
{}
```

**ObtenerProducto** y **EliminarProducto**, reemplazando el identificador:

```json
{ "id": 1 }
```

**ActualizarProducto**:

```json
{
  "id": 1,
  "producto": {
    "nombre": "Teclado actualizado",
    "descripcion": "Actualización mediante gRPC",
    "precio": "199.50"
  }
}
```

Consultar después de actualizar. Eliminar únicamente el producto creado para la demostración. Consultarlo después de eliminar devuelve `NOT_FOUND`. Crear con `"precio": "-1"`, `"precio": "1.234"` o nombre vacío devuelve `INVALID_ARGUMENT`. Los fallos inesperados se registran y se traducen a `INTERNAL` sin exponer detalles de la base.

## 4. Demostrar comunicación en tiempo real

En otra pestaña gRPC, ejecutar `ObservarCambios` con `{}` y mantener la llamada abierta. Primero aparece el evento `CONECTADO`. En la pestaña del CRUD, crear, actualizar y eliminar un producto. La pestaña de streaming recibe `CREADO`, `ACTUALIZADO` y `ELIMINADO` con el identificador correspondiente, sin repetir solicitudes de consulta. Cancelar la llamada al terminar.

Este streaming notifica cambios realizados por la aplicación gRPC después de guardar en MySQL. Los cambios realizados por GraphQL se ven al consultar la base compartida, pero no generan eventos en este canal. Los eventos son transitorios, no un historial: cada cliente mantiene hasta 100 eventos pendientes y un consumidor lento puede perder los más antiguos. Esta implementación educativa funciona dentro de una instancia del servidor.

## 5. Prueba automática opcional

Con ambos servidores en ejecución:

```powershell
dotnet restore Pruebas/Pruebas.csproj --source https://api.nuget.org/v3/index.json
dotnet run --project Pruebas/Pruebas.csproj --no-restore
```

El ejecutable verifica CRUD, selección de campos, acceso compartido a MySQL, errores y streaming. Crea productos temporales y los elimina al terminar. El resultado esperado es `PRUEBAS COMPLETADAS`. No utiliza una base simulada.

## Estructura para explicar el código

| Componente | Responsabilidad |
|---|---|
| `Models/Producto.cs` | Entidad con identificador, nombre, descripción y precio. |
| `Data/ProductosDbContext.cs` | Mapeo y restricciones de la tabla mediante EF Core. |
| `Services/ProductoService.cs` | CRUD asíncrono sin SQL manual. |
| `Services/ProductoValidacion.cs` | Validación compartida de entradas e identificadores. |
| `Productos.Core` | Compila y comparte los archivos anteriores entre ambas aplicaciones. |
| `GraphQL/Query.cs` y `Mutation.cs` | Operaciones declarativas de lectura y escritura. |
| `GraphQL/ProductoErrorFilter.cs` | Traducción de errores al formato GraphQL. |
| `Productos.Grpc/Protos/productos.proto` | Contrato y mensajes gRPC. |
| `Productos.Grpc/Services/ProductoGrpcService.cs` | Implementación de métodos y streaming. |
| `Productos.Grpc/Services/ErroresInterceptor.cs` | Traducción de excepciones a estados gRPC. |
| `Productos.Grpc/Services/ProductoEventos.cs` | Distribución de eventos a clientes conectados. |

Las validaciones se aplican en el servicio compartido y no dependen del protocolo. Se mantiene la API Key original para las rutas REST; GraphQL y gRPC están disponibles sin esa cabecera en este ejercicio local.

## Comparación para la actividad

| Aspecto | REST existente | GraphQL | gRPC |
|---|---|---|---|
| Contrato | Rutas, verbos HTTP y OpenAPI | Esquema de tipos, queries y mutations | Servicio y mensajes `.proto` |
| Consulta | El servidor define la respuesta de cada ruta | El cliente selecciona los campos necesarios | El cliente invoca un método con mensajes definidos |
| Representación habitual | JSON | JSON | Protocol Buffers binario |
| Ventaja en este ejercicio | Pruebas simples desde Swagger | Selección declarativa y varias lecturas en una solicitud | Contrato tipado, código generado y streaming |
| Desafío | Varias rutas y respuestas de forma fija | Validación y control de complejidad de consultas | Configuración HTTP/2 y cliente compatible |

El video de máximo 15 minutos debe mostrar la construcción explicando estos archivos, el CRUD y los errores de ambas APIs, la selección declarativa en GraphQL y el streaming gRPC. La publicación y la presentación de la URL de GitHub quedan a cargo del estudiante. Este documento es una guía de pruebas; el guion narrado se preparará cuando se solicite.
