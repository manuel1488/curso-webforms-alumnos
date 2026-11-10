# Día 2 · Clientes de punta a punta

## Objetivos
- Usar JavaScript y jQuery para reaccionar a eventos de la página.
- Guardar la información en SQL Server con un usuario dedicado y la cadena de conexión fuera de Git.
- Separar el acceso a datos y la lógica de negocio en capas.
- Guardar y listar clientes usando AJAX, un HTTP Handler y JSON, sin recargar la página.

## Temas

| Bloque | Temas |
|---|---|
| 1 | Preparar el proyecto: jQuery y Bootstrap Table como archivos locales y Newtonsoft.Json por NuGet. JavaScript en la consola: `let`/`const`/`var`, objetos, arreglos, JSON y funciones. jQuery: selectores, `$(function)`, eventos, `val()` y mostrar mensajes con `.text()` |
| 2 | SQL Server: la base `CursoWeb` y la tabla `Clientes` escritas a mano (`IDENTITY`, `DATETIME2`, `N'...'`). Usuario dedicado `usrCursoWeb` con permisos mínimos y cadena de conexión en `connectionStrings.config`. Capa de Datos con ADO.NET y **parámetros**. Capa de Negocio. HTTP Handler (`.ashx`) con `GET` y `POST` |
| 3 | `$.ajax` para guardar sin recargar. Bootstrap Table para el listado: búsqueda, páginas y orden |

## Seguridad que se ve en el día
- **Inyección de SQL**: por qué nunca se concatenan valores en el SQL.
- **Mínimo privilegio**: la aplicación entra con un usuario que solo puede lo que necesita, sin `DELETE`.
- **Contraseñas fuera de Git**: `connectionStrings.config` no se sube.
- **XSS**: por qué se muestra con `.text()` y no con `.html()`, y por qué Bootstrap Table lleva `escape: true`.

## Ejercicios
1. Arma en la consola el objeto `cliente`, conviértelo a JSON y valídalo con `validarCliente()`.
2. Valida el formulario de Clientes con jQuery y muestra los errores en un mensaje de Bootstrap.
3. Crea en SSMS la base `CursoWeb`, la tabla `Clientes` y el usuario `usrCursoWeb`.
4. Implementa el alta de clientes desde la página hasta SQL Server.
5. Muestra el listado de clientes en Bootstrap Table.

## Resultado esperado
Puedes dar de alta un cliente desde la página y verlo en el listado sin recargar.

## Material
- [Paso a paso del día](paso-a-paso.md)
- [Scripts SQL de referencia](../sql/README.md)
