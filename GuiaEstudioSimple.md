# Guia simple para estudiar

## LetrerosHerreraSYNCTIMEapi explicado desde cero

Este documento es para estudiar. El informe formal es `InformeTecnicoEntrega.md`.

---

## 1. Que estamos construyendo

Estamos construyendo una API. Una API es un programa que recibe peticiones HTTP y devuelve respuestas.

Ejemplo:

```text
GET /api/v1/productos/
```

La API recibe esa peticion, busca productos en SQL Server y devuelve JSON.

Nuestro negocio es una empresa de letreros. El flujo es:

```text
Cliente -> Solicitud -> Cotizacion -> Pedido
```

Un cliente pide un trabajo. Se revisa la solicitud. Se calcula una cotizacion. Si se acepta, se crea un pedido para ejecutar el trabajo.

---

## 2. De donde salen los datos

Los datos estan en SQL Server, en la base `LetrerosHerreradb`.

El script esta en:

```text
Scripts/LetrerosHerrera.Database.sql
```

Ese script crea tablas como:

- clientes;
- empleados;
- proveedores;
- productos_servicios;
- materiales;
- usuarios;
- solicitudes;
- cotizaciones;
- cotizacion_detalle;
- pedidos;
- tablas de inventario y seguimiento.

El programa no inventa esos datos en memoria. Se conecta a la base real usando Entity Framework Core.

---

## 3. Como llega una tabla a C Sharp

Entity Framework Core usa una clase llamada `DbContext` para representar la base.

El archivo principal es:

```text
LetrerosHerreraSYNCTIMEapi/Models/LetrerosHerreradbContext.cs
```

El `DbContext` tiene propiedades como:

```csharp
public virtual DbSet<Cliente> Clientes { get; set; }
public virtual DbSet<Pedido> Pedidos { get; set; }
```

Eso significa que podemos consultar la tabla desde C#:

```csharp
db.Clientes.ToListAsync()
```

EF Core transforma esa consulta en SQL y devuelve objetos C#.

---

## 4. Para que sirve cada carpeta

### `Program.cs`

Es donde arranca todo. No contiene todo el negocio; conecta las piezas.

Hace estas cosas:

1. Lee la conexion a SQL Server.
2. Registra el `DbContext`.
3. Configura JWT.
4. Configura autorizacion por roles.
5. Registra `AuthService` y Repository.
6. Configura errores, CORS, rate limiting y OpenAPI.
7. Activa el pipeline.
8. Registra los endpoints.

Cuando veas esto:

```csharp
app.MapAuthApi();
app.MapCatalogoApi();
app.MapSolicitudApi();
app.MapCotizacionApi();
app.MapPedidoApi();
```

significa que se estan conectando las rutas de cada area.

### `Endpoints/`

Son las puertas de entrada de la API.

- `AuthApi.cs`: login, usuario actual y crear usuarios.
- `CatalogoApi.cs`: clientes, productos, empleados y materiales.
- `SolicitudApi.cs`: solicitudes de los clientes.
- `CotizacionApi.cs`: propuestas de precio.
- `PedidoApi.cs`: trabajos que se van a ejecutar.

### `Models/`

Son las clases que representan las tablas SQL y sus relaciones.

### `Services/`

Son servicios reutilizables. `AuthService` hace dos cosas importantes:

- transforma una password en hash BCrypt;
- crea el JWT despues de un login correcto.

### `Repositories/`

Es una capa intermedia para no escribir toda la consulta en el endpoint. En este proyecto se usa principalmente para solicitudes.

El endpoint dice “necesito las solicitudes” y el Repository sabe como buscarlas en EF Core.

### `Middleware/`

Es codigo que pasa por todas las peticiones. Nuestro middleware registra:

- metodo HTTP;
- ruta;
- estado de respuesta;
- tiempo de respuesta.

### `OpenApi/`

Ayuda a que OpenAPI/Scalar sepa que existe autenticacion Bearer JWT.

### `Migrations/`

Guarda cambios que EF Core podria aplicar a la estructura de la base. Como nuestra base ya existia y tiene datos, la migracion inicial se genero pero no se aplico encima sin revisar.

### `LetrerosHerreraSYNCTIMEapi.Tests/`

Son pruebas automaticas que llaman la API y revisan los codigos HTTP.

### `bin/`, `obj/` y `.vs/`

Son carpetas generadas por Visual Studio y .NET. No son codigo nuestro y estan excluidas por `.gitignore`.

---

## 5. Como funciona un login

La peticion es:

```text
POST /api/v1/auth/login
```

Con un JSON que contiene email y password.

El proceso es:

1. `AuthApi` recibe el JSON.
2. Busca el usuario activo en `db.Usuarios`.
3. `AuthService` compara la password con el hash BCrypt.
4. Si falla, responde `401 Unauthorized`.
5. Si funciona, crea un JWT.
6. El cliente guarda el token.

El token se envia despues asi:

```text
Authorization: Bearer TOKEN
```

---

## 6. Que significa cada rol

Los roles estan guardados en el usuario y tambien viajan dentro del JWT.

- `dueno`: puede administrar usuarios y eliminar recursos.
- `disenador`: puede trabajar con solicitudes y cotizaciones.
- `operario`: puede trabajar con clientes, solicitudes y pedidos.
- `otro`: rol valido con permisos limitados.

Ejemplo:

```csharp
[Authorize(Roles = "dueno")]
```

Significa: solo puede entrar un usuario autenticado cuyo token tenga el rol `dueno`.

Diferencia entre respuestas:

- `401`: no hay token valido.
- `403`: hay token, pero el rol no alcanza.

---

## 7. Como funcionan los endpoints

Todos usan el mismo patron:

```text
recibir JSON
   -> validar
   -> consultar relaciones
   -> guardar o leer
   -> responder codigo HTTP
```

### Clientes

```text
GET    /api/v1/clientes/
GET    /api/v1/clientes/{id}
POST   /api/v1/clientes/
PUT    /api/v1/clientes/{id}
DELETE /api/v1/clientes/{id}
```

Consultar requiere login. Crear y modificar permite `dueno` u `operario`. Eliminar queda para `dueno`.

### Productos

```text
GET    /api/v1/productos/
GET    /api/v1/productos/{id}
POST   /api/v1/productos/
PUT    /api/v1/productos/{id}
DELETE /api/v1/productos/{id}
```

La lectura es publica. Las modificaciones quedan para `dueno`.

### Solicitudes

```text
GET    /api/v1/solicitudes/
GET    /api/v1/solicitudes/{id}
POST   /api/v1/solicitudes/
PUT    /api/v1/solicitudes/{id}
DELETE /api/v1/solicitudes/{id}
```

Antes de guardar se revisa que el cliente y el producto existan y que la cantidad sea mayor que cero.

### Cotizaciones

```text
GET    /api/v1/cotizaciones/
GET    /api/v1/cotizaciones/{id}
POST   /api/v1/cotizaciones/
PUT    /api/v1/cotizaciones/{id}
DELETE /api/v1/cotizaciones/{id}
```

Una cotizacion tiene detalles. El total se calcula en el servidor:

```text
cantidad x precio unitario = subtotal
suma de subtotales = total
```

Crear o modificar requiere `dueno` o `disenador`. Eliminar requiere `dueno`.

### Pedidos

```text
GET    /api/v1/pedidos/
GET    /api/v1/pedidos/{id}
POST   /api/v1/pedidos/
PUT    /api/v1/pedidos/{id}
DELETE /api/v1/pedidos/{id}
```

El pedido representa el trabajo que se ejecutara. Se validan solicitud, cliente, cotizacion, empleado, estado y prioridad.

### Consultas operativas

```text
GET /api/v1/empleados
GET /api/v1/materiales
```

Requieren autenticacion.

---

## 8. Seguridad explicada facil

### Password

Nunca se guarda la password original. Se guarda un hash BCrypt. El hash sirve para comparar, pero no para recuperar la password original.

### JWT

Es una credencial firmada. La API revisa que no haya sido alterada, que pertenezca al issuer esperado y que no este vencida.

### CORS

Indica que frontend puede llamar a la API desde un navegador. No usamos `AllowAnyOrigin`; se usa un origen configurado.

### Rate limiting

Pone un limite de peticiones. El login permite menos intentos que el resto de la API para dificultar ataques de fuerza bruta.

### HTTPS

Evita que el token viaje expuesto durante el transporte.

### Validacion

Se revisa la entrada antes de tocar la base. Por ejemplo, no se puede crear una solicitud con cliente inexistente.

---

## 9. GitHub y secretos

GitHub es donde se puede publicar el codigo. Si una password esta escrita en un archivo que se sube, otras personas pueden verla.

Por eso:

- `appsettings.json` no contiene secretos;
- la cadena SQL esta en User Secrets;
- la clave JWT esta en User Secrets;
- `.gitignore` excluye carpetas generadas;
- el archivo `.http` usa marcadores;
- no se debe subir el `.rar` completo si contiene salidas o configuraciones locales.

Una frase para explicar esto:

> “La configuracion publica queda en appsettings, pero los secretos se cargan desde User Secrets o variables de entorno para no versionarlos en GitHub.”

---

## 10. Pruebas

El comando principal es:

```powershell
dotnet test .\LetrerosHerreraSYNCTIMEapi\LetrerosHerreraSYNCTIMEapi.slnx -c Release
```

Las pruebas comprueban:

1. OpenAPI responde correctamente.
2. OpenAPI contiene Bearer.
3. Productos funciona sin login.
4. Clientes rechaza una peticion sin token.
5. Un login con password incorrecta responde `401`.
6. Si se configuran credenciales locales, login valido permite entrar a `/auth/me`.

La prueba de login valido lee las credenciales desde User Secrets. Si no estan configuradas, no intenta conectarse con una password escrita en el codigo.

---

## 11. Como estudiar el proyecto en orden

Recomiendo este orden:

1. Leer este documento.
2. Leer `Scripts/LetrerosHerrera.Database.sql` para entender las tablas.
3. Leer `Models/LetrerosHerreradbContext.cs` para ver como se mapean.
4. Leer `Program.cs` para ver como se registra todo.
5. Leer `Services/AuthService.cs` para entender BCrypt y JWT.
6. Leer `Endpoints/AuthApi.cs` para entender login.
7. Leer `Endpoints/CatalogoApi.cs` para entender un CRUD simple.
8. Leer `Endpoints/SolicitudApi.cs` para entender Repository y validacion.
9. Leer `CotizacionApi.cs` y `PedidoApi.cs` para ver relaciones del negocio.
10. Leer `Middleware/RequestLoggingMiddleware.cs`.
11. Leer `OWASP.md`.
12. Ejecutar las pruebas.

---

## 12. Resumen para memorizar

```text
Program configura.
Models representan la base.
Endpoints reciben peticiones.
Services hacen logica reutilizable.
Repository separa consultas.
Middleware observa todas las peticiones.
JWT identifica.
Roles autorizan.
Tests comprueban.
```
