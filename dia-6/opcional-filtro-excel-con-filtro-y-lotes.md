# Paso a paso · Día 5

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Filtro en Piezas y repaso del flujo**

Hoy tres cosas: cerramos Piezas, le ponemos un filtro por cliente y familia, y repasamos todo el recorrido de una petición. Después exportamos a Excel lo que el filtro muestre y al final arrancas el ejercicio integrador (requerimiento, tabla y modelo en las secciones 7 a 10).

## 1. Cierra Piezas

Antes de seguir, comprueba tu Piezas con la lista de la sección 11 del [paso a paso del día 4](../dia-4/paso-a-paso.md). Si te falta algo, copia los archivos de [`dia-4/ejemplos/piezas/`](../dia-4/ejemplos/piezas/), inclúyelos en el proyecto y compila. El filtro de hoy se agrega **sobre tu proyecto del día 4**.

## 2. Filtro en el listado de Piezas

📖 [Transact-SQL: WHERE](https://learn.microsoft.com/es-es/sql/t-sql/queries/where-transact-sql) · [Transact-SQL: AND](https://learn.microsoft.com/es-es/sql/t-sql/language-elements/and-transact-sql) · [Transact-SQL: OR](https://learn.microsoft.com/es-es/sql/t-sql/language-elements/or-transact-sql) · [Int32.TryParse](https://learn.microsoft.com/es-es/dotnet/api/system.int32.tryparse) · [Bootstrap Table: métodos (`refresh`)](https://bootstrap-table.com/docs/api/methods/) · [jQuery: `.val()`](https://api.jquery.com/val/) · [jQuery: `.on()`](https://api.jquery.com/on/) · [jQuery: `$.ajax()`](https://api.jquery.com/jQuery.ajax/) · [Bootstrap: Select](https://getbootstrap.com/docs/5.3/forms/select/) · [Bootstrap: Gutters](https://getbootstrap.com/docs/5.3/layout/gutters/)

Hoy no hay archivos nuevos: se modifican los de Piezas. La construcción es guiada: el instructor cambia cada archivo en pantalla, tú copias el cambio de esta sección, compilas (**Ctrl+Shift+B**) y se revisa la tabla de *qué hace* y *por qué*.

La tabla de Piezas tiene una caja de búsqueda, pero esa busca solo entre las filas que **ya llegaron al navegador**. El filtro de hoy se hace **en el servidor**: el navegador manda qué cliente y qué familia quiere y SQL devuelve solo esas piezas. Después del receso, el Excel exportará exactamente lo que el filtro muestre.

Resumen de lo que se modifica (no se crea ningún archivo nuevo, y `CursoWeb.csproj` no cambia):

| Archivo | Qué se hace |
|---|---|
| `Datos/PiezasDatos.cs` | **Cambiar** `Listar` para que reciba `clienteId` y `familiaId` |
| `Negocio/PiezasNegocio.cs` | **Cambiar** `Listar` para que los reciba y los pase |
| `Handlers/PiezasHandler.ashx.cs` | **Cambiar** la acción `listar` para leerlos de la URL |
| `Piezas.aspx` | **Agregar** la barra del filtro encima de la tabla |
| `js/piezas.js` | **Agregar** el filtro: combos, búsqueda y recarga de la tabla |

La idea que se repite en las tres capas: **`0` significa «todos»**.

### Datos

**Cambiar** en `Datos/PiezasDatos.cs`. Tres cambios dentro de `Listar`.

*1. La firma del método.*

Antes:

```csharp
        public List<Pieza> Listar()
```

Después:

```csharp
        public List<Pieza> Listar(int clienteId, int familiaId)
```

*2. El `WHERE` del SQL.*

Antes:

```sql
WHERE
    p.Activo = 1
ORDER BY
```

Después:

```sql
WHERE
    p.Activo = 1
AND (@ClienteId = 0 OR f.ClienteId = @ClienteId)
AND (@FamiliaId = 0 OR p.FamiliaId = @FamiliaId)
ORDER BY
```

*3. Los parámetros, antes de `cn.Open()`.*

Antes:

```csharp
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
```

Después:

```csharp
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = familiaId;
                cn.Open();
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `(@ClienteId = 0 OR f.ClienteId = @ClienteId)` | Si llega `0`, la condición siempre es verdadera y no filtra; si llega un cliente, solo deja las piezas de ese cliente | Es un **parámetro opcional**: una sola consulta sirve para «todos» y para «uno». La alternativa, armar el `WHERE` pegando texto, abre la puerta a la inyección de SQL |
| `AND` entre las dos condiciones | Las dos se cumplen a la vez | Cliente y familia se combinan: con `0` y `0` sale todo, con un cliente salen sus piezas, con cliente y familia sale solo esa familia |
| `cmd.Parameters.Add(...)` | Manda los dos números al SQL | Siempre como parámetros, igual que en todo el curso |


### Negocio

**Cambiar** en `Negocio/PiezasNegocio.cs`.

Antes:

```csharp
        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
```

Después:

```csharp
        // clienteId y familiaId en 0 significan "todos"
        public Respuesta Listar(int clienteId, int familiaId)
        {
            return Respuesta.Exito("", datos.Listar(clienteId, familiaId));
```

Negocio solo pasa los valores. No valida nada porque cualquier número es válido: un cliente que no existe simplemente regresa una lista vacía.

### Handler

**Cambiar** en `Handlers/PiezasHandler.ashx.cs`.

Antes:

```csharp
                    case "listar":
                        respuesta = negocio.Listar();
                        break;
```

Después:

```csharp
                    case "listar":
                        int clienteId;
                        int familiaId;
                        int.TryParse(context.Request.QueryString["clienteId"], out clienteId);
                        int.TryParse(context.Request.QueryString["familiaId"], out familiaId);
                        respuesta = negocio.Listar(clienteId, familiaId);
                        break;
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `QueryString["clienteId"]` | Lee `&clienteId=3` de la URL | Es un `GET`: los filtros viajan en la URL |
| `int.TryParse(..., out clienteId)` | Convierte el texto a número | Si el parámetro falta o no es un número, `TryParse` deja el valor en **`0`**, que aquí significa «todos». No hace falta revisar si funcionó: el `0` ya es el valor correcto por omisión. (En `listarPorCliente` sí se revisa, porque ahí el cliente es obligatorio) |

Prueba rápida en el navegador, con la aplicación corriendo:
- `Handlers/PiezasHandler.ashx?accion=listar` → todas las piezas, como antes.
- `Handlers/PiezasHandler.ashx?accion=listar&clienteId=1` → solo las piezas del cliente 1.
- `Handlers/PiezasHandler.ashx?accion=listar&clienteId=1&familiaId=1` → solo las de esa familia.

### La página

**Agregar** en `Piezas.aspx`, entre el `<div id="divMensaje" …>` y la tabla.

Antes:

```html
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <table id="tblPiezas"></table>
```

Después:

```html
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <!-- Filtro del listado -->
    <div class="row g-3 align-items-end mb-3">
        <div class="col-md-4">
            <label for="cboFiltroCliente" class="form-label">Cliente</label>
            <select id="cboFiltroCliente" class="form-select"></select>
        </div>
        <div class="col-md-4">
            <label for="cboFiltroFamilia" class="form-label">Familia</label>
            <select id="cboFiltroFamilia" class="form-select" disabled></select>
        </div>
        <div class="col-md-4">
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
        </div>
    </div>

    <table id="tblPiezas"></table>
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `cboFiltroCliente`, `cboFiltroFamilia` | Los combos del filtro | **Ids distintos** a `cboCliente` y `cboFamilia` del modal: son controles diferentes, y el prefijo `cbo` más el nombre dice qué son |
| `row g-3 align-items-end` | Una fila con separación entre columnas, alineada abajo | Para que los botones queden a la altura de los combos, no de sus etiquetas |
| `col-md-4` | Tres columnas iguales desde pantallas medianas | En pantallas chicas se apilan |
| `disabled` en el combo de familia | Empieza deshabilitado | Sin cliente elegido no hay familias que mostrar |
| `type="button"` | Los botones no envían el formulario | Mismo cuidado que en el modal |

### `js/piezas.js`

**Agregar y cambiar** cuatro cosas.

*1. Al arrancar: los eventos del filtro.* Después de la línea del evento `change` de `#cboCliente`.

Antes:

```javascript
    $('#cboCliente').on('change', cambiarCliente);
```

Después:

```javascript
    $('#cboCliente').on('change', cambiarCliente);
    $('#cboFiltroCliente').on('change', cambiarClienteFiltro);
    $('#btnBuscar').on('click', buscar);
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
```

*2. `cargarClientes`: llenar también el combo del filtro.*

Antes:

```javascript
            const $combo = $('#cboCliente');
            $combo.empty();
            $combo.append($('<option>').val('').text('Selecciona un cliente'));
            if (r.ok) {
                r.datos.forEach(function (c) {
                    $combo.append($('<option>').val(c.clienteId).text(c.nombre));
                });
            }
```

Después:

```javascript
            const $combo = $('#cboCliente');
            const $filtro = $('#cboFiltroCliente');
            $combo.empty();
            $combo.append($('<option>').val('').text('Selecciona un cliente'));
            $filtro.empty();
            $filtro.append($('<option>').val('0').text('Todos los clientes'));
            if (r.ok) {
                r.datos.forEach(function (c) {
                    $combo.append($('<option>').val(c.clienteId).text(c.nombre));
                    $filtro.append($('<option>').val(c.clienteId).text(c.nombre));
                });
            }
```

*3. Funciones nuevas.* **Agregar** antes de `function obtenerPieza()`:

```javascript
// Al cambiar el cliente del filtro, se piden sus familias para el combo del filtro
function cambiarClienteFiltro() {
    cargarFamiliasFiltro(Number($('#cboFiltroCliente').val()) || 0);
}

// Llena el combo de familias del filtro. Con "Todos los clientes" (0) queda deshabilitado
function cargarFamiliasFiltro(clienteId) {
    const $combo = $('#cboFiltroFamilia');
    $combo.empty();
    $combo.append($('<option>').val('0').text('Todas las familias'));
    $combo.prop('disabled', true);
    if (!clienteId) {
        return;
    }

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=listarPorCliente&clienteId=' + clienteId,
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            // Si el cliente del filtro cambió mientras llegaba la respuesta, esta ya no sirve
            if (clienteId !== (Number($('#cboFiltroCliente').val()) || 0)) {
                return;
            }
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger');
                return;
            }
            r.datos.forEach(function (f) {
                $combo.append($('<option>').val(f.familiaId).text(f.nombre));
            });
            $combo.prop('disabled', false);
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las familias del filtro (' + xhr.status + ').', 'danger');
        });
}

// La URL del listado lleva los filtros elegidos; 0 significa "todos"
function urlListado() {
    return 'Handlers/PiezasHandler.ashx?accion=listar'
        + '&clienteId=' + (Number($('#cboFiltroCliente').val()) || 0)
        + '&familiaId=' + (Number($('#cboFiltroFamilia').val()) || 0);
}

// Pide la tabla otra vez, con los filtros que hay en los combos
function recargarTabla() {
    $('#tblPiezas').bootstrapTable('refresh', { url: urlListado() });
}

function buscar() {
    ocultarMensaje();
    recargarTabla();
}

function limpiarFiltro() {
    $('#cboFiltroCliente').val('0');
    cargarFamiliasFiltro(0);
    buscar();
}
```

*4. La tabla usa la URL con filtros, y las recargas pasan por `recargarTabla()`.*

En `inicializarTabla`. Antes:

```javascript
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
```

Después:

```javascript
        url: urlListado(),
```

Y en `enviarPieza` y en `confirmarEliminar`. Antes (dos lugares):

```javascript
$('#tblPiezas').bootstrapTable('refresh');
```

Después:

```javascript
recargarTabla();
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `cargarFamiliasFiltro` | Llena el combo de familias del filtro con las del cliente elegido | Es `cargarFamilias` del modal, adaptada: pide `listarPorCliente` al servidor con AJAX, descarta la respuesta tardía y deshabilita el combo si no hay cliente. Se escribió **aparte** en lugar de modificar la del modal: el código se repite, pero cada función se entiende sola. Está en `notas/mejoras.md` (mejora 4) |
| `val('0')` en «Todos los clientes» y «Todas las familias» | El valor que significa «todos» | El mismo `0` que entiende SQL: viaja sin traducción por URL, handler, Negocio y Datos |
| `urlListado()` | Arma la URL con lo que hay en los combos | Una sola función que sabe cómo se pide el listado: la usan la tabla al nacer, **Buscar** y las recargas |
| `bootstrapTable('refresh', { url: … })` | Le dice a la tabla que vuelva a pedir sus datos, esta vez a esta URL | El método `refresh` acepta una `url`. La documentación oficial dice que la cambia «temporalmente»; no dependemos de eso porque **siempre** se la pasamos |
| `recargarTabla()` en guardar y eliminar | Tras guardar o eliminar, la tabla se recarga con el filtro actual | Si el usuario tiene filtrada una familia y guarda una pieza, la tabla sigue mostrando esa familia |
| Descartar la respuesta tardía | Si el cliente del filtro cambió mientras llegaba la respuesta, no se usa | Mismo cuidado que en el modal: AJAX es asíncrono |

### Probar

1. Compila y ejecuta. La tabla muestra **todas** las piezas, como antes.
2. En el filtro, **Familia** está deshabilitada.
3. Elige un cliente: la familia se habilita con **solo** sus familias.
4. **Buscar**: la tabla muestra solo las piezas de ese cliente.
5. Elige además una familia → **Buscar**: solo las de esa familia.
6. **Limpiar**: vuelve todo.
7. Con un filtro puesto, guarda una pieza nueva de esa familia: la tabla sigue filtrada y la muestra.

| Si pasa esto | Revisa |
|---|---|
| **Buscar** no hace nada | Que `#btnBuscar` esté enlazado en el arranque, y F12 → Console |
| La tabla no cambia pero el handler sí filtra | Que `urlListado()` se llame **al hacer clic**, no al arrancar, y que `recargarTabla()` pase `{ url: … }` |
| El combo de familia del filtro no se llena | La acción `listarPorCliente` del día 4, y la URL en Network (F12) |
| Sale vacío con un cliente que sí tiene piezas | Que las piezas estén activas y que su familia pertenezca a ese cliente |
| Visual Studio marca una variable repetida en el `switch` | `clienteId` y `familiaId` se declaran solo en el `case "listar"`; no deben declararse otra vez en otro `case` del mismo `switch` |
---

## 3. El recorrido de una petición

Cuando algo falla, casi siempre está en uno de estos pasos. Si sabes en cuál, la mitad del trabajo ya está hecha. Abre tu proyecto y F12 → *Network*, haz clic en **Buscar** y sigue cada paso:

| # | Dónde | Qué pasa |
|---|---|---|
| 1 | `Piezas.aspx` | Eliges cliente y familia y haces clic en `btnBuscar` |
| 2 | `js/piezas.js` → `buscar()` | Lee los combos, arma la URL con `urlListado()` y le dice a la tabla que se recargue |
| 3 | Navegador → servidor | `GET Handlers/PiezasHandler.ashx?accion=listar&clienteId=1&familiaId=2` (en *Network*) |
| 4 | `PiezasHandler.ashx.cs` | El `switch` elige `listar` y lee los dos números de la URL |
| 5 | `PiezasNegocio.Listar` | Pasa los valores; aquí irían las reglas |
| 6 | `PiezasDatos.Listar` | Manda el SQL con parámetros a SQL Server y lee los renglones |
| 7 | De regreso | Datos → Negocio → Handler: una lista de `Pieza` dentro de `Respuesta`, convertida a JSON (*Network → Response*) |
| 8 | `js/piezas.js` → `responseHandler` | Toma `r.datos` y se lo da a la tabla |
| 9 | Bootstrap Table | Dibuja las filas con los `field` de cada columna |

Haz lo mismo con **Guardar**. Cambia: es `POST`, el cuerpo lleva JSON, Negocio normaliza y valida, y Datos hace `INSERT`. La respuesta vuelve con `ok` y `mensaje`.

Para practicar: explícale a un compañero, con tu proyecto abierto, qué pasa desde el clic hasta que la tabla cambia.

---

> **Bloque 2 · Exportar a Excel**

Después del filtro, exportamos a Excel lo que la tabla muestra. Tres ideas:

- **Una librería**, ClosedXML, escribe el Excel por nosotros. Se instala con NuGet.
- **Una sola consulta.** El Excel usa el mismo `Listar` que la tabla, con el mismo filtro: no pueden discrepar.
- **El handler ahora responde un archivo**, no JSON. Dos encabezados HTTP le dicen al navegador qué es y que lo descargue.

## 4. ClosedXML: crear un Excel

📖 [Microsoft: qué es NuGet](https://learn.microsoft.com/es-es/nuget/what-is-nuget) · [Microsoft: Install-Package en la consola de Visual Studio](https://learn.microsoft.com/es-es/nuget/consume-packages/install-use-packages-powershell) · [Microsoft: `packages.config`](https://learn.microsoft.com/es-es/nuget/reference/packages-config) · [Microsoft: `MemoryStream`](https://learn.microsoft.com/es-es/dotnet/api/system.io.memorystream) · [ClosedXML: documentación](https://docs.closedxml.io/en/latest/)

### Instalar con NuGet

En Visual Studio: *Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes*. Verifica que **Proyecto predeterminado** diga `CursoWeb` y ejecuta:

```powershell
Install-Package ClosedXML -Version 0.105.1
```

Compilar con **Ctrl+Shift+B**. Debe compilar sin errores.

Qué pasó en tu proyecto:

| Dónde | Qué cambió | Por qué |
|---|---|---|
| `packages.config` | Aparecen 12 paquetes nuevos, no solo `ClosedXML` | ClosedXML **depende** de otras librerías (`DocumentFormat.OpenXml` para escribir el formato de Office, `SixLabors.Fonts` para medir texto, `System.Memory`, etc.). NuGet instala las dependencias solo |
| `CursoWeb.csproj` | Una referencia nueva por cada DLL | Es lo que permite escribir `using ClosedXML.Excel;` |
| `Web.config` | Cuatro `bindingRedirect` nuevos (`System.Memory`, `System.Buffers`, `System.Numerics.Vectors`, `System.Runtime.CompilerServices.Unsafe`) | Cuando dos librerías piden versiones distintas de la misma DLL, el redireccionamiento le dice a .NET cuál usar. NuGet lo agrega solo; si faltara, la aplicación falla **al ejecutar**, no al compilar |
| `bin/` | Las DLL de todas las librerías | Al publicar en el servidor, `bin/` viaja con el sitio: **el servidor no necesita internet ni NuGet** |

Nota: la conexión a internet se necesita solo para **instalar** el paquete. Esto es lo que distingue NuGet de un CDN: no hay descargas desde el navegador del usuario.

Se instala una **versión fija** (`-Version 0.105.1`) para que todos tengan la misma. La documentación de ClosedXML recomienda leer sus notas de versión antes de actualizar, porque su API puede cambiar.

### Crear el libro: `ExportarExcel`

📖 [ClosedXML: documentación](https://docs.closedxml.io/en/latest/)

**Agregar** a `Negocio/PiezasNegocio.cs`.

*1. Los `using` de arriba.*

Antes:

```csharp
using System.Collections.Generic;
using CursoWeb.Datos;
```

Después:

```csharp
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using CursoWeb.Datos;
```

*2. El método, antes de `Guardar`.*

```csharp
        // Genera el Excel de las piezas con los mismos filtros del listado y regresa los bytes del archivo
        public byte[] ExportarExcel(int clienteId, int familiaId)
        {
            var piezas = datos.Listar(clienteId, familiaId);

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
| `datos.Listar(clienteId, familiaId)` | Pide las piezas, con el filtro | **La misma consulta** del listado: el Excel y la tabla no pueden discrepar |
| `new XLWorkbook()` dentro de `using` | Crea un libro de Excel en memoria | `using` lo libera al terminar, como `SqlConnection` |
| `AddWorksheet("Piezas")` y `Cell(fila, columna)` | Crea una hoja y escribe en la celda de esa fila y columna | Como en Excel: fila 1 para los encabezados, datos desde la 2 |
| `.Value = p.FechaAlta` | Escribe una **fecha**, no un texto | Excel la reconoce como fecha: se puede ordenar y filtrar |
| `DateFormat.Format` | Cómo se **ve** la fecha | El valor es el mismo; solo cambia la presentación |
| `AdjustToContents()` | Ajusta el ancho de las columnas al contenido | Mide el texto con fuentes. En Windows las toma de `%SYSTEMROOT%\Fonts`; la documentación de ClosedXML advierte de errores de fuentes en Linux y servicios en la nube |
| `MemoryStream` y `SaveAs(memoria)` | Guarda el libro en memoria, no en disco | El servidor no deja archivos temporales. Se entrega con `ToArray()` |
| `byte[]` como resultado | Negocio regresa **los bytes**, no escribe la respuesta | Negocio no sabe de HTTP: quien responde es el handler. Así esta clase sirve también fuera de la web |

## 5. El handler de exportación

📖 [Microsoft: `HttpResponse.OutputStream`](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.outputstream) · [Microsoft: `HttpResponse.AddHeader` (la página indica usar `AppendHeader`)](https://learn.microsoft.com/es-es/dotnet/api/system.web.httpresponse.addheader) · [Microsoft: `Int32.TryParse`](https://learn.microsoft.com/es-es/dotnet/api/system.int32.tryparse)

| Encabezado | Qué le dice al navegador | Valor de hoy |
|---|---|---|
| `Content-Type` (`ContentType`) | Qué clase de contenido viene | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` es el tipo de un `.xlsx` |
| `Content-Disposition` | Qué hacer con él: mostrarlo o **descargarlo**, y con qué nombre | `attachment; filename=Piezas_….xlsx` |

El cuerpo de la respuesta ya no es texto: son los bytes del archivo, y se escriben en `Response.OutputStream`.

**Cambiar y agregar** en `Handlers/PiezasHandler.ashx.cs`.

*1. Agregar un método auxiliar*, antes de `LeerCuerpo`:

```csharp
        // Lee un número de la URL. Si falta o no es número, regresa 0
        private static int LeerEntero(HttpContext context, string nombre)
        {
            int valor;
            int.TryParse(context.Request.QueryString[nombre], out valor);
            return valor;
        }
```

*2. `listar` usa el método, y se agrega `exportar`.*

Antes:

```csharp
                    case "listar":
                        int clienteId;
                        int familiaId;
                        int.TryParse(context.Request.QueryString["clienteId"], out clienteId);
                        int.TryParse(context.Request.QueryString["familiaId"], out familiaId);
                        respuesta = negocio.Listar(clienteId, familiaId);
                        break;
```

Después:

```csharp
                    case "listar":
                        respuesta = negocio.Listar(LeerEntero(context, "clienteId"), LeerEntero(context, "familiaId"));
                        break;

                    case "exportar":
                        byte[] archivo = negocio.ExportarExcel(LeerEntero(context, "clienteId"), LeerEntero(context, "familiaId"));
                        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        context.Response.AppendHeader("Content-Disposition", "attachment; filename=Piezas_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");
                        context.Response.OutputStream.Write(archivo, 0, archivo.Length);
                        return;
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `LeerEntero` | Lee un número de la URL y regresa `0` si falta | Ahora se necesita en **dos** `case`. Las variables de un `switch` viven en todo el bloque: dos `case` no pueden declarar `clienteId`. Un método auxiliar evita repetir el `TryParse` |
| `negocio.ExportarExcel(...)` **antes** de tocar la respuesta | Genera el archivo primero | Si algo falla, el `catch` todavía puede responder un JSON de error: nada de la respuesta se ha escrito |
| `ContentType = "application/vnd…"` | Reemplaza el `application/json` del inicio | El handler empieza declarando JSON; aquí cambia porque este caso responde otra cosa |
| `AppendHeader("Content-Disposition", …)` | `attachment` = **descárgalo**, no lo muestres; `filename` = nombre sugerido | Sin `attachment` el navegador decide por su cuenta si lo muestra o lo descarga. `AppendHeader` es el método que recomienda la documentación (`AddHeader` es el equivalente heredado) |
| `DateTime.Now.ToString("yyyyMMdd_HHmm")` en el nombre | Cada descarga con un nombre distinto | El navegador no sobrescribe: `Piezas_20261111_2105.xlsx` |
| `OutputStream.Write(archivo, 0, archivo.Length)` | Escribe los bytes en el cuerpo de la respuesta | Es el equivalente binario de `Response.Write` |
| `return;` en lugar de `break;` | Termina el método **sin** pasar por el `Response.Write(JsonConvert…)` del final | Si no saliera aquí, se escribiría un JSON pegado al final del archivo, y ahí no debe ir |

Prueba rápida en el navegador, con la aplicación corriendo:
- `Handlers/PiezasHandler.ashx?accion=exportar` → baja `Piezas_….xlsx` con todas las piezas.
- `Handlers/PiezasHandler.ashx?accion=exportar&clienteId=1` → baja el archivo solo con ese cliente.
- Ábrelo en Excel.

Limitación: si `ExportarExcel` falla, el handler responde un JSON de error y el navegador lo muestra como texto, porque la descarga se pide con una navegación y no con AJAX. Para este curso se acepta.

| Si pasa esto | Revisa |
|---|---|
| Visual Studio no reconoce `XLWorkbook` | El `using ClosedXML.Excel;`, y que el paquete esté instalado en el proyecto (`packages.config`) |
| El archivo baja pero Excel no lo abre o se queja | Que `exportar` termine con `return;` y no con `break;` |
| Baja como `.ashx` o se muestra como texto | El `Content-Disposition` y el `ContentType` |
| Error de «No se puede cargar el archivo o ensamblado» al ejecutar | Los `bindingRedirect` en `Web.config`: NuGet los agrega al instalar. Reinstala el paquete si faltan |
| Exporta todo aunque filtraste | Que `LeerEntero` lea `clienteId` y `familiaId` con esos nombres exactos, y la URL en Network |

## 6. Descarga desde JavaScript

📖 [jQuery: `.on()`](https://api.jquery.com/on/) · [jQuery: `.val()`](https://api.jquery.com/val/)

**Cambiar y agregar** en `Piezas.aspx` y en `js/piezas.js`.

### La página

**Agregar** el botón en `Piezas.aspx`, después de **Limpiar**.

Antes:

```html
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
```

Después:

```html
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
```

### `js/piezas.js`

*1. Una variable nueva*, arriba, con las demás.

Antes:

```javascript
let modalConfirmar;
```

Después:

```javascript
let modalConfirmar;
let filtroAplicado = ''; // los filtros con los que se pidió la tabla; el Excel usa los mismos
```

*2. El evento del botón*, al arrancar.

Antes:

```javascript
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
```

Después:

```javascript
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
    $('#btnExportar').on('click', exportar);
```

*3. `urlListado` y `recargarTabla` cambian, y aparecen `parametrosFiltro` y `exportar`.*

Antes:

```javascript
// La URL del listado lleva los filtros elegidos; 0 significa "todos"
function urlListado() {
    return 'Handlers/PiezasHandler.ashx?accion=listar'
        + '&clienteId=' + (Number($('#cboFiltroCliente').val()) || 0)
        + '&familiaId=' + (Number($('#cboFiltroFamilia').val()) || 0);
}

// Pide la tabla otra vez, con los filtros que hay en los combos
function recargarTabla() {
    $('#tblPiezas').bootstrapTable('refresh', { url: urlListado() });
}
```

Después:

```javascript
// Los filtros que hay en los combos, listos para agregarse a una URL; 0 significa "todos"
function parametrosFiltro() {
    return '&clienteId=' + (Number($('#cboFiltroCliente').val()) || 0)
        + '&familiaId=' + (Number($('#cboFiltroFamilia').val()) || 0);
}

// La URL del listado lleva los filtros con los que se pidió la tabla
function urlListado() {
    return 'Handlers/PiezasHandler.ashx?accion=listar' + filtroAplicado;
}

// Pide la tabla otra vez, con los filtros que hay en los combos
function recargarTabla() {
    filtroAplicado = parametrosFiltro();
    $('#tblPiezas').bootstrapTable('refresh', { url: urlListado() });
}

// Descarga el Excel con los mismos filtros que tiene la tabla
function exportar() {
    window.location = 'Handlers/PiezasHandler.ashx?accion=exportar' + filtroAplicado;
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `window.location = url` | Le dice al navegador que navegue a esa URL | Como la respuesta es un archivo con `attachment`, el navegador lo descarga en lugar de cambiar de página |
| `filtroAplicado` | Guarda los filtros con los que se pidió la tabla por última vez | **El Excel debe coincidir con lo que se ve.** Si el usuario cambia un combo y no pulsa **Buscar**, la tabla sigue mostrando el filtro anterior, y el Excel también |
| `parametrosFiltro()` | Lee los combos y arma `&clienteId=…&familiaId=…` | Antes esa lectura estaba dentro de `urlListado`. Ahora se usa en un solo lugar: `recargarTabla` |
| `urlListado()` sin leer los combos | Usa `filtroAplicado` | Al arrancar es `''` (sin filtros): el primer listado trae todo, como antes |
| `exportar` | Reusa `filtroAplicado` en otra acción | Mismo filtro, otra forma de entregar el resultado |

### Probar

1. Compila y ejecuta. **Exportar a Excel** sin filtro: baja el archivo con todas las piezas.
2. Elige un cliente → **Buscar** → **Exportar a Excel**: el archivo trae solo ese cliente.
3. Elige además una familia → **Buscar** → **Exportar**: solo esa familia.
4. Elige otro cliente **sin pulsar Buscar** y exporta: el archivo trae lo que muestra la tabla, no lo que dicen los combos.
5. En Excel: acentos bien en *Número de parte* y *Descripción*, y la columna **Alta** como fecha.

| Si pasa esto | Revisa |
|---|---|
| El botón no hace nada | Que `#btnExportar` esté enlazado en el arranque, y F12 → Console |
| La página se queda en blanco o muestra texto raro | La acción en la URL: debe ser `accion=exportar` (minúsculas) |
| Exporta todo aunque filtraste | Que `recargarTabla()` actualice `filtroAplicado` **antes** de pedir la tabla |
| Los acentos salen raros en Excel | Que el `.cs` se guardó en UTF-8 (Visual Studio lo hace por omisión) |

---

> **Bloque 3 · Arranque del ejercicio integrador**

Cerramos el día con el ejercicio integrador: una pantalla nueva que pone en práctica todo lo del curso. Hoy se lee el requerimiento, se escribe la tabla `Lotes` y se hace el modelo `Lote`; el resto (datos, negocio, handler, página y JavaScript) se construye el [día 6](../dia-6/paso-a-paso.md). La solución completa está en [`dia-6/final/`](../dia-6/final/).

## 7. El requerimiento

**Contexto.** En planta se necesita saber cuántas piezas se produjeron, en qué lote y en qué turno, para poder rastrear un problema de calidad hasta su origen.

**Requerimiento.** Construye una pantalla **Producción** que permita:

1. Registrar un lote: cliente → familia → pieza (combos dependientes), número de lote, cantidad, fecha y turno (1, 2 o 3).
2. Consultar lotes filtrando por pieza y rango de fechas.
3. Editar y eliminar un lote.
4. Exportar a Excel la consulta filtrada.

**Reglas.**
- El número de lote no se repite para la misma pieza.
- La cantidad debe ser mayor a cero.
- La fecha no puede ser futura.

**Entregable.** La funcionalidad completa siguiendo la misma arquitectura del curso: página, handler, negocio, datos y tabla en SQL Server. Tú diseñas la tabla.

Es Piezas otra vez, con tres diferencias: la pantalla cuelga de **tres** niveles, el filtro es por **fechas** y las reglas son más. El handler, la exportación y la eliminación lógica ya los hiciste.

## 8. El diseño

La tabla `Lotes` lleva `LoteId`, `PiezaId`, `NumeroLote`, `Cantidad`, `FechaProduccion`, `Turno`, `Activo` y `FechaAlta`. Estas son las decisiones de la solución:

| Pieza del diseño | Decisión de la referencia | Por qué |
|---|---|---|
| **Tabla** | `LoteId`, `PiezaId`, `NumeroLote`, `Cantidad`, `FechaProduccion`, `Turno`, `Activo`, `FechaAlta` | Las columnas de control de siempre (`Activo`, `FechaAlta`) más lo que identifica a un lote |
| **Relación** | `PiezaId` con llave foránea a `Piezas` | Un lote es de una pieza; la familia y el cliente se alcanzan **pasando por** `Piezas` y `Familias`, como el cliente en Piezas |
| `FechaProduccion DATE` | Solo fecha, sin hora | La hora no importa para un lote, y evita problemas al filtrar por rango |
| `Turno TINYINT` | Un número chico | Solo 1, 2 o 3 |
| **Capas** | `Lote`, `LotesDatos`, `LotesNegocio`, `LotesHandler` | La misma arquitectura de Clientes, Familias y Piezas |
| **Eliminación** | Lógica (`Activo = 0`) | Igual que los demás catálogos |

Dónde vive cada regla:

| Regla | Dónde se aplica | Por qué ahí |
|---|---|---|
| El número de lote no se repite para la misma pieza | `UNIQUE (PiezaId, NumeroLote)` **y** Negocio (`ExisteNumeroLote`) | Negocio da un mensaje claro; la base es la última defensa. Es la llave **compuesta**: el mismo número de lote se puede repetir en piezas distintas |
| La cantidad es mayor a cero | `CHECK` en la tabla, Negocio y la pantalla | Tres capas de defensa. La pantalla avisa rápido, Negocio es la que decide, el `CHECK` evita datos malos si alguien entra por otro lado |
| El turno es 1, 2 o 3 | `CHECK`, Negocio y un combo con solo esas opciones | Mismo criterio |
| La fecha no es futura | Negocio y la pantalla (`max` del calendario) | **No** en la tabla: un `CHECK` con la fecha actual no es buena práctica, porque se evalúa al insertar y no vuelve a validarse |

Acciones del handler: `listar` (GET, con `piezaId`, `desde` y `hasta` opcionales), `guardar` (POST, inserta o actualiza según `loteId`), `eliminar` (POST, eliminación lógica) y `exportar` (GET, el mismo listado como Excel).

## 9. SQL: la tabla `Lotes`

📖 [CREATE TABLE](https://learn.microsoft.com/es-es/sql/t-sql/statements/create-table-transact-sql) · [Restricciones CHECK](https://learn.microsoft.com/es-es/sql/relational-databases/tables/create-check-constraints) · [Restricciones UNIQUE](https://learn.microsoft.com/es-es/sql/relational-databases/tables/create-unique-constraints)

Ejecuta en SSMS, sobre la base del curso:

```sql
CREATE TABLE dbo.Lotes
(
      LoteId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Lotes PRIMARY KEY
    , PiezaId          INT               NOT NULL CONSTRAINT FK_Lotes_Piezas REFERENCES dbo.Piezas (PiezaId)
    , NumeroLote       VARCHAR(50)       NOT NULL
    , Cantidad         INT               NOT NULL CONSTRAINT CK_Lotes_Cantidad CHECK (Cantidad > 0)
    , FechaProduccion  DATE              NOT NULL
    , Turno            TINYINT           NOT NULL CONSTRAINT CK_Lotes_Turno CHECK (Turno IN (1, 2, 3))
    , Activo           BIT               NOT NULL CONSTRAINT DF_Lotes_Activo DEFAULT (1)
    , FechaAlta        DATETIME2(0)      NOT NULL CONSTRAINT DF_Lotes_FechaAlta DEFAULT (SYSDATETIME())
    , CONSTRAINT UQ_Lotes_Pieza_NumeroLote UNIQUE (PiezaId, NumeroLote)
);
```

Para comprobar la relación de tres niveles (Cliente → Familia → Pieza → Lote):

```sql
-- Comprobar la relación de tres niveles: Cliente → Familia → Pieza → Lote
SELECT
      c.Nombre            AS Cliente
    , f.Nombre            AS Familia
    , p.NumeroParte       AS NumeroParte
    , l.NumeroLote        AS NumeroLote
    , l.Cantidad          AS Cantidad
    , l.FechaProduccion   AS FechaProduccion
    , l.Turno             AS Turno
FROM
    dbo.Lotes AS l
INNER JOIN
    dbo.Piezas AS p ON p.PiezaId = l.PiezaId
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId;
```

Pruebas con inserciones a mano: una cantidad de `0`, un turno de `4` y dos lotes con el mismo `PiezaId` y `NumeroLote` deben fallar por la restricción correspondiente; el mismo `NumeroLote` en dos piezas distintas sí se permite, porque la llave es compuesta. `usrCursoWeb` ya tiene permiso: el del día 2 es para todo el esquema `dbo`.

## 10. Modelo

**Archivo nuevo:** clic derecho en la carpeta `Modelos` → *Agregar → Clase* → `Lote.cs`. Reemplaza todo su contenido por:

```csharp
using System;

namespace CursoWeb.Modelos
{
    public class Lote
    {
        public int LoteId { get; set; }
        public int PiezaId { get; set; }
        public string NumeroParte { get; set; }
        public int FamiliaId { get; set; }
        public string FamiliaNombre { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string NumeroLote { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaProduccion { get; set; }
        public int Turno { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `PiezaId` | La columna que guarda la tabla | La relación |
| `NumeroParte`, `FamiliaId`, `FamiliaNombre`, `ClienteId`, `ClienteNombre` | No son columnas de `Lotes` | Vienen de los `JOIN` y sirven para mostrar y para editar con los combos dependientes |
| `Turno` como `int` | La columna es `TINYINT` | Se lee como `byte` y se convierte; en C# y en JSON es un número |
