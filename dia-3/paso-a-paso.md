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
