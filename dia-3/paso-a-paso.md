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
