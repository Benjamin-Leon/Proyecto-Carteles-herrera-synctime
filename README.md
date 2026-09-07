# LetrerosHerreraSYNCTIMEapi

API REST para gestionar el flujo comercial de una empresa de letreros.

## Documentos principales

- `InformeTecnicoEntrega.md`: informe formal para la entrega.
- `GuiaEstudioSimple.md`: explicacion simple para estudiar el proyecto.
- `OWASP.md`: mitigaciones de seguridad documentadas.

## Proyecto

La solucion esta en `LetrerosHerreraSYNCTIMEapi.slnx` e incluye:

- API: `LetrerosHerreraSYNCTIMEapi/`
- pruebas: `LetrerosHerreraSYNCTIMEapi.Tests/`

## Validacion

```powershell
dotnet test .\LetrerosHerreraSYNCTIMEapi.slnx -c Release
```

Los secretos se configuran mediante User Secrets o variables de entorno. No se deben publicar claves JWT, passwords ni cadenas de conexion en GitHub.
