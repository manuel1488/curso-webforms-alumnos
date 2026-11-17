# Paso a paso · Día 6 · Integrador «Producción por lote»: agregar el Excel

El proyecto de partida, [`inicio/`](inicio/), **ya trae el integrador «Producción por lote» construido** en todas sus capas: datos, negocio, handler, página, menú y JavaScript. **Lo único que le falta es exportar a Excel.** Eso es lo que agregas aquí, con lo mismo que ya hiciste en Piezas y en Clientes.

Cada sección corresponde a una capa y tiene dos partes: **lo que ya viene en `inicio/`** (para que lo repases: qué hace y por qué) y **lo que agregas** (los cambios, en formato Antes / Después). Los cambios son pequeños: cinco archivos y ningún archivo nuevo.

| Archivo | Qué se hace |
|---|---|
| `LotesDatos.cs` | Nada: el Excel reutiliza `Listar` |
| `LotesNegocio.cs` | **Agregar** `ExportarExcel` y dos `using` |
| `LotesHandler.ashx.cs` | **Agregar** la acción `exportar` |
| `Produccion.aspx` | **Agregar** el botón **Exportar a Excel** |
| `js/produccion.js` | **Agregar** el evento del botón y la función `exportar` |

**Antes de abrir `inicio/`:** copia `connectionStrings.ejemplo.config` como `connectionStrings.config` con tu contraseña y ejecuta [`sql/04-lotes.sql`](../sql/04-lotes.sql) en SSMS. La tabla `Lotes` no viene incluida en tu base de datos y la pantalla **Producción** no abre sin ella. El detalle de qué incluye `inicio/` y qué falta está en el [`README`](README.md) de esta carpeta.

El requerimiento, el diseño, la tabla `Lotes` y el modelo `Lote` están en las secciones 7 a 10 de [`opcional-filtro-excel-con-filtro-y-lotes.md`](opcional-filtro-excel-con-filtro-y-lotes.md). La solución completa, con Excel, está en [`final/`](final/): si algo no te funciona, compara tu archivo con el de esa carpeta.

---

## Antes de empezar: instalar ClosedXML

📖 [Microsoft: Install-Package en la consola de Visual Studio](https://learn.microsoft.com/es-es/nuget/consume-packages/install-use-packages-powershell) · [Microsoft: `packages.config`](https://learn.microsoft.com/es-es/nuget/reference/packages-config) · [ClosedXML: documentación](https://docs.closedxml.io/en/latest/)

`inicio/` solo trae `Newtonsoft.Json` y el compilador de Roslyn; **no trae ClosedXML**, así que `using ClosedXML.Excel;` no compilaría. Es el mismo paquete del día 5.

En Visual Studio: *Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes*. Verifica que **Proyecto predeterminado** diga `CursoWeb` y ejecuta:

```powershell
Install-Package ClosedXML -Version 0.105.1
```

Compila con **Ctrl+Shift+B**: debe compilar sin errores. Se necesita internet solo para instalar el paquete. Igual que en el día 5, aparecen en `packages.config` ClosedXML **y sus dependencias**, y en `Web.config` los `bindingRedirect` que hagan falta. Si faltaran, la aplicación falla **al ejecutar**, no al compilar.

## 1. Datos

📖 [SqlCommand.ExecuteScalar](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand.executescalar) · [SCOPE_IDENTITY](https://learn.microsoft.com/es-es/sql/t-sql/functions/scope-identity-transact-sql) · [Tipos de datos de fecha y hora](https://learn.microsoft.com/es-es/sql/t-sql/data-types/date-transact-sql) · [IS NULL](https://learn.microsoft.com/es-es/sql/t-sql/queries/is-null-transact-sql)

**Ya viene en `inicio/`:** `Datos/LotesDatos.cs`. **No se cambia.** El Excel llama a `LotesDatos.Listar` con los mismos filtros de la tabla: así el archivo y la pantalla no pueden discrepar.

| Pieza | Qué hace | Por qué |
|---|---|---|
| Tres `INNER JOIN` | Lotes → Piezas → Familias → Clientes | Todas las llaves son `NOT NULL` con llave foránea: ninguna fila se pierde |
| `(@Desde IS NULL OR l.FechaProduccion >= @Desde)` | Si no hay fecha inicial, no filtra | Es el parámetro opcional del filtro de Piezas, pero con **fechas**: `NULL` significa «sin límite» (en los números era `0`) |
| `DateTime?` | Una fecha que puede no existir | Es la forma de decir «sin límite» en C# |
| `(object)desde ?? DBNull.Value` | Manda `NULL` a SQL cuando no hay fecha | ADO.NET no acepta un `null` de C# como parámetro |
| `SqlDbType.Date` | El tipo del parámetro | El mismo de la columna: solo fecha |
| `Turno = dr.GetByte(...)` | Lee un `TINYINT` | `GetInt32` falla con `TINYINT`: cada tipo de SQL tiene su lector exacto |
| `(byte)l.Turno` al insertar | Convierte a `byte` | El parámetro es `TinyInt` |
| `ORDER BY l.FechaProduccion DESC` | Lo más reciente primero | Es lo que se espera de un registro de producción |
| `ExisteNumeroLote(piezaId, numeroLote, loteId)` | Cuenta lotes con ese número **para esa pieza**, activos o eliminados, sin el que se edita | Es `ExisteNumeroParte` con una llave compuesta |

El código completo está en [`inicio/…/Datos/LotesDatos.cs`](inicio/CursoWeb/CursoWeb/Datos/LotesDatos.cs).

## 2. Negocio

📖 [DateTime.Today](https://learn.microsoft.com/es-es/dotnet/api/system.datetime.today) · [Nullable&lt;T&gt;](https://learn.microsoft.com/es-es/dotnet/api/system.nullable-1) · [ClosedXML: documentación](https://docs.closedxml.io/en/latest/)

**Ya viene en `inicio/`:** `Negocio/LotesNegocio.cs` con `Listar`, `Guardar`, `Eliminar` y las validaciones.

| Pieza | Qué hace | Por qué |
|---|---|---|
| `Listar` valida el rango | Si `desde` es mayor que `hasta`, responde un error | La pantalla ya lo avisa, pero Negocio es quien decide |
| `Validar`: fecha futura | `FechaProduccion.Date > DateTime.Today` | La regla que no puede vivir en la tabla |
| `FechaProduccion == default(DateTime)` | Detecta que no llegó fecha | Una fecha que falta llega como `0001-01-01` |
| `Cantidad <= 0`, `Turno < 1 || Turno > 3` | Reglas de rango | También viven en los `CHECK` de la tabla: aquí dan un mensaje claro |
| `ExisteNumeroLote` antes de guardar | Mensaje claro si el lote ya existe para esa pieza | El `UNIQUE` sigue siendo la última defensa |

**Agregar** a `Negocio/LotesNegocio.cs`.

*1. Los `using` de arriba.*

Antes:

```csharp
using System;
using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;
```

Después:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using CursoWeb.Datos;
using CursoWeb.Modelos;
```

*2. El método, entre `Listar` y `Guardar`.*

```csharp
        // Genera el Excel de los lotes con los mismos filtros del listado y regresa los bytes del archivo
        public byte[] ExportarExcel(int piezaId, DateTime? desde, DateTime? hasta)
        {
            var lotes = datos.Listar(piezaId, desde, hasta);

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.AddWorksheet("Lotes");

                hoja.Cell(1, 1).Value = "Cliente";
                hoja.Cell(1, 2).Value = "Familia";
                hoja.Cell(1, 3).Value = "Número de parte";
                hoja.Cell(1, 4).Value = "Número de lote";
                hoja.Cell(1, 5).Value = "Cantidad";
                hoja.Cell(1, 6).Value = "Fecha de producción";
                hoja.Cell(1, 7).Value = "Turno";
                hoja.Row(1).Style.Font.Bold = true;

                int fila = 2;
                foreach (var l in lotes)
                {
                    hoja.Cell(fila, 1).Value = l.ClienteNombre;
                    hoja.Cell(fila, 2).Value = l.FamiliaNombre;
                    hoja.Cell(fila, 3).Value = l.NumeroParte;
                    hoja.Cell(fila, 4).Value = l.NumeroLote;
                    hoja.Cell(fila, 5).Value = l.Cantidad;
                    hoja.Cell(fila, 6).Value = l.FechaProduccion;
                    hoja.Cell(fila, 7).Value = l.Turno;
                    fila++;
                }

                hoja.Column(6).Style.DateFormat.Format = "dd/MM/yyyy";
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
| `datos.Listar(piezaId, desde, hasta)` | Pide los lotes con los filtros recibidos | **La misma consulta** del listado: el Excel y la tabla no pueden discrepar |
| `.Value = l.FechaProduccion` | Escribe una **fecha**, no un texto | Excel la reconoce como fecha: se puede ordenar y filtrar |
| `Column(6).Style.DateFormat.Format = "dd/MM/yyyy"` | Solo fecha, sin hora | La columna es `DATE`: no hay hora que mostrar (en Piezas, `Alta` sí llevaba hora) |
| `byte[]` como resultado | Negocio regresa **los bytes**, no escribe la respuesta | Negocio no sabe de HTTP: quien responde es el handler |

Es `PiezasNegocio.ExportarExcel` con otras columnas y con los filtros de pieza y fechas.

## 3. Handler

📖 [Microsoft: `HttpResponse.OutputStream`](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.outputstream) · [Microsoft: `HttpResponse.AddHeader` (la página indica usar `AppendHeader`)](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.addheader)

**Ya viene en `inicio/`:** `Handlers/LotesHandler.ashx` y `LotesHandler.ashx.cs` con las acciones `listar`, `guardar` y `eliminar`. El `.ashx` solo apunta a la clase y no se toca.

| Pieza | Qué hace | Por qué |
|---|---|---|
| `LeerFecha` | Lee `aaaa-mm-dd` de la URL y regresa `null` si falta o no es válida | `<input type="date">` entrega ese formato. Una fecha que no llega es «sin límite» |
| `TryParseExact` con `CultureInfo.InvariantCulture` | Convierte solo ese formato | No depende de la configuración regional del servidor |
| `LeerEntero` | El de Piezas | Igual: `0` si falta |
| `guardar` y `eliminar` solo con `POST` | Con otro método responden 405 | Las acciones que escriben solo se aceptan por `POST`; `listar` y `exportar` son de lectura (`GET`) |

**Agregar** en `Handlers/LotesHandler.ashx.cs`, entre `listar` y `guardar`.

Antes:

```csharp
                    case "listar":
                        respuesta = negocio.Listar(LeerEntero(context, "piezaId"), LeerFecha(context, "desde"), LeerFecha(context, "hasta"));
                        break;

                    case "guardar":
```

Después:

```csharp
                    case "listar":
                        respuesta = negocio.Listar(LeerEntero(context, "piezaId"), LeerFecha(context, "desde"), LeerFecha(context, "hasta"));
                        break;

                    case "exportar":
                        byte[] archivo = negocio.ExportarExcel(LeerEntero(context, "piezaId"), LeerFecha(context, "desde"), LeerFecha(context, "hasta"));
                        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        context.Response.AppendHeader("Content-Disposition", "attachment; filename=Lotes_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");
                        context.Response.OutputStream.Write(archivo, 0, archivo.Length);
                        return;

                    case "guardar":
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `exportar` | **Idéntico al de Piezas**, salvo los filtros que se leen de la URL | `ContentType`, `Content-Disposition` y `OutputStream`, con `return;` |
| `return;` en lugar de `break;` | Termina **sin** escribir el JSON del final | Si no, se pegaría un JSON al final del archivo y Excel se quejaría |
| `negocio.ExportarExcel(...)` **antes** de tocar la respuesta | Genera el archivo primero | Si algo falla, el `catch` todavía puede responder un JSON de error |

Prueba rápida con la aplicación corriendo: abre `Handlers/LotesHandler.ashx?accion=exportar` en el navegador. Baja `Lotes_….xlsx`; ábrelo en Excel. También funciona con filtros en la URL, por ejemplo `&desde=2026-11-01&hasta=2026-11-30`.

## 4. La página y el menú

**Ya viene en `inicio/`:** `Produccion.aspx` con el filtro, la tabla, el modal de alta y edición, y el modal de confirmación. El enlace **Producción** del menú de `Site.Master` también.

| Pieza | Qué hace | Por qué |
|---|---|---|
| `<input type="date">` | Calendario del navegador | Entrega `aaaa-mm-dd` siempre, sin importar la región |
| `type="number" min="1"` en la cantidad | Solo enteros desde 1 | Ayuda al usuario; la regla real sigue en Negocio |
| `cboTurno` con tres `<option>` fijos | Las opciones no vienen del servidor | Solo hay tres turnos: no hace falta un handler |
| Tres combos con `disabled` | `cboFamilia` y `cboPieza` empiezan deshabilitados | Cada uno espera al anterior |
| Ids distintos del filtro y del modal | `cboFiltroPieza`, `txtDesde`, `txtHasta` en el filtro; `cboCliente`, `cboFamilia`, `cboPieza`, `txtFecha` en el modal | Controles diferentes, con prefijo que dice qué son |

**Agregar** en `Produccion.aspx`: el botón va junto a **Buscar** y **Limpiar**, dentro de la barra del filtro.

Antes:

```html
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
```

Después:

```html
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
```

El botón está en la barra del **filtro** porque el Excel respeta los filtros.

## 5. JavaScript

📖 [jQuery: `.on()`](https://api.jquery.com/on/) · [jQuery: `$.ajax()`](https://api.jquery.com/jQuery.ajax/) · [jQuery: `.val()`](https://api.jquery.com/val/) · [jQuery: `.prop()`](https://api.jquery.com/prop/) · [jQuery: `.append()`](https://api.jquery.com/append/) · [Bootstrap Table: métodos (`refresh`)](https://bootstrap-table.com/docs/api/methods/)

**Ya viene en `inicio/`:** `js/produccion.js` con la tabla, el filtro, los combos encadenados y guardar, editar y eliminar. Lo que importa para el Excel es `filtroAplicado`.

| Pieza | Qué hace | Por qué |
|---|---|---|
| `cargarFamilias(clienteId, familiaId, piezaId)` | Llena familias y, al editar, encadena `cargarPiezas` | Tres niveles: la pieza no se puede seleccionar hasta que lleguen las familias. Es la misma asincronía del día 4, un nivel más |
| `cargarPiezas(familiaId, piezaId)` | Pide las piezas con `PiezasHandler?accion=listar&familiaId=` | **Reutiliza** el filtro de Piezas del día 5: `clienteId` no llega y vale `0` («todos») |
| Descartar la respuesta tardía en los dos niveles | Compara con el combo actual | Mismo cuidado que en Piezas |
| `cargarPiezasFiltro` | Todas las piezas, para el filtro | En el filtro basta un combo; los tres niveles son del alta |
| `hoy()` y el atributo `max` | La fecha de hoy con formato `aaaa-mm-dd` | Evita elegir una fecha futura sin escribirla a mano. Negocio la valida de todos modos |
| `desde > hasta` como texto | Compara fechas `aaaa-mm-dd` | Ese formato ordena igual como texto que como fecha |
| `filtroAplicado` | Los filtros con los que se pidió la tabla por última vez | El Excel coincide con lo que se ve, aunque hayas cambiado un control sin pulsar **Buscar** |
| `l.fechaProduccion.substring(0, 10)` al editar | Se queda con `aaaa-mm-dd` del JSON | El JSON trae `2026-11-10T00:00:00` y el `<input type="date">` solo entiende los diez primeros caracteres |

**Agregar** en `js/produccion.js`. Son dos cambios.

*1. El evento, al arrancar.*

Antes:

```javascript
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
```

Después:

```javascript
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
    $('#btnExportar').on('click', exportar);
```

*2. La función, justo después de `limpiarFiltro`.*

```javascript
// Descarga el Excel con los mismos filtros que tiene la tabla
function exportar() {
    window.location = 'Handlers/LotesHandler.ashx?accion=exportar' + filtroAplicado;
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `window.location = …` | Le dice al **navegador** que vaya al handler | Un archivo no se pide con `$.ajax`: queremos que el navegador lo reciba y lo guarde. Como la respuesta trae `attachment`, descarga y la página se queda donde está |
| `+ filtroAplicado` | Agrega `&piezaId=…&desde=…&hasta=…` | Son los mismos filtros con los que se pidió la tabla; el handler los lee igual que en `listar` |

## 6. Probar

1. Ejecuta y abre **Producción**: la tabla muestra los lotes (vacía al principio).
2. **Nuevo lote**: **Familia** y **Pieza** están deshabilitadas. Elige un cliente: se habilita **Familia**. Elige una familia: se habilita **Pieza** con solo sus piezas.
3. Guarda un lote válido: aparece en la tabla.
4. Prueba las validaciones: cantidad `0`, fecha futura escrita a mano, turno sin elegir, y el mismo número de lote para la misma pieza (debe decir que ya existe; con otra pieza sí se permite).
5. **Editar**: cliente, familia y pieza quedan seleccionados. Cambia la cantidad y guarda.
6. **Eliminar** → confirmar: desaparece.
7. **Filtro**: por pieza; por rango de fechas; con «desde» posterior a «hasta» avisa.
8. **Exportar a Excel** sin filtro: baja `Lotes_….xlsx` y la página no cambia. En Excel debe haber una fila por lote activo, con los acentos bien y **Fecha de producción** como fecha.
9. **Exportar a Excel** con filtro: filtra por pieza o por fechas, pulsa **Buscar** y exporta. El Excel trae las mismas filas que la tabla.
10. Elimina un lote y exporta otra vez: ya no aparece.

Si algo falla:

| Si pasa esto | Revisa |
|---|---|
| Visual Studio no reconoce `XLWorkbook` | El `using ClosedXML.Excel;` y que ClosedXML esté en `packages.config` (sección «Antes de empezar») |
| «No se puede cargar el archivo o ensamblado» al ejecutar | Los `bindingRedirect` en `Web.config`; reinstala el paquete si faltan |
| Excel no abre el archivo o se queja | Que `exportar` termine con `return;` y no con `break;` |
| Baja como `.ashx` o se muestra como texto | El `Content-Disposition` y el `ContentType` |
| Variable repetida en el `switch` | `archivo` solo se declara en el `case "exportar"` |
| El botón no hace nada | Que `#btnExportar` esté enlazado en el arranque, y F12 → Console |
| El Excel trae más filas que la tabla | Que la función `exportar` agregue `filtroAplicado` a la URL |
| El combo de pieza no se llena | Que la URL sea `PiezasHandler.ashx?accion=listar&familiaId=…` (F12 → Network) y que esa familia tenga piezas activas |
| Al editar, la pieza no queda seleccionada | Que `editar` llame `cargarFamilias(l.clienteId, l.familiaId, l.piezaId)` con los tres valores |
| La fecha aparece vacía al editar | Que se recorte con `substring(0, 10)` |
| El filtro por fechas no filtra | Que las fechas viajen como `aaaa-mm-dd` y que `LeerFecha` use ese formato |
| La tabla sale vacía pero el handler sí responde | Los `field` de las columnas contra el JSON (camelCase: `numeroLote`, no `NumeroLote`) |
| Visual Studio no reconoce `Lote`, `LotesDatos`… | Que el archivo esté incluido en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*) |
| Acentos raros (`ProducciÃ³n`) | Guarda `Produccion.aspx` con *UTF-8 con firma* |
