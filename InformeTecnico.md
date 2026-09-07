# Informe tecnico - LetrerosHerreraSYNCTIMEapi

## Como leer el proyecto

El proyecto conserva el estilo de `GamesApi`: `Program.cs` configura la aplicacion y cada archivo de `Endpoints/` agrupa las rutas de un area del negocio. La diferencia principal es que este proyecto agrega las capas necesarias para un sistema real: persistencia con una base existente, reglas de autorizacion, validaciones, middleware, pruebas y documentacion.

El recorrido general de una peticion es:

1. ASP.NET Core recibe la peticion.
2. El middleware registra la solicitud y el manejador de errores protege la respuesta.
3. CORS y rate limiting aplican controles previos.
4. JWT identifica al usuario y Authorization verifica sus permisos.
5. El endpoint valida la entrada y ejecuta la operacion de negocio.
6. Entity Framework Core o el repositorio consulta o modifica `LetrerosHerreradb`.
7. El endpoint devuelve un codigo HTTP y una respuesta JSON.

## Caso de negocio

La API administra el flujo de una empresa de letreros: clientes, productos y servicios, solicitudes, cotizaciones y pedidos. La base de datos utilizada es `LetrerosHerreradb`.

## Arquitectura

- ASP.NET Core Minimal API sobre .NET 10.
- Endpoints organizados por dominio en `Endpoints/`.
- Entity Framework Core SQL Server con el modelo generado desde la base existente.
- Inyeccion de dependencias: `DbContext`, `AuthService` y `ISolicitudRepository` registrados como servicios `Scoped`.
- Middleware propio para registrar metodo, ruta, estado y duracion de cada request.
- Respuestas de error centralizadas mediante `AddProblemDetails` y `UseExceptionHandler`.

### Responsabilidad de cada sector

- `Program.cs`: punto de composicion. Registra servicios, configura seguridad y ordena el pipeline.
- `Endpoints/`: capa HTTP. Define rutas, verbos, DTOs de entrada, validaciones y codigos de estado.
- `Models/`: entidades y `DbContext` generados a partir de la base SQL existente.
- `Services/`: logica transversal reutilizable, especialmente hashing y generacion de JWT.
- `Repositories/`: contrato y acceso aislado para solicitudes, demostrando el patron Repository.
- `Middleware/`: comportamiento comun para todas las peticiones, como logging y manejo de errores.
- `LetrerosHerreraSYNCTIMEapi.Tests/`: verifica el contrato HTTP sin depender de una interfaz grafica.

## Versionamiento y documentacion

Las rutas publicas de negocio usan el prefijo `/api/v1`. OpenAPI se expone en `/openapi/v1.json` y Scalar en `/scalar` durante Development. El transformador OpenAPI registra el esquema Bearer para que el token JWT pueda probarse desde Scalar.

La documentacion practica de requests se encuentra en el archivo `.http`, que incluye login, OpenAPI, catalogo publico, una prueba protegida sin token y una prueba autenticada.

## Persistencia

El `LetrerosHerreradbContext` mapea las tablas existentes, claves, indices, restricciones y relaciones de `LetrerosHerreradb`. El repositorio `ISolicitudRepository` aísla el acceso a solicitudes; los demas endpoints usan el contexto directamente por tratarse de operaciones simples del catalogo.

## Seguridad

- Login con BCrypt y JWT firmado.
- Secretos fuera del repositorio mediante User Secrets o variables de entorno.
- Autorizacion por roles: `dueno`, `disenador` y `operario`.
- CORS limitado a `Cors:AllowedOrigin`.
- Rate limiting: 10 intentos por minuto para login y 100 requests por minuto para la API general.
- HTTPS redirigido por middleware.
- Validacion de relaciones y rangos antes de persistir.

### GitHub, secretos y datos de prueba

GitHub es el sitio donde se guarda el historial del codigo mediante Git. Si este proyecto se publica, cualquier valor escrito en un archivo versionado puede quedar visible y tambien puede permanecer en el historial aunque despues se borre.

Por eso la conexion SQL Server y la clave JWT se guardan en `dotnet user-secrets` durante el desarrollo. `appsettings.json` y `appsettings.Development.json` no contienen esos valores. El archivo `.gitignore` excluye `bin/`, `obj/`, `.vs/` y archivos locales de secretos.

El script de base de datos contiene usuarios demo para poder probar la aplicacion. Sus credenciales son de laboratorio y no deben reutilizarse en produccion. El archivo `.http` deja la password como marcador para evitar publicar una credencial escrita.

El endpoint de login no revela si fallo el email o la contrasena: ambos casos responden `401`. Los endpoints de lectura y escritura se separan mediante grupos y roles; por ejemplo, consultar clientes requiere autenticacion, crear clientes permite `dueno` u `operario`, y eliminar clientes queda reservado a `dueno`.

## Pruebas

El proyecto `LetrerosHerreraSYNCTIMEapi.Tests` usa xUnit y `WebApplicationFactory`. Comprueba OpenAPI, catalogo publico, respuesta 401 sin autenticacion, login invalido, login valido y acceso autenticado.

Comando de ejecucion:

```powershell
dotnet test .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.Tests\LetrerosHerreraSYNCTIMEapi.Tests.csproj -c Release
```

Para ejecutar tambien la prueba de login valido, configurar localmente las credenciales demo sin escribirlas en el codigo:

```powershell
dotnet user-secrets set "TestCredentials:Email" "<email-demo>" --project .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.Tests\LetrerosHerreraSYNCTIMEapi.Tests.csproj
dotnet user-secrets set "TestCredentials:Password" "<password-demo>" --project .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.Tests\LetrerosHerreraSYNCTIMEapi.Tests.csproj
```

Si no se configuran, la prueba de login valido se omite; las demas pruebas siguen ejecutandose.

Las pruebas no modifican datos: solo consultan la base y validan el contrato HTTP.

La suite funciona como una demostracion automatizada del flujo central: comprueba que OpenAPI esta disponible y contiene Bearer, que productos es publico, que clientes rechaza peticiones sin token, que un password incorrecto devuelve `401` y que un login valido permite consultar `/auth/me`.

## Base de datos y migraciones

La base se creo inicialmente mediante `Scripts/LetrerosHerrera.Database.sql` y el modelo fue generado con enfoque Database-First. El script define clientes, empleados, proveedores, catalogo, usuarios y el flujo de solicitudes, cotizaciones, pedidos e inventario.

La migracion `InitialCreate` deja evidencia de EF Core Migrations, pero no se ejecuto automaticamente sobre la base actual para no alterar datos existentes. En un entorno de desarrollo nuevo se puede comparar y aplicar contra una copia de `LetrerosHerreradb`.

## Como explicar el proyecto en la exposicion

La idea central es que `GamesApi` entrega el patron basico de Minimal API y este proyecto lo aplica a un caso de negocio mas completo. `CatalogoApi` administra informacion base; `SolicitudApi`, `CotizacionApi` y `PedidoApi` representan el flujo comercial; `AuthApi` controla identidad y usuarios. El resto de las capas evita repetir reglas y permite probar el sistema de forma reproducible.

## Guia simple para entender cada carpeta

### `Program.cs`

Es la puerta de entrada. Aqui no se escribe todo el negocio: se conectan las piezas.

1. Lee la conexion a SQL Server y registra el `DbContext`.
2. Lee la configuracion JWT y prepara la validacion de tokens.
3. Registra servicios que los endpoints necesitaran.
4. Configura CORS, rate limiting, errores y OpenAPI.
5. Activa el pipeline en el orden correcto.
6. Llama a cada `Map...Api()` para registrar las rutas.

### `Endpoints/`

Son las puertas de entrada HTTP. Cada archivo representa una parte del negocio:

- `AuthApi.cs`: login, perfil del usuario y creacion de usuarios.
- `CatalogoApi.cs`: clientes, productos, empleados y materiales.
- `SolicitudApi.cs`: recibe la necesidad del cliente y valida sus referencias.
- `CotizacionApi.cs`: arma una propuesta con productos, cantidades y total.
- `PedidoApi.cs`: convierte el flujo comercial en un trabajo que se ejecutara.

Todos usan el mismo patron: reciben JSON, validan, consultan la base, guardan si corresponde y devuelven `200`, `201`, `204`, `400`, `401`, `403`, `404` o `409` segun el caso.

### `Models/`

Aqui estan las clases que representan las tablas de `LetrerosHerreradb`. `LetrerosHerreradbContext` es el traductor entre C# y SQL Server: permite escribir consultas LINQ y que EF Core genere SQL.

### `Services/`

Contiene logica reutilizable que no deberia repetirse dentro de los endpoints. `AuthService` hashea contrasenas y genera JWT con identidad, email y rol.

### `Repositories/`

Es una capa intermedia. El endpoint pide “dame las solicitudes” o “guarda esta solicitud”, y el repository decide como hacerlo usando EF Core. Esto facilita cambiar o probar el acceso a datos.

### `Middleware/`

Es codigo que pasa por todas las peticiones. `RequestLoggingMiddleware` mide cuanto demora cada request y registra el resultado, incluso cuando hay un error.

### `OpenApi/`

Contiene el transformador que le explica a OpenAPI que existe autenticacion Bearer JWT. No cambia la seguridad real; solo mejora la documentacion y la prueba desde Scalar.

### `Migrations/`

Guarda el historial de cambios que EF Core podria aplicar al esquema. La base actual se creo primero con SQL, por eso la migracion se conserva como evidencia y no se aplico automaticamente sobre datos reales.

### `LetrerosHerreraSYNCTIMEapi.Tests/`

Es la carpeta de pruebas. Levanta la aplicacion en memoria con `WebApplicationFactory` y llama endpoints reales. Comprueba que el sistema responda como contrato HTTP, incluyendo login, JWT, rutas publicas y rutas protegidas.

## Ejemplo explicado de una peticion

Cuando alguien llama `POST /api/v1/auth/login`, la peticion pasa por el pipeline, llega a `AuthApi`, busca el usuario en `Usuarios`, compara la contrasena con BCrypt y, si todo coincide, `AuthService` crea un JWT. Luego el cliente envia ese token en `Authorization: Bearer ...` para llamar, por ejemplo, a `GET /api/v1/clientes/`. ASP.NET Core lee el token, identifica el rol y el endpoint decide si responde o devuelve `401`/`403`.

Cuando alguien crea una solicitud, `SolicitudApi` valida que el cliente y producto existan, usa `ISolicitudRepository` para guardar y devuelve `201 Created`. Esa solicitud puede alimentar una cotizacion y luego un pedido.

## Frase corta para la defensa

“El proyecto sigue el estilo Minimal API de GamesApi, pero separa el sistema por responsabilidades: `Program` configura, `Endpoints` reciben peticiones, `Services` resuelven logica reutilizable, `Models` representan la base, `Repositories` aíslan consultas, `Middleware` controla todas las peticiones y `Tests` comprueban que el contrato funcione.”

## Comandos utiles

Desde la carpeta del workspace:

```powershell
dotnet build .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.slnx -c Release
dotnet test .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.slnx -c Release
dotnet ef migrations list --project .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.csproj
```
