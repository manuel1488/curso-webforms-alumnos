# Paso a paso · Día 5

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día. Esa solución incluye además el **filtro por cliente y familia** de Piezas, que es material opcional de [`dia-6/`](../dia-6/README.md); no hace falta para seguir la clase.

Hoy trabajas **sobre tu proyecto del día 4**.

---

> **Bloque 1 · Exportar Piezas a Excel**

Tres ideas:

- **Una librería**, ClosedXML, escribe el Excel por nosotros. Se instala con NuGet.
- **Una sola consulta.** El Excel usa el mismo `Listar` que la tabla: no pueden discrepar.
- **El handler ahora responde un archivo**, no JSON. Dos encabezados HTTP le dicen al navegador qué es y que lo descargue.

## 1. Instalar ClosedXML

📖 [Microsoft: qué es NuGet](https://learn.microsoft.com/es-es/nuget/what-is-nuget) · [Microsoft: Install-Package en la consola de Visual Studio](https://learn.microsoft.com/es-es/nuget/consume-packages/install-use-packages-powershell) · [Microsoft: `packages.config`](https://learn.microsoft.com/es-es/nuget/reference/packages-config) · [ClosedXML: documentación](https://docs.closedxml.io/en/latest/)

Antes de empezar, comprueba que tu Piezas lista, guarda y elimina. Si te falta algo, copia los archivos de [`dia-4/ejemplos/piezas/`](../dia-4/ejemplos/piezas/), inclúyelos en el proyecto y compila.

En Visual Studio: *Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes*. Verifica que **Proyecto predeterminado** diga `CursoWeb` y ejecuta:

```powershell
Install-Package ClosedXML -Version 0.105.1
```

Compilar con **Ctrl+Shift+B**. Debe compilar sin errores. Se necesita internet solo para instalar el paquete.

Qué pasó en tu proyecto:

| Dónde | Qué cambió | Por qué |
|---|---|---|
| `packages.config` | Aparecen 12 paquetes nuevos, no solo `ClosedXML` | ClosedXML **depende** de otras librerías. NuGet instala las dependencias solo |
| `CursoWeb.csproj` | Una referencia nueva por cada DLL | Es lo que permite escribir `using ClosedXML.Excel;` |
| `Web.config` | Cuatro `bindingRedirect` nuevos | Cuando dos librerías piden versiones distintas de la misma DLL, le dicen a .NET cuál usar. Si faltaran, la aplicación falla **al ejecutar**, no al compilar |
| `bin/` | Las DLL de todas las librerías | Al publicar, `bin/` viaja con el sitio: **el servidor no necesita internet ni NuGet** |

## 2. Crear el libro: `ExportarExcel`

**Agregar** a `Negocio/PiezasNegocio.cs`.

*1. Los `using` de arriba.*

Antes:

```csharp
using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;
```

Después:

```csharp
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using CursoWeb.Datos;
using CursoWeb.Modelos;
```

*2. El método, entre `Listar` y `Guardar`.*

```csharp
        // Genera el Excel con todas las piezas activas y regresa los bytes del archivo
        public byte[] ExportarExcel()
        {
            var piezas = datos.Listar();

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.AddWorksheet("Piezas");

                hoja.Cell(1, 1).Value = "Cliente";
                hoja.Cell(1, 2).Value = "Familia";
                hoja.Cell(1, 3).Value = "Número de parte";
                hoja.Cell(1, 4).Value = "Descripción";
                hoja.Cell(1, 5).Value = "Alta";
                hoja.Row(1).Style.Font.Bold = true;

                int fila = 2;
                foreach (var p in piezas)
                {
                    hoja.Cell(fila, 1).Value = p.ClienteNombre;
                    hoja.Cell(fila, 2).Value = p.FamiliaNombre;
                    hoja.Cell(fila, 3).Value = p.NumeroParte;
                    hoja.Cell(fila, 4).Value = p.Descripcion;
                    hoja.Cell(fila, 5).Value = p.FechaAlta;
                    fila++;
                }

                hoja.Column(5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                hoja.Columns().AdjustToContents();

                using (var memoria = new MemoryStream())
                {
                    libro.SaveAs(memoria);
                    return memoria.ToArray();
                }
            }
        }
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `datos.Listar()` | Pide las piezas | **La misma consulta** del listado: el Excel y la tabla no pueden discrepar |
| `new XLWorkbook()` dentro de `using` | Crea un libro de Excel en memoria | `using` lo libera al terminar, como `SqlConnection` |
| `AddWorksheet` y `Cell(fila, columna)` | Crea una hoja y escribe en esa celda | Como en Excel: fila 1 para los encabezados, datos desde la 2 |
| `.Value = p.FechaAlta` | Escribe una **fecha**, no un texto | Excel la reconoce como fecha: se puede ordenar y filtrar |
| `AdjustToContents()` | Ajusta el ancho de las columnas | Mide el texto con las fuentes del sistema |
| `MemoryStream` y `SaveAs(memoria)` | Guarda el libro en memoria, no en disco | El servidor no deja archivos temporales |
| `byte[]` como resultado | Negocio regresa **los bytes**, no escribe la respuesta | Negocio no sabe de HTTP: quien responde es el handler |

## 3. La acción `exportar` del handler

📖 [Microsoft: `HttpResponse.OutputStream`](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.outputstream) · [Microsoft: `HttpResponse.AddHeader` (la página indica usar `AppendHeader`)](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.addheader)

| Encabezado | Qué le dice al navegador | Valor de hoy |
|---|---|---|
| `Content-Type` (`ContentType`) | Qué clase de contenido viene | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` es el tipo de un `.xlsx` |
| `Content-Disposition` | Qué hacer con él: mostrarlo o **descargarlo**, y con qué nombre | `attachment; filename=Piezas_….xlsx` |

**Agregar** en `Handlers/PiezasHandler.ashx.cs`, entre `listar` y `guardar`.

Antes:

```csharp
                    case "listar":
                        respuesta = negocio.Listar();
                        break;

                    case "guardar":
```

Después:

```csharp
                    case "listar":
                        respuesta = negocio.Listar();
                        break;

                    case "exportar":
                        byte[] archivo = negocio.ExportarExcel();
                        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        context.Response.AppendHeader("Content-Disposition", "attachment; filename=Piezas_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");
                        context.Response.OutputStream.Write(archivo, 0, archivo.Length);
                        return;

                    case "guardar":
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `negocio.ExportarExcel()` **antes** de tocar la respuesta | Genera el archivo primero | Si algo falla, el `catch` todavía puede responder un JSON de error |
| `ContentType = "application/vnd…"` | Reemplaza el `application/json` del inicio | Este caso responde otra cosa |
| `AppendHeader("Content-Disposition", …)` | `attachment` = **descárgalo**; `filename` = nombre sugerido | Sin `attachment` el navegador decide por su cuenta |
| `DateTime.Now.ToString(...)` en el nombre | Cada descarga con un nombre distinto | El navegador no sobrescribe archivos |
| `OutputStream.Write(...)` | Escribe los bytes en la respuesta | Es el equivalente binario de `Response.Write` |
| `return;` en lugar de `break;` | Termina **sin** escribir el JSON del final | Si no, se pegaría un JSON al final del archivo y Excel se quejaría |

Prueba rápida con la aplicación corriendo: abre `Handlers/PiezasHandler.ashx?accion=exportar` en el navegador. Baja `Piezas_….xlsx`; ábrelo en Excel.

| Si pasa esto | Revisa |
|---|---|
| Visual Studio no reconoce `XLWorkbook` | El `using ClosedXML.Excel;` y que el paquete esté en `packages.config` |
| Excel no abre el archivo o se queja | Que `exportar` termine con `return;` y no con `break;` |
| Baja como `.ashx` o se muestra como texto | El `Content-Disposition` y el `ContentType` |
| «No se puede cargar el archivo o ensamblado» al ejecutar | Los `bindingRedirect` en `Web.config`; reinstala el paquete si faltan |
| Variable repetida en el `switch` | `archivo` solo se declara en el `case "exportar"` |

## 4. El botón y la descarga

📖 [jQuery: `.on()`](https://api.jquery.com/on/)

Todo lo que hicimos con el servidor desde JavaScript fue con `$.ajax`. Un archivo es distinto: queremos que **el navegador** lo reciba y lo guarde, así que no usamos AJAX: le decimos al navegador que vaya a esa URL. Como la respuesta trae `attachment`, descarga el archivo y la página se queda donde está.

**Cambiar** `Piezas.aspx`: el botón nuevo va junto a **Nueva pieza**, dentro de un `div`.

Antes:

```html
        <button type="button" id="btnNuevo" class="btn btn-primary">Nueva pieza</button>
```

Después:

```html
        <div>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
            <button type="button" id="btnNuevo" class="btn btn-primary">Nueva pieza</button>
        </div>
```

**Cambiar** `js/piezas.js`. El evento, al arrancar.

Antes:

```javascript
    $('#btnNuevo').on('click', nuevo);
```

Después:

```javascript
    $('#btnNuevo').on('click', nuevo);
    $('#btnExportar').on('click', exportar);
```

**Agregar** al final del archivo:

```javascript
// Descarga el Excel: se le pide al navegador que vaya al handler; la respuesta es un archivo
function exportar() {
    window.location = 'Handlers/PiezasHandler.ashx?accion=exportar';
}
```

### Probar

1. Compila y ejecuta. **Exportar a Excel**: baja `Piezas_….xlsx` y la página no cambia.
2. En Excel: una fila por pieza activa, acentos bien y **Alta** como fecha.
3. Elimina una pieza y exporta otra vez: ya no aparece.

| Si pasa esto | Revisa |
|---|---|
| El botón no hace nada | Que `#btnExportar` esté enlazado en el arranque, y F12 → Console |
| La página se queda en blanco o muestra texto raro | La acción en la URL: `accion=exportar` en minúsculas |
| Acentos raros en Excel | Que el `.cs` esté guardado en UTF-8 (Visual Studio lo hace por omisión) |

---

> **Bloque 2 · Ejercicio integrador y publicación**

## 5. Ejercicio integrador: exportar Clientes

Ahora lo haces tú. En Piezas exportamos a Excel; haz lo mismo en **Clientes**.

**Requerimiento.** En la pantalla Clientes, un botón **Exportar a Excel** que descargue un archivo con los clientes activos.

**Columnas.** Nombre, RFC, Contacto, Correo y Alta.

**Entregable.** Funcionando con la misma arquitectura: Negocio, handler y página.

Pistas:

| Capa | Qué tocar | Te puede servir |
|---|---|---|
| Negocio | `ClientesNegocio`: un método que regrese `byte[]` | Mira `PiezasNegocio.ExportarExcel`. `RFC`, `Contacto` y `Correo` pueden ser `NULL` |
| Handler | `ClientesHandler.ashx.cs`: una acción nueva | Mira cómo termina `exportar` en `PiezasHandler` |
| Página | `Clientes.aspx` y `js/clientes.js`: botón y función | `window.location` |

Tiempo: 40 minutos. Al terminar lo revisamos juntos.

### El otro integrador: Producción por lote (opcional)

No lo hacemos en clase, pero queda publicado para que lo hagas por tu cuenta: una pantalla **Producción** para registrar, consultar (por pieza y rango de fechas), editar, eliminar y exportar lotes de producción. Es Piezas otra vez, con tres diferencias: cuelga de **tres** niveles (cliente → familia → pieza), el filtro es por **fechas** y tiene más reglas.

Lo que necesitas para abrirlo:

1. Descarga el repositorio actualizado (*Git → Extraer* o `git pull origin main`).
2. **Ejecuta [`sql/04-lotes.sql`](../sql/04-lotes.sql) en SSMS.** Es la tabla `Lotes`; sin ella la solución no funciona. En clase lo hacemos juntos.
3. Abre la solución de [`dia-6/final/`](../dia-6/final/): copia `connectionStrings.ejemplo.config` como `connectionStrings.config` y escribe tu contraseña, como el día 2.

El material completo (requerimiento, diseño, código por capa) está en [`dia-6/`](../dia-6/README.md).

## 6. De desarrollo a producción

📖 [Microsoft: implementación en prueba en IIS local](https://learn.microsoft.com/es-es/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/deploying-to-iis) (tutorial de 2019 con Visual Studio 2017: sirve de referencia, algunos pasos cambian)

Hasta hoy la aplicación corre en IIS Express, en tu máquina, con tu usuario. **Publicar** es dejarla en un servidor para que otros la usen. Cambian tres cosas: quién la ejecuta, cómo se compila y dónde está la base de datos.

| Tema | En desarrollo | En producción |
|---|---|---|
| Servidor | IIS Express, con tu usuario de Windows | IIS, con la **identidad del Application Pool** |
| `debug` en `Web.config` | `true` | `false` |
| Errores | Se ve el mensaje técnico | `customErrors` oculta el detalle |
| Cadena de conexión | Tu base y tu usuario de desarrollo | La de producción |
| Librerías | Las del proyecto | Las de `bin/`: el servidor no necesita internet ni NuGet |

`connectionStrings.config` no se sube a Git, pero **si está incluido en el proyecto sí se copia al publicar**, con la contraseña de desarrollo. En el servidor se reemplaza por la de producción.

## 7. Publicar a una carpeta

1. En Visual Studio: clic derecho en el proyecto **CursoWeb** → *Publicar…* → destino **Carpeta**.
2. Ruta: `C:\sitios\CursoWeb` (evita `C:\inetpub`, que pide permisos de administrador). Configuración **Release**.
3. **Publicar** y abre la carpeta:

| Qué revisar | Qué debe verse |
|---|---|
| `bin/` | Las DLL del proyecto y de las librerías (`CursoWeb.dll`, `ClosedXML.dll`…) |
| `.aspx`, `.ashx`, `Site.Master` | Sí están |
| Archivos `.cs` | **No** están: ya se compilaron dentro de `CursoWeb.dll` |
| `lib/`, `css/`, `js/` | Sí están |
| `Web.config` | Sin `debug="true"` |

---

> **Bloque 3 · IIS y cierre**

## 8. IIS: Application Pool, sitio y permisos de carpeta

📖 [Microsoft: identidades del grupo de aplicaciones](https://learn.microsoft.com/es-es/iis/manage/configuring-security/application-pool-identities)

Necesitas IIS con **ASP.NET 4.8** activado (*Activar o desactivar las características de Windows*) y permisos de administrador. Si no puedes instalarlo, sigue la clase en la pantalla del instructor.

**Application Pool.** *Administrador de IIS → Grupos de aplicaciones → Agregar grupo de aplicaciones…*

| Campo | Valor | Por qué |
|---|---|---|
| Nombre | `CursoWeb` | La identidad se llamará `IIS AppPool\CursoWeb` |
| Versión de .NET CLR | `v4.0.30319` | Es el runtime de .NET Framework 4.x |
| Modo de canalización | **Integrado** | El modo normal de las aplicaciones actuales |

**Sitio.** *Sitios → Agregar sitio web…*

| Campo | Valor |
|---|---|
| Nombre del sitio | `CursoWeb` |
| Grupo de aplicaciones | `CursoWeb` |
| Ruta de acceso física | `C:\sitios\CursoWeb` (la carpeta publicada) |
| Puerto | `8081` |

**Permisos de la carpeta.** La identidad del pool necesita leerla:

```powershell
icacls "C:\sitios\CursoWeb" /grant "IIS AppPool\CursoWeb:(OI)(CI)RX"
```

## 9. Permisos en SQL Server

📖 [Microsoft: `CREATE LOGIN`](https://learn.microsoft.com/es-es/sql/t-sql/statements/create-login-transact-sql) · [Microsoft: `GRANT` permisos de esquema](https://learn.microsoft.com/es-es/sql/t-sql/statements/grant-schema-permissions-transact-sql)

La identidad del pool también necesita entrar a SQL Server, con los **mismos permisos mínimos** que `usrCursoWeb`. En SSMS, como administrador:

```sql
USE master;
GO
CREATE LOGIN [IIS APPPOOL\CursoWeb] FROM WINDOWS;
GO

USE CursoWeb;
GO
CREATE USER [IIS APPPOOL\CursoWeb] FOR LOGIN [IIS APPPOOL\CursoWeb];
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO [IIS APPPOOL\CursoWeb];
GO
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `CREATE LOGIN … FROM WINDOWS` | Da de alta en el **servidor** a una identidad de Windows | La identidad es la del pool: no hay contraseña que guardar ni filtrar |
| `CREATE USER … FOR LOGIN` | La mapea a un usuario **dentro de la base** | Login = entrar al servidor; usuario = estar en una base |
| `GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo` | Los mismos permisos de `usrCursoWeb` | Mínimo privilegio, sin `DELETE` |

Luego, en la **carpeta publicada** (no en el proyecto), cambia `C:\sitios\CursoWeb\connectionStrings.config` para usar la identidad del pool:

```xml
<add name="CursoWeb" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=CursoWeb;Integrated Security=SSPI"
     providerName="System.Data.SqlClient"/>
```

Si falla el acceso por identidad, deja el `connectionStrings.config` publicado como se copió (con `usrCursoWeb`) y sigue: la aplicación funciona igual. En producción se usa la identidad del pool.

## 10. Probar la aplicación publicada y qué sigue

Abre `http://localhost:8081/`. La aplicación corre **sin Visual Studio**.

1. `Default.aspx` carga el menú con estilos.
2. Clientes lista, guarda, edita y elimina.
3. **Exportar a Excel** en Clientes y en Piezas descarga el archivo.
4. F12 → Network: las llamadas a los handlers regresan 200.

| Síntoma | Qué revisar |
|---|---|
| **HTTP 500.19** | ASP.NET 4.8 activado y el pool en `v4.0.30319` |
| **403** o acceso denegado | Permisos de carpeta de `IIS AppPool\CursoWeb` |
| La tabla sale vacía y hay un 500 en Network | La cadena de conexión publicada y el login: busca *Login failed for user 'IIS APPPOOL\CursoWeb'* |
| «Ocurrió un error en el servidor» | Es el `catch` del handler: con `debug="false"` no muestra el detalle. Prueba con `usrCursoWeb` |
| Estilos o scripts no cargan | Que `lib/`, `css/` y `js/` estén en la carpeta publicada |
| «No se puede cargar el archivo o ensamblado» | Que `bin/` tenga las DLL de ClosedXML |

**Qué sigue.** El mismo `Listar()` de Clientes con Entity Framework 6 en lugar de ADO.NET, las migraciones de base de datos y Web API 2 como forma moderna de hacer lo que hicimos con los `.ashx`. No se instala nada hoy: queda como camino para seguir aprendiendo.

---

## Material opcional

Si quieres practicar más, en [`dia-6/`](../dia-6/README.md) están el **filtro por cliente y familia** del listado de Piezas y el ejercicio **Producción por lote**, con paso a paso y solución.
