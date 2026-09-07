# Mitigaciones OWASP API Security

## API1 - Broken Object Level Authorization

Los endpoints exigen autenticacion y roles. Las operaciones de eliminacion se limitan a `dueno`; las escrituras se separan entre `dueno`, `disenador` y `operario`. Los identificadores relacionados se validan antes de guardar.

## API2 - Broken Authentication

El login devuelve una respuesta generica `401` tanto para usuario inexistente como para password incorrecto. Las contrasenas se almacenan con BCrypt y los JWT validan issuer, audience, firma y expiracion.

## API3 - Broken Object Property Level Authorization

Los endpoints reciben DTOs/records de request en lugar de enlazar directamente entidades completas. Los campos de estado, rol, cantidades y precios se validan mediante listas permitidas y rangos.

## API4 - Unrestricted Resource Consumption

Se configuraron limites fijos: 10 intentos de login por minuto y 100 requests por minuto para la API general. El middleware de rate limiting responde `429` al superar el limite.

## API5 - Broken Function Level Authorization

La autorizacion se aplica por endpoint con `RequireAuthorization` y `Authorize(Roles = ...)`. Por ejemplo, crear usuarios y eliminar entidades requieren el rol `dueno`.

## API8 - Security Misconfiguration

Las claves JWT y la cadena de conexion no estan en `appsettings.json`; se cargan desde User Secrets o variables de entorno. CORS no usa `AllowAnyOrigin`, sino un origen configurado.

## API9 - Improper Inventory Management

La API mantiene versionamiento explicito `/api/v1`, documentacion OpenAPI y rutas separadas por recurso. El archivo `.http` permite repetir las pruebas principales.

## Evidencia ejecutable

La suite de integracion ubicada en `LetrerosHerreraSYNCTIMEapi.Tests` verifica login, JWT, respuestas 401 y endpoints publicos/protegidos.