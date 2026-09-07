# Informe tecnico - LetrerosHerreraSYNCTIMEapi

## 1. Alcance

`LetrerosHerreraSYNCTIMEapi` es una API REST para gestionar el flujo comercial de una empresa de letreros. El sistema permite administrar clientes, productos, solicitudes, cotizaciones y pedidos mediante una API Minimal de ASP.NET Core.

La solucion usa .NET 10, Entity Framework Core, SQL Server, JWT, BCrypt, OpenAPI, Scalar y pruebas de integracion. El informe formal de entrega se encuentra en `InformeTecnicoEntrega.md`; esta version resume las decisiones tecnicas y la forma de ejecutar el proyecto.

## 2. Flujo de negocio

```text
Cliente -> Solicitud -> Cotizacion -> Pedido
```

Una solicitud registra la necesidad del cliente. Luego puede convertirse en una cotizacion y, si se acepta, en un pedido para su ejecucion. El catalogo contiene productos y servicios; empleados y materiales se consultan para apoyar la operacion.

## 3. Arquitectura

```text
Cliente HTTP / Scalar / Postman
              |
       ASP.NET Core pipeline
              |
 Middleware, CORS y rate limiting
              |
      JWT y autorizacion por rol
              |
           Endpoints
              |
       EF Core o Repository
              |
        SQL Server real
```

`Program.cs` compone la aplicacion y registra el `DbContext`, `AuthService`, el repositorio de solicitudes, autenticacion, autorizacion, CORS, rate limiting, errores y OpenAPI.

Los endpoints se separan por dominio:

- `AuthApi.cs`: login, perfil y creacion de usuarios.
- `CatalogoApi.cs`: clientes, productos, empleados y materiales.
- `SolicitudApi.cs`: operaciones sobre solicitudes y validacion de relaciones.
- `CotizacionApi.cs`: cotizaciones y sus detalles.
- `PedidoApi.cs`: pedidos y sus relaciones.

Las entidades y el `DbContext` estan en `Models/`. `AuthService` concentra el hashing y la generacion de tokens. `ISolicitudRepository` y `SolicitudRepository` aislan el acceso a solicitudes. `RequestLoggingMiddleware` registra metodo, ruta, estado y duracion de cada peticion.

## 4. Persistencia

La base `LetrerosHerreradb` se creo con `Scripts/LetrerosHerrera.Database.sql`. El modelo C# se genero con enfoque Database-First y representa las tablas, relaciones, indices y restricciones existentes.

La cadena de conexion se obtiene desde `ConnectionStrings:DefaultConnection`. No se guarda en los archivos versionados: durante el desarrollo se configura con User Secrets o variables de entorno.

La aplicacion usa Entity Framework Core directamente para operaciones simples. El flujo de solicitudes usa un repositorio para demostrar la separacion entre la capa HTTP y el acceso a datos. La migracion `InitialCreate` se conserva como evidencia de EF Core, pero no se aplica automaticamente sobre la base real porque esta ya contiene datos.

## 5. Rutas y respuestas

Todas las rutas de negocio usan el prefijo `/api/v1`.

| Area | Ruta | Operaciones principales | Acceso |
| --- | --- | --- | --- |
| Autenticacion | `/api/v1/auth` | login, perfil, crear usuario | login publico; usuarios solo `dueno` |
| Clientes | `/api/v1/clientes` | GET, POST, PUT, DELETE | autenticado y segun rol |
| Productos | `/api/v1/productos` | GET, POST, PUT, DELETE | GET publico; escritura restringida |
| Solicitudes | `/api/v1/solicitudes` | GET, POST, PUT, DELETE | autenticado y segun rol |
| Cotizaciones | `/api/v1/cotizaciones` | GET, POST, PUT, DELETE | autenticado y segun rol |
| Pedidos | `/api/v1/pedidos` | GET, POST, PUT, DELETE | autenticado y segun rol |
| Empleados | `/api/v1/empleados` | GET | autenticado |
| Materiales | `/api/v1/materiales` | GET | autenticado |

La API usa `200 OK`, `201 Created`, `204 No Content`, `400` o `422` para datos invalidos, `401 Unauthorized` cuando falta o falla la autenticacion, `403 Forbidden` cuando el rol no tiene permiso, `404 Not Found`, `409 Conflict` y `429 Too Many Requests`.

OpenAPI se publica en `/openapi/v1.json` y Scalar en `/scalar` durante Development. El documento OpenAPI incluye el esquema Bearer para probar rutas protegidas.

## 6. Autenticacion y autorizacion

`POST /api/v1/auth/login` recibe email y password. La API busca un usuario activo, verifica el hash BCrypt y devuelve un JWT firmado. El token incluye identificador, nombre, email y rol. En cada ruta protegida se validan firma, issuer, audience y expiracion.

Los roles definidos por la API son:

- `dueno`: administra usuarios y operaciones de mayor privilegio.
- `disenador`: trabaja con solicitudes y cotizaciones.
- `operario`: trabaja con solicitudes, clientes y pedidos.
- `otro`: rol valido con permisos limitados.

Un token ausente, invalido o vencido produce `401`. Un token valido cuyo rol no tiene permiso produce `403`.

## 7. Seguridad

Las medidas principales son:

1. Passwords almacenadas con BCrypt, nunca en texto plano.
2. JWT firmado y validado con issuer, audience, expiracion y clave secreta.
3. Autorizacion por rol en cada operacion sensible.
4. User Secrets o variables de entorno para conexion SQL y configuracion JWT.
5. CORS limitado a `Cors:AllowedOrigin`.
6. Rate limiting de 10 intentos por minuto para login y 100 solicitudes por minuto para la API general.
7. `ProblemDetails` y `UseExceptionHandler` para respuestas de error controladas.
8. Validacion de datos y de relaciones antes de guardar.
9. Redireccion HTTPS.

Estas medidas se relacionan con las mitigaciones documentadas en `OWASP.md`, especialmente autenticacion, autorizacion, consumo de recursos, configuracion segura y control de inventario de la API.

GitHub almacena el codigo y su historial, pero no los secretos de desarrollo. `.gitignore` excluye `bin/`, `obj/`, `.vs/`, `.test-obj/` y archivos locales de secretos. Las credenciales incluidas en el script SQL son solo datos de laboratorio y no deben usarse en produccion.

## 8. Pruebas

`LetrerosHerreraSYNCTIMEapi.Tests` usa xUnit y `WebApplicationFactory`. Las pruebas verifican:

- disponibilidad de OpenAPI y del esquema Bearer;
- consulta publica de productos;
- rechazo `401` de clientes sin token;
- rechazo de un password incorrecto;
- login valido y acceso posterior a `/api/v1/auth/me`, cuando se configuran credenciales locales.

La prueba de login valido lee `TestCredentials:Email` y `TestCredentials:Password` desde User Secrets. Si no existen, esa prueba se omite y no se publica ninguna credencial en el codigo.

Desde la carpeta del proyecto se puede compilar y probar con:

```powershell
dotnet build .\LetrerosHerreraSYNCTIMEapi.slnx -c Release
dotnet test .\LetrerosHerreraSYNCTIMEapi.slnx -c Release
```

Para ejecutar la API en desarrollo, primero deben existir `ConnectionStrings:DefaultConnection`, `Jwt:Key`, `Jwt:Issuer` y `Jwt:Audience` en User Secrets o variables de entorno. Luego:

```powershell
dotnet run --project .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.csproj
```

## 9. Limitaciones y siguiente paso

La base actual es Database-First y contiene datos existentes; por eso las migraciones no se ejecutan automaticamente. Antes de aplicar cambios de esquema se debe trabajar con una copia de la base y revisar la migracion.

El repositorio contiene el codigo y la documentacion, pero GitHub no ejecuta la API por si solo. Para disponer de una URL publica se requiere publicar la aplicacion y configurar secretos en el servicio de alojamiento.
