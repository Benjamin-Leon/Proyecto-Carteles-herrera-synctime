# Guion para la exposicion

## Letreros Herrera SYNCTIME API

Este documento sirve como base para preparar las diapositivas y explicar el proyecto frente al docente. Presenta el caso de negocio, el problema original, la solucion implementada, las herramientas utilizadas, las medidas de seguridad, las pruebas y las limitaciones actuales.

## 1. Caso de estudio

Letreros Herrera es una tienda real de letreria que funciona principalmente mediante recomendaciones y contacto boca a boca. El negocio recibe solicitudes de clientes para fabricar letreros, productos personalizados y otros trabajos relacionados.

Antes de este proyecto, gran parte del proceso era manual:

- el cliente explicaba lo que necesitaba;
- la solicitud se anotaba de forma simple;
- los diseños se guardaban manualmente en un disco duro;
- el seguimiento dependia de la memoria y de registros dispersos;
- no existia un sistema central para relacionar clientes, productos, cotizaciones y pedidos.

El problema no era solamente guardar datos. Tambien era dificil saber en que estado estaba cada trabajo, quien lo atendia, que materiales podia requerir y como una solicitud terminaba convirtiendose en una cotizacion y luego en un pedido.

## 2. Propuesta del proyecto

El objetivo fue replicar el funcionamiento del rubro de manera virtual mediante una API REST. La API registra el proceso completo y relaciona la informacion que antes se manejaba de forma manual.

El flujo principal es:

```text
Cliente -> Solicitud -> Cotizacion -> Pedido -> Ejecucion
```

La solucion no reemplaza el trabajo fisico de fabricar los letreros. Organiza digitalmente la informacion necesaria para coordinarlo, consultar su estado y reducir la perdida de datos.

## 3. Que problemas resuelve

### Problema 1: informacion dispersa

Antes, los datos podian estar en anotaciones, conversaciones o un disco duro. Ahora se registran en una base SQL Server con entidades relacionadas.

### Problema 2: solicitudes poco detalladas

La API guarda cliente, producto o servicio, cantidades, observaciones, estado y relaciones necesarias para continuar el trabajo.

### Problema 3: falta de seguimiento

Una solicitud puede avanzar hacia una cotizacion y luego hacia un pedido. El sistema permite consultar esos registros y asociarlos.

### Problema 4: acceso sin control

No todas las personas deben crear usuarios, modificar productos o eliminar informacion. JWT y roles separan las operaciones segun el perfil.

### Problema 5: errores por datos incompletos

Antes de guardar se validan campos, cantidades, estados, roles y relaciones como cliente, empleado, producto o cotizacion.

### Problema 6: dificultad para repetir el proceso

OpenAPI, Scalar, el archivo `.http` y las pruebas automatizadas permiten documentar y repetir las operaciones principales.

## 4. Modelo de datos

La base `LetrerosHerreradb` contiene las entidades principales del negocio:

- clientes;
- empleados;
- usuarios;
- productos y servicios;
- materiales;
- proveedores;
- solicitudes;
- cotizaciones y detalles de cotizacion;
- pedidos;
- fechas comprometidas;
- horarios de atencion;
- movimientos de inventario;
- notificaciones.

Las relaciones mas importantes del flujo son:

```text
Cliente 1 ---- N Solicitudes
Solicitud 1 -- N Cotizaciones
Cotizacion 1 - N Detalles
Cotizacion ---- Pedido
Empleado 1 --- N Pedidos
Producto ----- N Solicitudes
Material ----- N Movimientos de inventario
```

La API no trata cada tabla como un dato aislado. Las relaciones se validan antes de guardar para evitar solicitudes con clientes inexistentes o pedidos sin referencias validas.

## 5. Arquitectura utilizada

El proyecto usa ASP.NET Core Minimal API sobre .NET 10.

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

### Herramientas y tecnologias

- **C# y .NET 10:** lenguaje y plataforma del backend.
- **ASP.NET Core Minimal API:** crea endpoints sin controladores MVC.
- **Entity Framework Core:** ORM que traduce consultas C# a SQL Server.
- **SQL Server:** almacenamiento persistente de la informacion.
- **JWT Bearer:** autenticacion mediante tokens.
- **BCrypt:** hashing de contrasenas.
- **OpenAPI y Scalar:** documentacion y prueba manual de endpoints.
- **xUnit y WebApplicationFactory:** pruebas de integracion.
- **Git y GitHub:** versionamiento y entrega del codigo.
- **User Secrets:** almacenamiento local de secretos fuera del repositorio.

## 6. Responsabilidad de cada parte

### `Program.cs`

Es el punto de composicion. Registra la base de datos, autenticacion, autorizacion, servicios, repositorio, CORS, rate limiting, manejo de errores y OpenAPI. Tambien activa el pipeline y registra los grupos de endpoints.

### `Endpoints/`

Es la capa HTTP:

- `AuthApi.cs`: login, perfil y creacion de usuarios.
- `CatalogoApi.cs`: clientes, productos, empleados y materiales.
- `SolicitudApi.cs`: solicitudes y validacion de relaciones.
- `CotizacionApi.cs`: cotizaciones y sus detalles.
- `PedidoApi.cs`: pedidos y sus relaciones.

### `Models/`

Contiene las entidades y el `DbContext` generado desde la base existente. Representa tablas, claves, relaciones e indices.

### `Services/AuthService.cs`

Centraliza el hashing BCrypt, la verificacion de credenciales y la generacion de JWT.

### `Repositories/`

Contiene `ISolicitudRepository` y `SolicitudRepository`. El Repository separa la logica del endpoint del acceso especifico a las solicitudes.

### `Middleware/`

`RequestLoggingMiddleware` registra metodo, ruta, codigo de respuesta y duracion de cada request.

### `Migrations/`

Contiene una migracion inicial como evidencia de EF Core. La base existente fue creada mediante SQL y no se modifica automaticamente para proteger sus datos.

## 7. Endpoints principales

Todas las rutas de negocio utilizan el prefijo `/api/v1`.

| Area | Ruta | Funcion |
| --- | --- | --- |
| Auth | `/api/v1/auth/login` | valida credenciales y devuelve JWT |
| Auth | `/api/v1/auth/me` | muestra el usuario autenticado |
| Clientes | `/api/v1/clientes` | administra clientes |
| Productos | `/api/v1/productos` | consulta y administra catalogo |
| Solicitudes | `/api/v1/solicitudes` | registra necesidades de clientes |
| Cotizaciones | `/api/v1/cotizaciones` | registra propuestas y detalles |
| Pedidos | `/api/v1/pedidos` | registra trabajos a ejecutar |
| Empleados | `/api/v1/empleados` | consulta personal autorizado |
| Materiales | `/api/v1/materiales` | consulta materiales disponibles |

Codigos importantes:

- `200`: operacion exitosa;
- `201`: recurso creado;
- `204`: eliminado o actualizado sin contenido;
- `400` o `422`: datos invalidos;
- `401`: falta autenticacion o credenciales incorrectas;
- `403`: el usuario no tiene el rol requerido;
- `404`: recurso o relacion inexistente;
- `409`: conflicto o duplicado;
- `429`: limite de peticiones superado.

## 8. Login, JWT y roles

El flujo de autenticacion es:

1. El cliente envia email y password a `/api/v1/auth/login`.
2. La API busca un usuario activo.
3. `AuthService` compara la password con el hash BCrypt.
4. Si falla, responde `401` sin revelar si fallo el email o la password.
5. Si funciona, genera un JWT firmado.
6. El cliente envia el token en `Authorization: Bearer TOKEN`.
7. ASP.NET Core valida firma, issuer, audience y expiracion.
8. La autorizacion revisa el rol antes de permitir la operacion.

Roles utilizados:

- `dueno`: mayor privilegio, incluyendo crear usuarios y eliminar recursos.
- `disenador`: trabaja principalmente con solicitudes y cotizaciones.
- `operario`: trabaja con clientes, solicitudes y pedidos.
- `otro`: rol valido con permisos limitados.

La diferencia entre `401` y `403` es importante:

- `401`: no hay una identidad valida.
- `403`: la identidad es valida, pero el rol no alcanza.

## 9. Seguridad y OWASP

El proyecto documenta siete areas del OWASP API Security Top 10 en `OWASP.md`:

1. **API1 - Broken Object Level Authorization:** autenticacion, roles y validacion de identificadores relacionados.
2. **API2 - Broken Authentication:** BCrypt, JWT, expiracion y respuestas genericas de login.
3. **API3 - Broken Object Property Level Authorization:** records de entrada y campos permitidos.
4. **API4 - Unrestricted Resource Consumption:** rate limiting de login y API general.
5. **API5 - Broken Function Level Authorization:** permisos diferentes por endpoint y rol.
6. **API8 - Security Misconfiguration:** secretos fuera de GitHub y CORS limitado.
7. **API9 - Improper Inventory Management:** version `/api/v1`, OpenAPI y archivo `.http`.

Protecciones adicionales:

- HTTPS mediante `UseHttpsRedirection`.
- `ProblemDetails` y `UseExceptionHandler` para errores controlados.
- validacion de relaciones antes de persistir.
- secretos en User Secrets o variables de entorno.
- carpetas generadas excluidas mediante `.gitignore`.

## 10. Pruebas

La suite usa xUnit y `WebApplicationFactory` para llamar la API mediante HTTP.

Pruebas implementadas:

- OpenAPI responde `200` y contiene Bearer.
- Productos se pueden consultar sin autenticacion.
- Clientes responde `401` sin token.
- Login con password incorrecta responde `401`.
- Login valido genera un JWT y permite consultar `/api/v1/auth/me`, cuando se configuran credenciales locales.

Comando:

```powershell
dotnet test .\LetrerosHerreraSYNCTIMEapi.slnx -c Release
```

La prueba de login valido lee las credenciales desde User Secrets y no las guarda en el codigo.

## 11. Versionamiento y documentacion

Las rutas utilizan `/api/v1` para identificar la version de la API. Esto permite agregar una futura `/api/v2` sin romper de inmediato a los clientes existentes.

En Development se puede consultar:

- OpenAPI: `/openapi/v1.json`.
- Scalar: `/scalar`.
- Requests repetibles: `LetrerosHerreraSYNCTIMEapi.http`.

GitHub almacena el codigo, la documentacion y el historial. No almacena la conexion SQL ni la clave JWT porque esas configuraciones se manejan localmente con User Secrets o variables de entorno.

## 12. Que se cumple y que queda como limite

### Cubierto por el proyecto

- caso de negocio realista;
- Minimal API y arquitectura por responsabilidades;
- middleware propio;
- inyeccion de dependencias;
- CRUD de recursos principales;
- validaciones y codigos HTTP;
- ORM con EF Core y SQL Server;
- Repository para solicitudes;
- versionamiento por URL;
- OpenAPI y Scalar;
- JWT y cuatro roles;
- siete mitigaciones OWASP documentadas;
- pruebas de integracion;
- informe tecnico y README.

### Limites que se deben explicar con honestidad

- La migracion inicial existe, pero no se aplica automaticamente sobre la base real porque contiene datos.
- No todas las tablas de la base tienen CRUD publico; algunas estan mapeadas para futuras ampliaciones.
- GitHub guarda el proyecto, pero no ejecuta la API ni reemplaza SQL Server.
- La evidencia de validacion del caso por el docente y la evidencia de pruebas deben presentarse aparte si fueron solicitadas.
- La exposicion y las diapositivas son parte de la entrega y deben prepararse con este material.

## 13. Orden recomendado para las diapositivas

1. Portada: Letreros Herrera SYNCTIME API.
2. Problema original del negocio.
3. Objetivo y solucion propuesta.
4. Flujo Cliente -> Solicitud -> Cotizacion -> Pedido.
5. Modelo de datos y tablas principales.
6. Arquitectura de la API.
7. Endpoints implementados.
8. Login, JWT y roles.
9. Seguridad y OWASP.
10. Pruebas y evidencia de ejecucion.
11. GitHub, secretos y versionamiento.
12. Limites, mejoras futuras y conclusion.

## 14. Texto breve para pedirle diapositivas a otra IA

```text
Genera una presentacion academica de 10 a 12 diapositivas sobre el proyecto "Letreros Herrera SYNCTIME API". El caso es una tienda de letreria que funciona principalmente por boca a boca y antes registraba solicitudes manualmente, guardaba disenos en un disco duro y no tenia un sistema centralizado. El objetivo es digitalizar el flujo Cliente -> Solicitud -> Cotizacion -> Pedido mediante una API REST.

Explica: problema del negocio, objetivo, modelo de datos SQL Server, arquitectura ASP.NET Core Minimal API .NET 10, Entity Framework Core, endpoints CRUD, Repository, middleware de logging, JWT, BCrypt, roles dueno/disenador/operario/otro, OpenAPI y Scalar, pruebas xUnit con WebApplicationFactory, GitHub y User Secrets. Incluye una diapositiva que relacione siete mitigaciones OWASP API Security con la implementacion. Menciona honestamente que la migracion existe pero no se aplica automaticamente a la base con datos, que algunas tablas quedan para futuras ampliaciones y que GitHub no ejecuta la API. Usa lenguaje claro para una exposicion oral, poco texto por diapositiva y notas del presentador con una explicacion mas completa.
```

## 15. Frase de cierre

“Letreros Herrera SYNCTIME API transforma un proceso manual y disperso en un flujo digital trazable: registra clientes, solicitudes, cotizaciones y pedidos, protege las operaciones con JWT y roles, valida la informacion antes de guardarla y deja una base preparada para seguir creciendo.”