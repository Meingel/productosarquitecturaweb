# ProductosApiNet8

API RESTful para gestionar productos, desarrollada con ASP.NET Core 8, Entity Framework Core y MySQL.

## Arquitectura

- `Controllers`: expone el contrato HTTP y los códigos de respuesta.
- `Services`: contiene los casos de uso CRUD.
- `Data`: configura Entity Framework Core y el mapeo de la entidad.
- `Models`: entidad persistente `Producto`.
- `DTOs`: contratos de entrada y salida, con validaciones.

La tabla `productos` se crea automáticamente al iniciar la aplicación mediante `EnsureCreatedAsync()`. No se escriben consultas SQL manuales.

## Requisitos

- .NET SDK 8.
- MySQL Server local ejecutándose en el puerto `3306`.
- Base de datos `productos_db` y usuario `productos_app` creados con el script suministrado.

## Ejecución

```powershell
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet run
```

En desarrollo, Swagger está disponible en `http://localhost:5000/swagger` (el puerto puede variar según el perfil de ejecución).

La conexión y la API Key se encuentran visibles en `appsettings.json` para facilitar las pruebas de este ejercicio.

## Endpoints

| Método | Ruta | Descripción | Respuestas principales |
|---|---|---|---|
Todos los endpoints requieren el encabezado `X-API-Key: ProductosApiKey2026`. Si la clave falta o es incorrecta, la API responde `401 Unauthorized`.

| Método | Ruta | Descripción | Respuestas principales |
|---|---|---|---|
| GET | `/api/productos` | Lista productos | `200 OK`, `401 Unauthorized` |
| GET | `/api/productos/{id}` | Consulta un producto | `200 OK`, `401`, `404 Not Found` |
| POST | `/api/productos` | Crea un producto | `201 Created`, `400`, `401` |
| PUT | `/api/productos/{id}` | Actualiza un producto | `204 No Content`, `400`, `401`, `404` |
| DELETE | `/api/productos/{id}` | Elimina un producto | `204 No Content`, `401`, `404` |

### JSON de ejemplo

```json
{
  "nombre": "Teclado mecánico",
  "descripcion": "Teclado con conexión USB-C",
  "precio": 249.90
}
```

El nombre debe tener entre 2 y 120 caracteres, la descripción máximo 500 y el precio debe ser mayor que cero. Los errores de validación son devueltos automáticamente como `400 Bad Request` por `[ApiController]`.