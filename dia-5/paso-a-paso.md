# Paso a paso · Día 5

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Filtro en Piezas y repaso del flujo**

Hoy tres cosas: cerramos Piezas, le ponemos un filtro por cliente y familia, y repasamos todo el recorrido de una petición. Después exportamos a Excel lo que el filtro muestre y, al final del día, recibes el ejercicio integrador.

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
