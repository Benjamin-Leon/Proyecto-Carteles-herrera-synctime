# Informe tecnico

## LetrerosHerreraSYNCTIMEapi

**Asignatura:** Lenguaje de Programacion Web II  
**Unidad:** UT1 - Diseno e implementacion de APIs RESTful  
**Base de datos:** `LetrerosHerreradb`  
**Tecnologia:** ASP.NET Core Minimal API, .NET 10 y Entity Framework Core  
**Fecha:** 07-09-2026

---

## 1. Resumen del proyecto

`LetrerosHerreraSYNCTIMEapi` es una API REST para apoyar la gestion de una empresa de letreros. El sistema organiza el flujo principal del negocio:

1. Se registra o consulta un cliente.
2. Se consulta un producto o servicio disponible.
3. Se crea una solicitud del cliente.
4. Un usuario autorizado prepara una cotizacion.
5. La cotizacion puede convertirse en un pedido.
6. El personal autorizado consulta y actualiza el trabajo.

La API utiliza Minimal API, siguiendo el estilo estudiado en `GamesApi`. La diferencia es que el dominio cambia de videojuegos a letreros y se incorporan controles adicionales solicitados por la rubrica: autenticacion, roles, validaciones, middleware, pruebas, documentacion y medidas de seguridad.

---

## 2. Objetivos

### Objetivo general

Construir una API REST funcional que permita administrar el flujo comercial de una empresa de letreros, conectada a una base SQL Server real y protegida mediante autenticacion JWT y autorizacion por roles.

### Objetivos especificos

- Aplicar Minimal API sin controladores MVC.
- Conectar Entity Framework Core con `LetrerosHerreradb`.
- Exponer operaciones CRUD para clientes, productos, solicitudes, cotizaciones y pedidos.
- Validar datos y relaciones antes de guardar.
- Implementar login con contrasenas hasheadas mediante BCrypt.
- Generar tokens JWT con identidad y rol.
- Restringir operaciones segun el rol del usuario.
- Documentar la API con OpenAPI y Scalar.
- Agregar middleware propio y pruebas de integracion.
- Mantener secretos fuera de los archivos versionables.

---

## 3. Caso de negocio

La empresa recibe solicitudes para fabricar letreros y otros productos o servicios relacionados. Un cliente puede solicitar un trabajo; esa solicitud puede ser evaluada por un disenador o dueno mediante una cotizacion; finalmente, el trabajo puede transformarse en un pedido que sera gestionado por el personal operativo.

La base de datos contiene entidades para clientes, empleados, proveedores, productos, materiales, usuarios, solicitudes, cotizaciones, pedidos y seguimiento. La API implementa el flujo principal y deja disponibles consultas operativas de empleados y materiales.

---

## 4. Arquitectura general

El proyecto usa una arquitectura simple por responsabilidades:

```text
Cliente HTTP / Scalar / Postman
              |
              v
        ASP.NET Core pipeline
              |
     Middleware, CORS, rate limit
              |
       JWT y autorizacion por rol
              |
           Endpoints
              |
       EF Core o Repository
              |
        SQL Server real
```

### Flujo de una peticion

1. La aplicacion recibe una peticion HTTP.
2. El middleware registra metodo, ruta, estado y duracion.
3. CORS revisa el origen permitido y rate limiting controla el volumen.
4. Authentication valida el JWT cuando la ruta es protegida.
5. Authorization revisa si el usuario tiene el rol necesario.
6. El endpoint valida el JSON y sus relaciones.
7. EF Core o el Repository consulta o modifica la base.
8. Se devuelve una respuesta JSON con un codigo HTTP apropiado.

---

## 5. Estructura de carpetas

### `Program.cs`

Es el punto de entrada y configuracion de la aplicacion. Registra el `DbContext`, JWT, autorizacion, servicios, CORS, rate limiting, errores y OpenAPI. Tambien define el orden del pipeline y registra cada grupo de endpoints mediante metodos `Map...Api()`.

### `Endpoints/`

Es la capa HTTP. Cada archivo agrupa las rutas de un area del negocio.

- `AuthApi.cs`: login, perfil y creacion de usuarios.
- `CatalogoApi.cs`: clientes, productos, empleados y materiales.
- `SolicitudApi.cs`: CRUD de solicitudes y validacion de sus relaciones.
- `CotizacionApi.cs`: CRUD de cotizaciones y sus detalles.
- `PedidoApi.cs`: CRUD de pedidos y validacion de relaciones.

### `Models/`

Contiene las entidades que representan las tablas y relaciones de la base. `LetrerosHerreradbContext.cs` es el `DbContext`: traduce consultas LINQ de C# a consultas SQL Server y conoce las claves foraneas, indices, columnas y restricciones.

### `Services/`

Contiene logica reutilizable. `AuthService.cs` hashea contrasenas, verifica credenciales y genera JWT con los claims de usuario, email e identidad del rol.

### `Repositories/`

Contiene el contrato `ISolicitudRepository` y su implementacion `SolicitudRepository`. Esta capa concentra el acceso a solicitudes para que el endpoint no tenga que conocer todos los detalles de las consultas al `DbContext`.

### `Middleware/`

Contiene codigo que se ejecuta alrededor de las peticiones. `RequestLoggingMiddleware` mide el tiempo y registra el resultado de cada solicitud.

### `OpenApi/`

Contiene el transformador que registra el esquema Bearer JWT en el documento OpenAPI para que Scalar pueda documentar y probar la autenticacion.

### `Migrations/`

Contiene una migracion inicial generada desde el modelo actual. La base original fue creada mediante SQL y ya contiene datos; por eso la migracion se genero como evidencia y no se aplico automaticamente sobre la base real.

### `LetrerosHerreraSYNCTIMEapi.Tests/`

Contiene las pruebas automatizadas con xUnit y `WebApplicationFactory`. Las pruebas levantan la aplicacion y comprueban respuestas HTTP reales.

### `Scripts/`

Contiene el script SQL de creacion y datos iniciales de la base. Este script se utiliza para preparar el ambiente de desarrollo.

---

## 6. Base de datos y acceso a datos

La base se creo inicialmente con `Scripts/LetrerosHerrera.Database.sql`. El modelo C# fue generado mediante un enfoque Database-First. Esto significa que primero existia la base SQL y luego se generaron las entidades y el `DbContext`.

El `DbContext` se registra por inyeccion de dependencias:

```csharp
builder.Services.AddDbContext<LetrerosHerreradbContext>(options =>
    options.UseSqlServer(connectionString));
```

La cadena de conexion no se guarda en `appsettings.json`. Se carga desde User Secrets o variables de entorno.

El Repository se aplica al flujo de solicitudes. Los otros endpoints usan directamente el `DbContext` porque realizan operaciones simples sobre recursos concretos. Esta es una decision de alcance del proyecto y no se presenta como un Repository completo para todas las tablas.

---

## 7. Endpoints implementados

Todas las rutas de negocio usan el prefijo `/api/v1`.

| Area | Ruta principal | Operaciones | Proteccion |
| --- | --- | --- | --- |
| Autenticacion | `/api/v1/auth` | login, perfil, crear usuario | login publico; crear usuario solo `dueno` |
| Clientes | `/api/v1/clientes` | GET, POST, PUT, DELETE | lectura autenticada; escritura por rol |
| Productos | `/api/v1/productos` | GET, POST, PUT, DELETE | GET publico; escritura solo `dueno` |
| Solicitudes | `/api/v1/solicitudes` | GET, POST, PUT, DELETE | autenticacion y roles |
| Cotizaciones | `/api/v1/cotizaciones` | GET, POST, PUT, DELETE | autenticacion y roles |
| Pedidos | `/api/v1/pedidos` | GET, POST, PUT, DELETE | autenticacion y roles |
| Empleados | `/api/v1/empleados` | GET | autenticado |
| Materiales | `/api/v1/materiales` | GET | autenticado |

### Codigos HTTP usados

- `200 OK`: consulta o actualizacion exitosa.
- `201 Created`: recurso creado correctamente.
- `204 No Content`: eliminacion exitosa.
- `400` o `422`: datos invalidos, segun la respuesta de validacion.
- `401 Unauthorized`: falta el token o las credenciales no son validas.
- `403 Forbidden`: el token es valido, pero el rol no tiene permiso.
- `404 Not Found`: no existe el recurso relacionado.
- `409 Conflict`: existe un registro duplicado o incompatible.
- `429 Too Many Requests`: se supero el limite de peticiones.

---

## 8. Autenticacion y autorizacion

El login recibe email y password. El endpoint busca un usuario activo y verifica la password contra el hash BCrypt almacenado. Si es correcta, `AuthService` genera un JWT firmado.

El token incluye:

- identificador del usuario;
- nombre;
- email;
- rol.

En cada ruta protegida, ASP.NET Core valida firma, issuer, audience y expiracion. Luego las reglas `[Authorize(Roles = "...")]` comparan el rol del token con el rol permitido.

Roles utilizados:

- `dueno`: administra usuarios y puede eliminar recursos.
- `disenador`: puede trabajar con solicitudes y cotizaciones.
- `operario`: puede trabajar con solicitudes, clientes y pedidos.
- `otro`: existe como rol valido, pero tiene permisos mas limitados.

---

## 9. Seguridad y OWASP

Las medidas implementadas y documentadas en `OWASP.md` son:

1. **API1 - Broken Object Level Authorization:** se exige autenticacion y se validan identificadores relacionados.
2. **API2 - Broken Authentication:** BCrypt, JWT, expiracion y respuestas genericas `401`.
3. **API3 - Broken Object Property Level Authorization:** se usan records de entrada y se validan campos permitidos.
4. **API4 - Unrestricted Resource Consumption:** rate limiting para login y API general.
5. **API5 - Broken Function Level Authorization:** permisos diferentes por endpoint y rol.
6. **API8 - Security Misconfiguration:** secretos fuera del repositorio y CORS restringido.
7. **API9 - Improper Inventory Management:** versionamiento `/api/v1`, OpenAPI y archivo `.http`.

Tambien se utiliza redireccion HTTPS y validacion de relaciones antes de guardar para evitar errores poco claros de base de datos.

---

## 10. Secretos y GitHub

GitHub almacena el codigo y su historial. Por eso no se deben subir claves JWT ni contrasenas SQL.

En este proyecto:

- `appsettings.json` no contiene la cadena de conexion ni la clave JWT.
- User Secrets contiene la configuracion local.
- `.gitignore` excluye `bin/`, `obj/`, `.vs/` y archivos locales de secretos.
- El archivo `.http` usa marcadores en lugar de passwords.
- Las credenciales demo del script son solo para laboratorio y no deben reutilizarse en produccion.

Antes de publicar el repositorio se debe revisar el historial Git. Si alguna clave real fue subida anteriormente, no basta con borrarla del archivo: se debe cambiar la clave y limpiar el historial si corresponde.

---

## 11. Pruebas de integracion

El proyecto de pruebas verifica:

- OpenAPI responde `200` y contiene el esquema Bearer.
- Productos se pueden consultar sin autenticacion.
- Clientes devuelve `401` sin token.
- Un password incorrecto devuelve `401`.
- Cuando se configuran credenciales locales, login devuelve un JWT y `/auth/me` responde `200`.

Comando:

```powershell
dotnet test .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.slnx -c Release
```

Las pruebas no crean, modifican ni eliminan registros de negocio.

---

## 12. Documentacion y ejecucion

En Development se dispone de:

- OpenAPI: `/openapi/v1.json`.
- Scalar: `/scalar`.
- Archivo de requests: `LetrerosHerreraSYNCTIMEapi.http`.

Para ejecutar la API se requieren los User Secrets locales con:

- `ConnectionStrings:DefaultConnection`.
- `Jwt:Key`.
- `Jwt:Issuer`.
- `Jwt:Audience`.
- `Jwt:ExpirationMinutes`.

No se incluyen los valores en este informe ni en el repositorio.

---

## 13. Alcance y limites actuales

El alcance implementado cubre el flujo principal de clientes, catalogo, solicitudes, cotizaciones y pedidos. La base tambien posee tablas de proveedores, inventario, fechas comprometidas, horarios, movimientos y notificaciones; esas tablas estan mapeadas en el modelo, pero no todas tienen CRUD publico en esta version.

La migracion inicial esta generada, pero no fue aplicada sobre la base existente. Esto evita alterar los datos reales del laboratorio. Para una base nueva se debe revisar y aplicar en un ambiente de desarrollo separado.

Estas limitaciones se dejan explicitas para que el informe represente el estado real del sistema.

---

## 14. Conclusion

El proyecto aplica el patron de `GamesApi` a un caso de negocio distinto y agrega los elementos exigidos por el Trabajo 1: Minimal API, ORM, CRUD, validacion, versionamiento, OpenAPI, JWT, roles, middleware, Repository, seguridad OWASP y pruebas automatizadas.

La aplicacion queda organizada para que cada integrante pueda explicar que hace cada carpeta y como una peticion avanza desde HTTP hasta SQL Server.
