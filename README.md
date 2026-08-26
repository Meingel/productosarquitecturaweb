# ProductosApiNet8

API RESTful para gestionar productos, desarrollada con ASP.NET Core 8, Entity Framework Core y MySQL.

## Arquitectura


La tabla `productos` se crea automáticamente al iniciar la aplicación mediante `EnsureCreatedAsync()`. No se escriben consultas SQL manuales.

## Requisitos


## Ejecución

```powershell
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet run
```

## Crear un commit y subir cambios a GitHub

Ejecutar los comandos desde la carpeta raíz, donde se encuentra `ProductosApiNet8.csproj`:

```powershell
dotnet build
git status
git add .
git status
git commit -m "Actualiza API REST de productos"
git push
```

Después del `push`, actualizar el repositorio en GitHub y confirmar que los archivos modificados aparezcan correctamente. Si se desea consultar el historial de commits, ejecutar:

```powershell
git log --oneline -5
```

En desarrollo, Swagger está disponible en `http://localhost:5000/swagger` (el puerto puede variar según el perfil de ejecución).

# ProductosApiNet8

API RESTful para administrar productos mediante operaciones CRUD. La solución está desarrollada con ASP.NET Core 8, Entity Framework Core y MySQL.

## Objetivo de la entrega

La aplicación permite crear, consultar, actualizar y eliminar productos almacenados en una base de datos relacional. También incorpora validación de datos, manejo de errores, documentación con Swagger y control de acceso mediante API Key.

## Tecnologías

- .NET 8 y ASP.NET Core Web API.
- Entity Framework Core como ORM.
- Pomelo.EntityFrameworkCore.MySql como proveedor de conexión con MySQL.
- MySQL Server como base de datos local.
- Swagger/OpenAPI para documentar y probar la API.

## Arquitectura del proyecto

```text
ProductosApiNet8/
|-- Controllers/       Endpoints HTTP de la API
|-- Data/              Contexto y configuración de Entity Framework Core
|-- DTOs/              Modelos de entrada y salida
|-- Models/            Entidades persistentes
|-- Services/          Lógica de las operaciones CRUD
|-- Properties/        Configuración de ejecución local
|-- Program.cs         Registro de servicios y pipeline HTTP
|-- appsettings.json   Configuración de conexión y API Key
|-- guion.md           Guion para el video de sustentación
```

Esta separación mantiene responsabilidades claras: el controlador atiende HTTP, el servicio contiene la lógica de aplicación y el contexto administra la persistencia mediante el ORM.

## Requisitos previos

Instalar y tener disponibles:

- Visual Studio 2022 con la carga de trabajo **ASP.NET y desarrollo web**.
- .NET 8 SDK.
- MySQL Server local ejecutándose en el puerto `3306`.
- MySQL Workbench, phpMyAdmin u otra herramienta para ejecutar el script SQL.

## Primera configuración de MySQL

Abrir MySQL Workbench o phpMyAdmin, conectarse con un usuario administrador y ejecutar el siguiente script completo:

```sql
CREATE DATABASE IF NOT EXISTS productos_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'productos_app'@'localhost'
    IDENTIFIED BY 'Productos2026!';

ALTER USER 'productos_app'@'localhost'
    IDENTIFIED BY 'Productos2026!';

GRANT ALL PRIVILEGES ON productos_db.*
    TO 'productos_app'@'localhost';

FLUSH PRIVILEGES;

SHOW DATABASES LIKE 'productos_db';
SHOW GRANTS FOR 'productos_app'@'localhost';
```

El comando `ALTER USER` garantiza que la contraseña coincida aunque el usuario ya existiera. Las consultas `SHOW` permiten comprobar que la base fue creada y que el usuario tiene permisos.

## Configuración de la aplicación

La conexión está definida en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ProductosDb": "Server=localhost;Port=3306;Database=productos_db;User=productos_app;Password=Productos2026!;CharSet=utf8mb4"
  },
  "ApiKey": "ProductosApiKey2026"
}
```

Estas credenciales están visibles intencionalmente para facilitar la ejecución de este ejercicio. En un sistema real deben reemplazarse por User Secrets, variables de entorno o un gestor de secretos antes de publicar el proyecto.

## Primera ejecución desde Visual Studio 2022

1. Abrir la carpeta o el archivo `ProductosApiNet8.csproj` en Visual Studio 2022.
2. Esperar a que Visual Studio restaure los paquetes NuGet.
3. Confirmar que MySQL Server está iniciado.
4. Confirmar que se ejecutó el script anterior y que la contraseña coincide con `appsettings.json`.
5. Seleccionar el perfil `http` o `IIS Express` en la barra superior.
6. Presionar `Ctrl + F5` o el botón de inicio.
7. Abrir la dirección que muestre Visual Studio y agregar `/swagger`.

Por ejemplo:

```text
http://localhost:5053/swagger
```

El puerto puede cambiar según el perfil seleccionado.

## Creación automática de tablas

La tabla no debe crearse manualmente. Durante el inicio, `Program.cs` obtiene el `ProductosDbContext` y ejecuta:

```csharp
await context.Database.EnsureCreatedAsync();
```

Entity Framework Core verifica si existe la tabla `productos`. Si no existe, la crea utilizando el modelo `Producto` y la configuración del contexto. El resultado esperado es una tabla con estas columnas:

| Columna | Tipo | Características |
|---|---|---|
| `Id` | `int` | Clave primaria y autoincremental |
| `Nombre` | `varchar(120)` | Obligatorio |
| `Descripcion` | `varchar(500)` | Opcional |
| `Precio` | `decimal(10,2)` | Obligatorio y positivo |

En ejecuciones posteriores, `EnsureCreatedAsync()` detecta que la tabla ya existe y no la vuelve a crear. Para esta actividad se utiliza este mecanismo porque el modelo es pequeño y no requiere migraciones evolutivas. En un sistema productivo con cambios frecuentes de esquema se recomienda utilizar migraciones de Entity Framework Core.

## Control de acceso

Los endpoints de productos requieren el siguiente encabezado:

```http
X-API-Key: ProductosApiKey2026
```

En Swagger:

1. Presionar **Authorize**.
2. Escribir `ProductosApiKey2026`.
3. Presionar **Authorize** y cerrar la ventana.
4. Ejecutar los endpoints.

Si la clave falta o es incorrecta, la respuesta es `401 Unauthorized`.

## Endpoints CRUD

| Método | Ruta | Descripción | Respuestas principales |
|---|---|---|---|
| `GET` | `/api/productos` | Lista todos los productos | `200`, `401` |
| `GET` | `/api/productos/{id}` | Consulta un producto | `200`, `401`, `404` |
| `POST` | `/api/productos` | Crea un producto | `201`, `400`, `401` |
| `PUT` | `/api/productos/{id}` | Actualiza un producto | `204`, `400`, `401`, `404` |
| `DELETE` | `/api/productos/{id}` | Elimina un producto | `204`, `401`, `404` |

### Cuerpo para crear o actualizar

```json
{
  "nombre": "Teclado mecanico",
  "descripcion": "Teclado USB-C para pruebas",
  "precio": 249.90
}
```

El nombre debe tener entre 2 y 120 caracteres, la descripción puede tener hasta 500 caracteres y el precio debe ser mayor que cero. Los datos inválidos generan `400 Bad Request`.

## Repositorio y ejecución reproducible

Para ejecutar el proyecto después de clonarlo:

```powershell
git clone URL_DEL_REPOSITORIO
cd ProductosApiNet8
dotnet restore
dotnet build
dotnet run
```

Antes de `dotnet run`, se debe ejecutar el script SQL de esta documentación y verificar que MySQL esté activo.
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