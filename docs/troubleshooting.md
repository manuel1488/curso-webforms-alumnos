# Troubleshooting

Antes de preguntar, abre **F12 → Network** y revisa la respuesta del servidor: casi siempre ahí está el error real.

## En la consola sale `favicon.ico` 404
Es normal: el navegador siempre busca un icono para la pestaña y el proyecto no tiene. No afecta en nada.

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

## Bootstrap no se ve como en el ejemplo
- Revisa la clase letra por letra: si está mal escrita (`navbar-toggle` en lugar de `navbar-toggler`), Bootstrap no avisa, simplemente no la aplica.
- Revisa que la clase exista en la versión que usamos (5.3): muchos ejemplos de internet son de versiones anteriores.
- Con F12 → Elements → Styles ves qué reglas se aplican al elemento.

## En modo dispositivo (F12) todo se ve enorme y cortado
No es tu página: es el zoom que Chrome guarda para `localhost`. Presiona **Ctrl+0** en esa pestaña y recarga. Si sigue, prueba en una ventana de incógnito.

## En celular el diseño no se adapta
Falta la etiqueta `viewport` en el `<head>` de `Site.Master`:

```html
<meta name="viewport" content="width=device-width, initial-scale=1" />
```

## El modal no abre o no cierra
- En Bootstrap 5 los atributos son `data-bs-toggle` y `data-bs-target`.
- Desde JavaScript: `bootstrap.Modal.getOrCreateInstance('#miModal').show();`

## En IIS: error 500.19 o 500.21
- Falta la característica ASP.NET 4.8 en IIS (ver guía de instalación).
- El Application Pool debe usar **.NET CLR v4.0** en modo **Integrated**.

## En IIS: no conecta a SQL Server
El sitio corre con la identidad del Application Pool, no con tu usuario. En SSMS crea el inicio de sesión `IIS APPPOOL\NombreDelPool` y dale permisos sobre la base de datos.

## Git: `git` no se reconoce como comando
- Instala Git para Windows (guía de instalación, sección 6.0).
- Cierra y vuelve a abrir la terminal después de instalar.

## Git: `Author identity unknown` al hacer commit
Falta configurar tu nombre y correo: `git config --global user.name "..."` y `git config --global user.email "..."`. Revisa lo que tienes con `git config --global -l`.

## Git: `Repository not found` o `Authentication failed` con el repo del curso
El repo es privado y GitHub no te reconoce con acceso:
- Revisa que aceptaste la invitación (llega por correo o en `https://github.com/notifications`).
- Si iniciaste sesión con otra cuenta de GitHub, ve al *Administrador de credenciales* de Windows → *Credenciales de Windows*, borra `git:https://github.com` y vuelve a intentar.

## Git: mis commits en GitHub no salen con mi usuario
El correo de `git config --global user.email` no es el de tu cuenta de GitHub. Cámbialo por el de tu cuenta (o por el `@users.noreply.github.com` de *Settings → Emails*). Los commits nuevos ya saldrán con tu usuario.

## Git: `src refspec main does not match any` al hacer push
Todavía no tienes ningún commit, o tu rama se llama `master`. Revisa con `git branch`. Si es `master`: `git branch -M main` y vuelve a hacer push.

## Git: `rejected` o `fetch first` al hacer push
El repositorio de GitHub tiene commits que tú no tienes. Tráelos y vuelve a subir:

```powershell
git pull origin main
git push
```

Si el `pull` responde `refusing to merge unrelated histories`, creaste el repo en GitHub con README o `.gitignore`. Lo más sencillo: bórralo, créalo otra vez **vacío** y repite el `git push -u origin main`.

## Git: `bin/`, `obj/` o `.vs/` aparecen en `git status`
Te faltó el `.gitignore` o lo copiaste después del primer `git add`. Cópialo de `plantillas/VisualStudio.gitignore` del repo del curso como `.gitignore` en tu proyecto. Si esos archivos ya habían entrado a un commit, sácalos del control de versiones (no se borran de tu disco) y vuelve a guardar:

```powershell
git rm -r --cached .
git add .
git commit -m "Aplicar .gitignore"
```

## Git: `pull` marca conflicto en el repo del curso
Modificaste archivos dentro del repo del curso. Deshaz tus cambios con `git restore .` y vuelve a hacer `git pull origin main`. Tu código va en tu proyecto, no ahí.
