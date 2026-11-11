# Paso a paso · Día 3

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Editar y eliminar Clientes**

Hoy el catálogo de Clientes se completa: además de dar de alta y listar, podrás **editar** y **eliminar**. Tres decisiones:

- Los datos para editar salen del **renglón de la tabla**: Bootstrap Table ya los tiene, no hace falta pedirlos otra vez al servidor.
- Una sola acción `guardar`: si `clienteId` es `0` inserta, si no actualiza. `enviarCliente()` no cambia.
- Antes de eliminar se pregunta con `confirm()`. En el bloque 2 lo cambiamos por un modal de Bootstrap.

## 1. Servidor: actualizar y eliminar

📖 [UPDATE (Transact-SQL)](https://learn.microsoft.com/es-es/sql/t-sql/queries/update-transact-sql) · [SqlCommand.ExecuteNonQuery](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand.executenonquery)

### Datos: `Actualizar` y `Desactivar`

Agrega a `Datos/ClientesDatos.cs`:

```csharp
public int Actualizar(Cliente c)
{
    const string sql = @"
UPDATE c
SET
      c.Nombre     = @Nombre
    , c.RFC        = @RFC
    , c.Contacto   = @Contacto
    , c.Correo     = @Correo
FROM
    dbo.Clientes AS c
WHERE
    c.ClienteId = @ClienteId
    AND c.Activo = 1;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = c.ClienteId;
        cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = c.Nombre;
        cmd.Parameters.Add("@RFC", SqlDbType.VarChar, 13).Value = (object)c.RFC ?? DBNull.Value;
        cmd.Parameters.Add("@Contacto", SqlDbType.NVarChar, 100).Value = (object)c.Contacto ?? DBNull.Value;
        cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = (object)c.Correo ?? DBNull.Value;

        cn.Open();
        return cmd.ExecuteNonQuery();
    }
}

public int Desactivar(int clienteId)
{
    const string sql = @"
UPDATE c
SET
    c.Activo = 0
FROM
    dbo.Clientes AS c
WHERE
    c.ClienteId = @ClienteId
    AND c.Activo = 1;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
        cn.Open();
        return cmd.ExecuteNonQuery();
    }
}
```

| Pieza | Para qué |
|---|---|
| `ExecuteNonQuery()` | Para `INSERT`, `UPDATE` y `DELETE`: regresa **cuántos renglones cambió** |
| `AND c.Activo = 1` | No se edita ni se "elimina" otra vez un cliente inactivo. Si regresa `0`, el cliente no existe o ya estaba eliminado |
| `Desactivar` es un `UPDATE` | La eliminación lógica: `Activo = 0`. `usrCursoWeb` ni siquiera tiene permiso de `DELETE` |

### Negocio: insertar o actualizar

En `Negocio/ClientesNegocio.cs`, en `Guardar`, las dos últimas líneas se convierten en:

```csharp
if (c.ClienteId == 0)
{
    c.ClienteId = datos.Insertar(c);
    return Respuesta.Exito("Cliente guardado.", c);
}

if (datos.Actualizar(c) == 0)
{
    return Respuesta.Error("El cliente no existe o fue eliminado.");
}
return Respuesta.Exito("Cliente actualizado.", c);
```

Y agrega:

```csharp
public Respuesta Eliminar(int clienteId)
{
    if (datos.Desactivar(clienteId) == 0)
    {
        return Respuesta.Error("El cliente no existe o ya fue eliminado.");
    }
    return Respuesta.Exito("Cliente eliminado.");
}
```

La limpieza y la validación no cambian: aplican igual a un cliente nuevo que a uno editado.

### Handler: la acción `eliminar`

En `Handlers/ClientesHandler.ashx.cs`, agrega un `case` antes del `default`:

```csharp
case "eliminar":
    if (context.Request.HttpMethod != "POST")
    {
        context.Response.StatusCode = 405;
        respuesta = Respuesta.Error("Para eliminar se usa POST.");
        break;
    }

    var aEliminar = LeerCuerpo<Cliente>(context);
    respuesta = aEliminar == null
        ? Respuesta.Error("No llegó el cliente a eliminar.")
        : negocio.Eliminar(aEliminar.ClienteId);
    break;
```

`eliminar` va por `POST` porque cambia datos. El body trae solo `{ "clienteId": 3 }`.

## 2. La columna Acciones

📖 [Bootstrap Table: opciones de columna](https://bootstrap-table.com/docs/api/column-options/)

En `inicializarTabla()`, agrega al final de `columns`:

```javascript
{ field: 'clienteId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
```

| Opción | Qué hace | Por defecto | Por qué este valor |
|---|---|---|---|
| `field: 'clienteId'` | El dato del renglón que recibe la columna | Ninguno | La columna no lo muestra, lo usa |
| `align` | Alineación del contenido | Izquierda | `right`: los botones a la derecha |
| `formatter` | Qué HTML pintar en la celda | Ninguno | Los dos botones |
| `events` | Qué hacer al dar clic dentro de la celda | Ninguno | Llamar a `editar` o `eliminar` con el renglón completo |

Y al final de `js/clientes.js`:

```javascript
function formatoAcciones() {
    return '<button type="button" class="btn btn-sm btn-outline-primary btn-editar">Editar</button> ' +
        '<button type="button" class="btn btn-sm btn-outline-danger btn-eliminar">Eliminar</button>';
}

const eventosAcciones = {
    'click .btn-editar': function (e, valor, renglon) {
        editar(renglon);
    },
    'click .btn-eliminar': function (e, valor, renglon) {
        eliminar(renglon);
    }
};
```

- `btn-editar` y `btn-eliminar` no son clases de Bootstrap: son nombres nuestros para que `events` encuentre cada botón.
- `'click .btn-editar'` se lee *"clic en el elemento con clase `btn-editar`"*: el mismo selector de jQuery.
- `renglon` es el objeto completo del renglón, tal como lo mandó el handler.
- `escape: true` escapa los **datos**, no lo que regresa un `formatter`; por eso los botones se pintan como botones.

## 3. Editar

1. Arriba de todo en `js/clientes.js`:
   ```javascript
   let clienteIdEditando = 0;
   ```
   `0` significa *"nuevo"*, igual que en el servidor.
2. En `obtenerCliente`, cambia una línea: `clienteId: 0,` → `clienteId: clienteIdEditando,`.
3. En `Clientes.aspx`, dale un `id` al título del formulario:
   ```html
   <h5 id="lblTituloFormulario" class="card-title">Nuevo cliente</h5>
   ```
4. En `limpiarCampos`, agrega al final:
   ```javascript
   clienteIdEditando = 0;
   $('#lblTituloFormulario').text('Nuevo cliente');
   ```
5. Agrega la función:
   ```javascript
   function editar(c) {
       clienteIdEditando = c.clienteId;
       $('#txtNombre').val(c.nombre);
       $('#txtRFC').val(c.rfc || '');
       $('#txtContacto').val(c.contacto || '');
       $('#txtCorreo').val(c.correo || '');
       $('#lblTituloFormulario').text('Editar cliente');
       ocultarMensaje();
       $('#txtNombre').trigger('focus');
   }
   ```

`c.rfc || ''` pone texto vacío si el valor viene `null`. `enviarCliente()` no se toca: manda el `clienteId` del formulario y el servidor decide si inserta o actualiza.

## 4. Eliminar con `confirm()`

📖 [MDN: `Window.confirm()`](https://developer.mozilla.org/es/docs/Web/API/Window/confirm)

```javascript
function eliminar(c) {
    if (!confirm('¿Eliminar al cliente "' + c.nombre + '"?')) {
        return;
    }

    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ clienteId: c.clienteId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                if (c.clienteId === clienteIdEditando) {
                    limpiarCampos();
                }
                $('#tblClientes').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
```

- `confirm` regresa `true` con *Aceptar* y `false` con *Cancelar*: es el `MessageBox` con Sí/No de WinForms.
- Solo viaja el `clienteId`: es lo único que necesita el servidor.
- Si eliminas el cliente que estás editando, el formulario se limpia.

## 5. Probar

1. **Editar** → el formulario se llena y el título dice *"Editar cliente"*. Cambia algo y **Guardar** → *"Cliente actualizado."*, sin un renglón nuevo.
2. **Limpiar** a medio editar → regresa a *"Nuevo cliente"*.
3. **Eliminar** → el `confirm`. Con *Aceptar*, el renglón desaparece; en SSMS sigue ahí con `Activo = 0`.
4. Elimina el cliente que estás editando → el formulario se limpia.

| Si pasa esto | Revisa |
|---|---|
| Edito y me crea otro cliente | `obtenerCliente` debe mandar `clienteId: clienteIdEditando`, no `0` |
| Los botones no hacen nada | Que la columna tenga `events: eventosAcciones` y que la clase del botón coincida con el selector |
| Al dar clic, la página se recarga | Que los botones del `formatter` tengan `type="button"` |
| `ReferenceError: $ is not defined` | Con Ctrl+U, que `jquery` aparezca antes que `clientes.js`, y que tu `<script>` esté en el `Content` de `Scripts` |

---

> **Bloque 2 · Modales y Familias**

## 6. Modales

📖 [Bootstrap: Modal](https://getbootstrap.com/docs/5.3/components/modal/) · [Bootstrap: Flex](https://getbootstrap.com/docs/5.3/utilities/flex/) · [MDN: parámetros por defecto](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Functions/Default_parameters)

Un modal es una ventana encima de la página que hay que cerrar para seguir, como `ShowDialog()` en WinForms. Lo usamos para el formulario de alta y edición, y para confirmar antes de eliminar. Esta vez el código no solo crece: el formulario **se mueve** al modal.

### `Clientes.aspx`

| | Qué | Por qué |
|---|---|---|
| **Se quita** | La tarjeta (`card`) con el formulario | El formulario se va a un modal |
| **Se quita** | El botón **Limpiar** | Lo reemplaza **Cancelar**, que cierra el modal |
| **Se pone** | El botón **Nuevo cliente** (`btnNuevo`) junto al título | Abre el formulario para un alta |
| **Se pone** | El modal `mdlCliente` con **el mismo formulario** adentro | Mismos `id` y mismo grid; el título `lblTituloFormulario` pasa al encabezado del modal |
| **Se pone** | `divMensajeModal`, dentro del modal | Con el modal abierto, `divMensaje` queda detrás del fondo oscuro |
| **Se pone** | El modal `mdlConfirmar` | Reemplaza al `confirm()` |

Así queda el contenido de `MainContent`:

```html
<div class="d-flex justify-content-between align-items-center mb-4">
    <h1 class="mb-0">Clientes</h1>
    <button type="button" id="btnNuevo" class="btn btn-primary">Nuevo cliente</button>
</div>
<div id="divMensaje" class="alert d-none" role="alert"></div>

<table id="tblClientes"></table>

<!-- Modal: alta y edición -->
<div class="modal fade" id="mdlCliente" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 id="lblTituloFormulario" class="modal-title">Nuevo cliente</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <div id="divMensajeModal" class="alert d-none" role="alert"></div>
                <div class="row g-3">
                    <div class="col-md-6">
                        <label for="txtNombre" class="form-label">Nombre</label>
                        <input type="text" id="txtNombre" class="form-control" maxlength="150" />
                    </div>
                    <div class="col-md-3">
                        <label for="txtRFC" class="form-label">RFC</label>
                        <input type="text" id="txtRFC" class="form-control" maxlength="13" />
                    </div>
                    <div class="col-md-3">
                        <label for="txtContacto" class="form-label">Contacto</label>
                        <input type="text" id="txtContacto" class="form-control" maxlength="100" />
                    </div>
                    <div class="col-md-6">
                        <label for="txtCorreo" class="form-label">Correo</label>
                        <input type="email" id="txtCorreo" class="form-control" maxlength="150" />
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
            </div>
        </div>
    </div>
</div>

<!-- Modal: confirmar eliminación -->
<div class="modal fade" id="mdlConfirmar" tabindex="-1" aria-labelledby="lblTituloConfirmar" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 id="lblTituloConfirmar" class="modal-title">Eliminar cliente</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <p id="lblConfirmar" class="mb-0"></p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" id="btnConfirmarEliminar" class="btn btn-danger">Eliminar</button>
            </div>
        </div>
    </div>
</div>
```

| Pieza | Qué es |
|---|---|
| `d-flex justify-content-between align-items-center` | Pone el título y el botón en una fila, cada uno en un extremo y centrados verticalmente |
| `modal fade` | El contenedor del modal; `fade` le da la animación |
| `modal-dialog` / `modal-lg` | La ventana; `modal-lg` la hace más ancha |
| `modal-header` / `modal-body` / `modal-footer` | Título, contenido y botones |
| `data-bs-dismiss="modal"` | Cierra el modal sin JavaScript: la **X** y **Cancelar** |
| `tabindex`, `aria-*` | Accesibilidad; vienen así en la documentación y se copian tal cual |

### `js/clientes.js`

| | Qué | Por qué |
|---|---|---|
| **Se pone** arriba | `clienteAEliminar`, `modalCliente`, `modalConfirmar` | El cliente por eliminar y los dos modales |
| **Cambia** el arranque | Crea los modales, conecta `btnNuevo` y `btnConfirmarEliminar`, ya no conecta `btnLimpiar`, y pone el foco al terminar de abrir | |
| **Cambia** `mostrarMensaje` y `ocultarMensaje` | Reciben un tercer parámetro opcional, `contenedor` | Sirven para la página y para el modal; las llamadas anteriores no cambian |
| **Se pone** | `nuevo()` | Abre el modal vacío |
| **Cambia** `guardar` y `enviarCliente` | Los errores van al modal; si todo sale bien, el modal se cierra | Corriges sin perder lo que escribiste |
| **Cambia** `limpiarCampos` | Ya no pone el foco | Lo hace el evento `shown.bs.modal` |
| **Se quita** | `limpiar()` | Ya no existe el botón Limpiar |
| **Cambia** `editar` | Abre el modal | El formulario vive en el modal |
| **Cambia** `eliminar` y **se pone** `confirmarEliminar` | `eliminar` solo pregunta; `confirmarEliminar` hace el `$.ajax` | Ver la nota de abajo |

El arranque y los mensajes:

```javascript
let clienteIdEditando = 0;
let clienteAEliminar = null;
let modalCliente;
let modalConfirmar;

$(function () {
    modalCliente = new bootstrap.Modal('#mdlCliente');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#mdlCliente').on('shown.bs.modal', function () {
        $('#txtNombre').trigger('focus');
    });

    inicializarTabla();
});

function mostrarMensaje(texto, tipo, contenedor = '#divMensaje') {
    $(contenedor)
        .removeClass('d-none alert-success alert-danger')
        .addClass('alert-' + tipo)
        .text(texto);
}

function ocultarMensaje(contenedor = '#divMensaje') {
    $(contenedor).addClass('d-none').empty();
}

function nuevo() {
    limpiarCampos();
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalCliente.show();
}
```

Guardar con los errores en el modal:

```javascript
function guardar() {
    ocultarMensaje('#divMensajeModal');
    const cliente = obtenerCliente();
    const errores = validarCliente(cliente);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarCliente(cliente);
}
```

En `enviarCliente`, solo cambian `.done` y `.fail`:

```javascript
.done(function (r) {
    if (!r.ok) {
        mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
        return;
    }
    modalCliente.hide();
    mostrarMensaje(r.mensaje, 'success');
    limpiarCampos();
    $('#tblClientes').bootstrapTable('refresh');
})
.fail(function (xhr) {
    mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
})
```

`limpiarCampos` y `editar`:

```javascript
function limpiarCampos() {
    $('#txtNombre, #txtRFC, #txtContacto, #txtCorreo').val('');
    clienteIdEditando = 0;
    $('#lblTituloFormulario').text('Nuevo cliente');
}

function editar(c) {
    clienteIdEditando = c.clienteId;
    $('#txtNombre').val(c.nombre);
    $('#txtRFC').val(c.rfc || '');
    $('#txtContacto').val(c.contacto || '');
    $('#txtCorreo').val(c.correo || '');
    $('#lblTituloFormulario').text('Editar cliente');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalCliente.show();
}
```

Eliminar con el modal de confirmación:

```javascript
function eliminar(c) {
    clienteAEliminar = c;
    $('#lblConfirmar').text('¿Eliminar al cliente "' + c.nombre + '"?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ clienteId: clienteAEliminar.clienteId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblClientes').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
```

| Pieza | Para qué |
|---|---|
| `new bootstrap.Modal('#mdlCliente')` | Crea el objeto del modal una vez; después solo se usan `.show()` y `.hide()` |
| `shown.bs.modal` | Evento de Bootstrap cuando el modal **terminó** de abrirse; antes no se puede poner el foco |
| `contenedor = '#divMensaje'` | Parámetro por defecto: si no lo mandas, usa `'#divMensaje'` |

> **`confirm()` contra un modal.** `confirm()` detiene el código hasta que respondes; un modal **no**: se abre y el código sigue. Por eso eliminar se parte en dos funciones: `eliminar` pregunta y `confirmarEliminar` actúa. Mientras tanto, el cliente se guarda en `clienteAEliminar`.

No cambian `obtenerCliente`, `validarCliente`, `inicializarTabla`, `formatoFecha`, `formatoAcciones`, `eventosAcciones` ni nada del servidor.

### Probar

1. **Nuevo cliente** → modal vacío con el foco en *Nombre*. Guarda uno válido → el modal se cierra, mensaje verde y la tabla se refresca.
2. **Nuevo cliente** con el nombre vacío → el error aparece dentro del modal.
3. **Editar** → el modal se abre lleno con *"Editar cliente"*. Guarda → *"Cliente actualizado."*
4. **Cancelar** o la **X** → se cierra sin guardar.
5. **Eliminar** → la confirmación con el nombre. *Eliminar* → el renglón desaparece.

## 7. La tabla `Familias` en SQL Server

📖 [Restricciones de clave principal y clave externa](https://learn.microsoft.com/es-es/sql/relational-databases/tables/primary-and-foreign-key-constraints) · [Combinaciones (JOIN)](https://learn.microsoft.com/es-es/sql/relational-databases/performance/joins)

Escríbela a mano en SSMS, con tu usuario de administrador. Si algo no te funciona, compara con [`sql/01-esquema.sql`](../sql/01-esquema.sql).

```sql
USE CursoWeb;
GO

CREATE TABLE dbo.Familias
(
      FamiliaId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Familias PRIMARY KEY
    , ClienteId      INT               NOT NULL CONSTRAINT FK_Familias_Clientes REFERENCES dbo.Clientes (ClienteId)
    , Nombre         NVARCHAR(100)     NOT NULL
    , Descripcion    NVARCHAR(250)     NULL
    , Activo         BIT               NOT NULL CONSTRAINT DF_Familias_Activo DEFAULT (1)
    , FechaAlta      DATETIME2(0)      NOT NULL CONSTRAINT DF_Familias_FechaAlta DEFAULT (SYSDATETIME())
);
GO
```

Casi todo es igual a `Clientes`. Lo nuevo es la **llave foránea**:

| Pieza | Para qué |
|---|---|
| `ClienteId INT NOT NULL` | Cada familia pertenece a un cliente, y es obligatorio |
| `CONSTRAINT FK_Familias_Clientes` | El nombre de la regla, con la convención `FK_TablaHija_TablaPadre`. Aparece en el mensaje de error |
| `REFERENCES dbo.Clientes (ClienteId)` | El valor de `ClienteId` tiene que existir en `dbo.Clientes` |

Prueba la relación con dos `ClienteId` que existan en tu tabla:

```sql
INSERT INTO dbo.Familias (ClienteId, Nombre, Descripcion)
VALUES
      (1, N'Cerraduras', N'Cerraduras de puerta delanteras y traseras')
    , (1, N'Bisagras',   NULL);

SELECT
      c.Nombre      AS Cliente
    , f.Nombre      AS Familia
    , f.Descripcion AS Descripcion
FROM
    dbo.Familias AS f
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId;
```

`Familias` solo guarda el número del cliente; el `JOIN` une las dos tablas para ver su nombre.

Y ahora una familia de un cliente que no existe:

```sql
INSERT INTO dbo.Familias (ClienteId, Nombre) VALUES (999, N'Familia huérfana');
```

Falla con:

```
Msg 547, Level 16, State 0
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Familias_Clientes".
The conflict occurred in database "CursoWeb", table "dbo.Clientes", column 'ClienteId'.
```

Es lo correcto: la base de datos no deja guardar una familia de un cliente que no existe.

> ¿Y si el cliente existe pero está eliminado (`Activo = 0`)? La llave foránea **sí lo permite**, porque el cliente sigue en la tabla. Esa regla la cuida la capa de Negocio.

`usrCursoWeb` ya puede usar la tabla nueva: el permiso del día 2 es para todo el esquema `dbo`.

## 8. Catálogo de Familias

📖 [Bootstrap: Select](https://getbootstrap.com/docs/5.3/forms/select/) · [MDN: `<select>`](https://developer.mozilla.org/es/docs/Web/HTML/Reference/Elements/select) · [MDN: `<textarea>`](https://developer.mozilla.org/es/docs/Web/HTML/Reference/Elements/textarea) · [jQuery: `.append()`](https://api.jquery.com/append/)

Familias es un catálogo igual a Clientes: misma arquitectura, mismas capas, mismos pasos. Lo construyes **copiando cada archivo de Clientes y cambiando lo que cambia**. Si te atoras, los archivos completos están en [`ejemplos/familias/`](ejemplos/familias/).

### El mapa

| Archivo nuevo | Se copia de | Qué cambia |
|---|---|---|
| `Modelos/Familia.cs` | `Cliente.cs` | Las propiedades, más `ClienteNombre` (viene del `JOIN`, solo para mostrar) |
| `Datos/FamiliasDatos.cs` | `ClientesDatos.cs` | El SQL. `Listar` hace el `JOIN` con Clientes |
| `Negocio/FamiliasNegocio.cs` | `ClientesNegocio.cs` | Las reglas: cliente obligatorio y largos 100 y 250 |
| `Handlers/FamiliasHandler.ashx` | `ClientesHandler` | Solo nombres |
| `Familias.aspx` | `Clientes.aspx` | Los campos: un `<select>` para el cliente y un `<textarea>` para la descripción |
| `js/familias.js` | `clientes.js` | Los nombres, más `cargarClientes()` para llenar el combo |

> **Truco:** en cada archivo copiado, **Ctrl+H** para reemplazar `Cliente` → `Familia` y `cliente` → `familia` respetando mayúsculas. Después revisa a mano: Familias **también** tiene un `ClienteId`, y el reemplazo automático lo habrá cambiado donde no debía.

### 1. El modelo

**Archivo nuevo:** clic derecho en la carpeta `Modelos` → *Agregar → Clase* → `Familia.cs`. Reemplaza todo su contenido por:
```csharp
using System;

namespace CursoWeb.Modelos
{
    public class Familia
    {
        public int FamiliaId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
```

`ClienteNombre` no es columna de la tabla: llega del `JOIN` y es lo que muestra la tabla de la página.

### 2. Datos

**Archivo nuevo:** clic derecho en la carpeta `Datos` → *Agregar → Clase* → `FamiliasDatos.cs`. Reemplaza todo su contenido por:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class FamiliasDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        public List<Familia> Listar()
        {
            const string sql = @"
SELECT
      f.FamiliaId     AS FamiliaId
    , f.ClienteId     AS ClienteId
    , c.Nombre        AS ClienteNombre
    , f.Nombre        AS Nombre
    , f.Descripcion   AS Descripcion
    , f.Activo        AS Activo
    , f.FechaAlta     AS FechaAlta
FROM
    dbo.Familias AS f
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    f.Activo = 1
ORDER BY
      c.Nombre
    , f.Nombre;";

            var lista = new List<Familia>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Familia
                        {
                            // NOT NULL en la tabla: se leen con su tipo exacto
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                            // Acepta NULL
                            Descripcion = dr["Descripcion"] as string,
                            // NOT NULL
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public int Insertar(Familia f)
        {
            const string sql = @"
INSERT INTO dbo.Familias (ClienteId, Nombre, Descripcion)
VALUES (@ClienteId, @Nombre, @Descripcion);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS FamiliaId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = f.ClienteId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = f.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = (object)f.Descripcion ?? DBNull.Value;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int Actualizar(Familia f)
        {
            const string sql = @"
UPDATE f
SET
      f.ClienteId     = @ClienteId
    , f.Nombre        = @Nombre
    , f.Descripcion   = @Descripcion
FROM
    dbo.Familias AS f
WHERE
    f.FamiliaId = @FamiliaId
    AND f.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = f.FamiliaId;
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = f.ClienteId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = f.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = (object)f.Descripcion ?? DBNull.Value;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int familiaId)
        {
            const string sql = @"
UPDATE f
SET
    f.Activo = 0
FROM
    dbo.Familias AS f
WHERE
    f.FamiliaId = @FamiliaId
    AND f.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = familiaId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
```

| Método | Qué cambia respecto a `ClientesDatos.cs` |
|---|---|
| `Listar` | El `SELECT` une las dos tablas con `INNER JOIN` y trae `c.Nombre AS ClienteNombre`. Solo `Descripcion` se lee con `as string`, porque es la única que acepta `NULL` |
| `Insertar` | Los parámetros: `@ClienteId Int`, `@Nombre NVarChar 100`, `@Descripcion NVarChar 250` |
| `Actualizar` | El `WHERE` usa `FamiliaId`, y también se actualiza `ClienteId`: una familia se puede pasar a otro cliente |
| `Desactivar` | `FamiliaId` en lugar de `ClienteId`; lo demás, igual |

La estructura (`using`, parámetros, `ExecuteReader`, `ExecuteScalar`, `ExecuteNonQuery`) es **exactamente la misma** que en Clientes.

### 3. Negocio

**Archivo nuevo:** clic derecho en la carpeta `Negocio` → *Agregar → Clase* → `FamiliasNegocio.cs`. Reemplaza todo su contenido por:

```csharp
using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class FamiliasNegocio
    {
        private readonly FamiliasDatos datos = new FamiliasDatos();

        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
        }

        public Respuesta Guardar(Familia f)
        {
            Normalizar(f);

            var errores = Validar(f);
            if (errores.Count > 0)
            {
                return Respuesta.Error(string.Join(" ", errores));
            }

            if (f.FamiliaId == 0)
            {
                f.FamiliaId = datos.Insertar(f);
                return Respuesta.Exito("Familia guardada.", f);
            }

            if (datos.Actualizar(f) == 0)
            {
                return Respuesta.Error("La familia no existe o fue eliminada.");
            }
            return Respuesta.Exito("Familia actualizada.", f);
        }

        public Respuesta Eliminar(int familiaId)
        {
            if (datos.Desactivar(familiaId) == 0)
            {
                return Respuesta.Error("La familia no existe o ya fue eliminada.");
            }
            return Respuesta.Exito("Familia eliminada.");
        }

        private static void Normalizar(Familia f)
        {
            f.Nombre = Limpiar(f.Nombre);
            f.Descripcion = Limpiar(f.Descripcion);
        }

        private static string Limpiar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            return valor.Trim();
        }

        private static List<string> Validar(Familia f)
        {
            var errores = new List<string>();

            if (f.ClienteId <= 0)
            {
                errores.Add("Selecciona un cliente.");
            }

            if (f.Nombre == null)
            {
                errores.Add("El nombre es obligatorio.");
            }
            else if (f.Nombre.Length > 100)
            {
                errores.Add("El nombre no puede pasar de 100 caracteres.");
            }

            if (f.Descripcion != null && f.Descripcion.Length > 250)
            {
                errores.Add("La descripción no puede pasar de 250 caracteres.");
            }

            return errores;
        }
    }
}
```

| Método | Qué cambia respecto a `ClientesNegocio.cs` |
|---|---|
| `Listar`, `Guardar`, `Eliminar`, `Limpiar` | Iguales, con `Familia` y `FamiliaId`, y los mensajes en femenino |
| `Normalizar` | Solo nombre y descripción |
| `Validar` | **Cliente obligatorio** (`ClienteId <= 0`), nombre (100) y descripción (250) |

Si no se eligió cliente, llega `0`. La llave foránea también lo rechazaría, pero con un mensaje de SQL en inglés.

### 4. Handler

**Archivo nuevo:** clic derecho en la carpeta `Handlers` → *Agregar → Nuevo elemento →* **Controlador genérico** → `FamiliasHandler.ashx`. Visual Studio crea dos archivos: `FamiliasHandler.ashx`, que **no se toca**, y `FamiliasHandler.ashx.cs`. Reemplaza todo el contenido de este último por:

```csharp
using System;
using System.IO;
using System.Web;
using CursoWeb.Modelos;
using CursoWeb.Negocio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CursoWeb.Handlers
{
    public class FamiliasHandler : IHttpHandler
    {
        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            Respuesta respuesta;

            try
            {
                var negocio = new FamiliasNegocio();
                switch (context.Request.QueryString["accion"])
                {
                    case "listar":
                        respuesta = negocio.Listar();
                        break;

                    case "guardar":
                        if (context.Request.HttpMethod != "POST")
                        {
                            context.Response.StatusCode = 405;
                            respuesta = Respuesta.Error("Para guardar se usa POST.");
                            break;
                        }

                        var familia = LeerCuerpo<Familia>(context);
                        respuesta = familia == null
                            ? Respuesta.Error("No llegaron los datos de la familia.")
                            : negocio.Guardar(familia);
                        break;

                    case "eliminar":
                        if (context.Request.HttpMethod != "POST")
                        {
                            context.Response.StatusCode = 405;
                            respuesta = Respuesta.Error("Para eliminar se usa POST.");
                            break;
                        }

                        var aEliminar = LeerCuerpo<Familia>(context);
                        respuesta = aEliminar == null
                            ? Respuesta.Error("No llegó la familia a eliminar.")
                            : negocio.Eliminar(aEliminar.FamiliaId);
                        break;

                    default:
                        respuesta = Respuesta.Error("Acción no válida.");
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = Respuesta.Error(context.IsDebuggingEnabled
                    ? ex.Message
                    : "Ocurrió un error en el servidor.");
            }

            context.Response.Write(JsonConvert.SerializeObject(respuesta, Json));
        }

        private static T LeerCuerpo<T>(HttpContext context)
        {
            using (var lector = new StreamReader(context.Request.InputStream))
            {
                return JsonConvert.DeserializeObject<T>(lector.ReadToEnd(), Json);
            }
        }

        public bool IsReusable => false;
    }
}
```

Respecto a `ClientesHandler.ashx.cs` **solo cambian nombres**: `FamiliasHandler`, `FamiliasNegocio`, `LeerCuerpo<Familia>`, `aEliminar.FamiliaId` y los mensajes. Pruébalo: `Handlers/FamiliasHandler.ashx?accion=listar`.

### 5. La página

`Familias.aspx` ya existe desde el día 1 (solo tiene el `<h1>`). Reemplaza **todo el archivo** por:

```aspx
<%@ Page Title="Familias" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Familias.aspx.cs" Inherits="CursoWeb.Familias" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Familias</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nueva familia</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <table id="tblFamilias"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlFamilia" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nueva familia</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div id="divMensajeModal" class="alert d-none" role="alert"></div>
                    <div class="row g-3">
                        <div class="col-12">
                            <label for="cboCliente" class="form-label">Cliente</label>
                            <select id="cboCliente" class="form-select"></select>
                        </div>
                        <div class="col-12">
                            <label for="txtNombre" class="form-label">Nombre</label>
                            <input type="text" id="txtNombre" class="form-control" maxlength="100" />
                        </div>
                        <div class="col-12">
                            <label for="txtDescripcion" class="form-label">Descripción</label>
                            <textarea id="txtDescripcion" class="form-control" rows="3" maxlength="250"></textarea>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" id="btnGuardar" class="btn btn-primary">Guardar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal: confirmar eliminación -->
    <div class="modal fade" id="mdlConfirmar" tabindex="-1" aria-labelledby="lblTituloConfirmar" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar familia</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <p id="lblConfirmar" class="mb-0"></p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" id="btnConfirmarEliminar" class="btn btn-danger">Eliminar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <script src="js/familias.js"></script>
</asp:Content>
```

Es el mismo esqueleto de `Clientes.aspx`. Lo que cambia:

| Parte | En `Clientes.aspx` | En `Familias.aspx` |
|---|---|---|
| Títulos y textos | *Clientes*, *Nuevo cliente* | *Familias*, *Nueva familia* |
| `id` de la tabla y del modal | `tblClientes`, `mdlCliente` | `tblFamilias`, `mdlFamilia` |
| Ancho del modal | `modal-lg` | Sin `modal-lg`: tres campos, uno por renglón |
| Campos | Cuatro `<input>` | Un `<select>` para el cliente, un `<input>` y un `<textarea>` |
| Script | `js/clientes.js` | `js/familias.js` |

- El `<select>` va vacío: las opciones las pone JavaScript. Para `<select>` la clase de Bootstrap es `form-select`.
- `<textarea>` es texto de varias líneas; `rows` es la altura inicial.

### 6. `familias.js` y el combo

**Archivo nuevo:** clic derecho en la carpeta `js` → *Agregar → Nuevo elemento → Archivo JavaScript* → `familias.js`. Contenido completo:

```javascript
let familiaIdEditando = 0;
let familiaAEliminar = null;
let modalFamilia;
let modalConfirmar;

$(function () {
    modalFamilia = new bootstrap.Modal('#mdlFamilia');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#mdlFamilia').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    inicializarTabla();
});

// Llena el combo con los clientes activos, reutilizando el handler de Clientes
function cargarClientes() {
    $.ajax({
        url: 'Handlers/ClientesHandler.ashx?accion=listar',
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            const $combo = $('#cboCliente');
            $combo.empty();
            $combo.append($('<option>').val('').text('Selecciona un cliente'));
            if (r.ok) {
                r.datos.forEach(function (c) {
                    $combo.append($('<option>').val(c.clienteId).text(c.nombre));
                });
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar los clientes (' + xhr.status + ').', 'danger');
        });
}

function obtenerFamilia() {
    return {
        familiaId: familiaIdEditando,
        clienteId: Number($('#cboCliente').val()) || 0,
        nombre: $('#txtNombre').val().trim(),
        descripcion: $('#txtDescripcion').val().trim()
    };
}

function validarFamilia(f) {
    const errores = [];
    if (!f.clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!f.nombre) {
        errores.push('El nombre es obligatorio.');
    }
    return errores;
}

function mostrarMensaje(texto, tipo, contenedor = '#divMensaje') {
    $(contenedor)
        .removeClass('d-none alert-success alert-danger')
        .addClass('alert-' + tipo)
        .text(texto);
}

function ocultarMensaje(contenedor = '#divMensaje') {
    $(contenedor).addClass('d-none').empty();
}

function nuevo() {
    limpiarCampos();
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalFamilia.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const familia = obtenerFamilia();
    const errores = validarFamilia(familia);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarFamilia(familia);
}

function enviarFamilia(familia) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(familia),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalFamilia.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblFamilias').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#cboCliente, #txtNombre, #txtDescripcion').val('');
    familiaIdEditando = 0;
    $('#lblTituloFormulario').text('Nueva familia');
}

function inicializarTabla() {
    $('#tblFamilias').bootstrapTable({
        url: 'Handlers/FamiliasHandler.ashx?accion=listar',
        responseHandler: function (r) {
            return r.ok ? r.datos : [];
        },
        classes: 'table table-striped table-hover',
        escape: true,
        search: true,
        pagination: true,
        pageSize: 10,
        locale: 'es-MX',
        columns: [
            { field: 'clienteNombre', title: 'Cliente', sortable: true },
            { field: 'nombre', title: 'Familia', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha },
            { field: 'familiaId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
        ]
    });
}

function formatoFecha(valor) {
    return valor ? new Date(valor).toLocaleDateString('es-MX') : '';
}

function formatoAcciones() {
    return '<button type="button" class="btn btn-sm btn-outline-primary btn-editar">Editar</button> ' +
        '<button type="button" class="btn btn-sm btn-outline-danger btn-eliminar">Eliminar</button>';
}

const eventosAcciones = {
    'click .btn-editar': function (e, valor, renglon) {
        editar(renglon);
    },
    'click .btn-eliminar': function (e, valor, renglon) {
        eliminar(renglon);
    }
};

function editar(f) {
    familiaIdEditando = f.familiaId;
    $('#cboCliente').val(f.clienteId);
    $('#txtNombre').val(f.nombre);
    $('#txtDescripcion').val(f.descripcion || '');
    $('#lblTituloFormulario').text('Editar familia');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalFamilia.show();
}

function eliminar(f) {
    familiaAEliminar = f;
    $('#lblConfirmar').text('¿Eliminar la familia "' + f.nombre + '" de ' + f.clienteNombre + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ familiaId: familiaAEliminar.familiaId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblFamilias').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
```

| Función | Qué cambia respecto a `clientes.js` |
|---|---|
| Arranque | El foco va al combo, y llama a **`cargarClientes()`** antes de `inicializarTabla()` |
| `cargarClientes` | **Nueva**: llena el combo (ver abajo) |
| `obtenerFamilia` | Lee el combo con `Number(...)`, el nombre y la descripción |
| `validarFamilia` | Cliente elegido y nombre obligatorio |
| `inicializarTabla` | Columnas `clienteNombre`, `nombre`, `descripcion`, `fechaAlta`; Acciones con `familiaId` |
| `editar` | También selecciona el cliente en el combo: `$('#cboCliente').val(f.clienteId)` |
| `eliminar` | El texto incluye el cliente: *"¿Eliminar la familia X de Cliente Y?"* |
| Las demás | Iguales, con `familia` en lugar de `cliente` y `FamiliasHandler` |

**Lo nuevo: `cargarClientes()`**

| Pieza | Para qué |
|---|---|
| `ClientesHandler.ashx?accion=listar` | Se **reutiliza** el handler de Clientes: un handler sirve a más de una página |
| `$combo.empty()` | Vacía el combo antes de llenarlo |
| `$('<option>').val(...).text(...)` | Crea cada opción. Con `.text()`, por XSS |
| La primera opción con `val('')` | Obliga a elegir un cliente |

En `obtenerFamilia`, el valor de un `<select>` siempre es texto (`'3'`); `Number` lo convierte, y si no se eligió nada queda `0`.

### Probar

1. La tabla muestra las familias con la columna **Cliente**.
2. **Nueva familia** → el combo trae los clientes. Guarda una → aparece en la tabla.
3. Sin elegir cliente → *"Selecciona un cliente."*
4. **Editar** → el combo aparece con el cliente correcto.
5. **Eliminar** → *"¿Eliminar la familia X de Cliente Y?"*

| Si pasa esto | Revisa |
|---|---|
| Visual Studio no reconoce una clase nueva | Que el archivo esté incluido en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*) |
| El combo sale vacío | Que `cargarClientes()` se llame en el arranque, y la URL en Network |
| Acentos raros (`DescripciÃ³n`) | Guarda el `.aspx` desde Visual Studio con *UTF-8 con firma* |
