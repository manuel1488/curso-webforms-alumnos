# Troubleshooting

Antes de preguntar, abre **F12 → Network** y revisa la respuesta del servidor: casi siempre ahí está el error real.

## La llamada AJAX no hace nada
- Revisa la pestaña **Console** por errores de JavaScript.
- Revisa **Network**: ¿se hizo la petición? ¿Qué código de estado regresó?
- Verifica que jQuery se cargue **antes** que tus scripts.

## El handler responde 500
- En Network abre la respuesta: ahí viene el mensaje de la excepción.
- Pon un punto de interrupción en `ProcessRequest` y ejecuta con F5.

## El handler responde 404
- Revisa la URL, por ejemplo `/Handlers/ClientesHandler.ashx`.
- Verifica que el archivo `.ashx` exista y que su clase coincida con la del `.ashx.cs`.

## `$.ajax` entra a `error` aunque el servidor respondió bien
- El handler debe responder `context.Response.ContentType = "application/json";`.
- La respuesta debe ser JSON válido. Revísala en Network → Response.

## El valor llega vacío al handler
- Con `POST` léelo con `context.Request.Form["campo"]`; con `GET`, con `context.Request.QueryString["campo"]`.
- El nombre del campo debe ser igual en JavaScript y en C#.

## Error de conexión a SQL Server
- Revisa la cadena de conexión en `Web.config` (servidor e instancia).
- Prueba la misma conexión en SSMS. Con SQL Express la instancia suele ser `.\SQLEXPRESS`.

## Bootstrap Table no muestra datos
- `data-field` debe coincidir **exactamente** (mayúsculas incluidas) con las propiedades del JSON.
- Verifica que el JSON tenga la forma que espera la tabla (un arreglo de objetos).

## El modal no abre o no cierra
- En Bootstrap 5 los atributos son `data-bs-toggle` y `data-bs-target`.
- Desde JavaScript: `bootstrap.Modal.getOrCreateInstance('#miModal').show();`

## En IIS: error 500.19 o 500.21
- Falta la característica ASP.NET 4.8 en IIS (ver guía de instalación).
- El Application Pool debe usar **.NET CLR v4.0** en modo **Integrated**.

## En IIS: no conecta a SQL Server
El sitio corre con la identidad del Application Pool, no con tu usuario. En SSMS crea el inicio de sesión `IIS APPPOOL\NombreDelPool` y dale permisos sobre la base de datos.
