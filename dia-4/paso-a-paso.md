# Paso a paso · Día 4

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Requerimiento, diseño y listado de Piezas**

Hoy construimos un catálogo nuevo: **Piezas**. Cuelga de dos niveles (Cliente → Familia → Pieza) y usa la misma arquitectura de Clientes y Familias. Primero lo diseñamos entre todos, después creamos la tabla a mano y construimos el listado. Tres ideas para el día:

- **El diseño va antes del código.** Decidimos tabla, capas y pantalla con el requerimiento en la mano, y hasta entonces escribimos código.
- **Piezas se construye como Familias.** Misma arquitectura, otros nombres y otro SQL. Lo nuevo es la relación de dos niveles y el número de parte único.
- **Hoy solo el listado.** Ver las piezas llegar al navegador es la prueba de que tabla, Datos, Negocio y handler hablan entre sí. El guardado y la pantalla van en el siguiente bloque.

## 1. El requerimiento

La planta necesita un **catálogo maestro de piezas**. Cada pieza pertenece a una **familia**, y cada familia a un **cliente**.

- De cada pieza se captura su **número de parte** y una **descripción**. Ambos son obligatorios.
- El **número de parte no se puede repetir**.
- Una pieza no se borra: se **elimina de forma lógica** (`Activo = 0`), igual que los otros catálogos.
- La pantalla muestra una tabla con búsqueda, y un formulario en modal para dar de alta y editar.
- Al capturar una pieza se elige primero el **cliente** y después la **familia**, que solo muestra las familias de ese cliente.

Antes de seguir, piensa: ¿qué columnas tendría la tabla?, ¿cómo se relaciona con el cliente?, ¿qué evita que se repita el número de parte? Lo comparamos con el diseño siguiente.

## 2. El diseño

| Decisión | Resultado | Por qué |
|---|---|---|
| **Tabla** | `PiezaId`, `FamiliaId`, `NumeroParte`, `Descripcion`, `Activo`, `FechaAlta` | Las mismas columnas de control que Clientes y Familias (`Activo`, `FechaAlta`), más lo que identifica a una pieza |
| **Relación con el cliente** | La pieza guarda solo `FamiliaId`; **no** lleva `ClienteId` | El cliente ya se conoce a través de la familia. Si la pieza guardara también `ClienteId`, un dato podría contradecir al otro (una pieza de una familia del cliente A marcada como del cliente B) |
| **Número de parte único** | Restricción `UNIQUE` en la base, y más adelante una revisión en Negocio | La base es la última defensa: aunque falle todo lo demás, no deja guardar un duplicado. Pero su mensaje es técnico y en inglés; Negocio revisa primero y le dice al usuario algo que entienda. Es el mismo criterio de la llave foránea del día 3 |
| **Piezas eliminadas** | Un número de parte eliminado sigue ocupado | El `UNIQUE` no mira la columna `Activo`. Es lo más simple, y es una decisión de este curso básico |
| **Capas** | `Pieza`, `PiezasDatos`, `PiezasNegocio`, `PiezasHandler` | Las mismas cuatro de siempre: cada capa tiene una sola responsabilidad |
| **Handler** | Acciones `listar`, `guardar` y `eliminar`. Hoy construimos `listar` | Con `listar` ya se comprueba todo el camino, de la tabla al navegador, sin tener que construir antes la pantalla |
| **Pantalla** | `Piezas.aspx`, con dos combos dependientes: Cliente → Familia | Una pieza se ubica por su familia, y las familias dependen del cliente: el segundo combo debe cambiar cuando cambia el primero |

Qué archivo se copia de cuál:

| Archivo nuevo | Se copia de | Qué cambia |
|---|---|---|
| `Modelos/Pieza.cs` | `Familia.cs` | Las propiedades |
| `Datos/PiezasDatos.cs` | `FamiliasDatos.cs` | El SQL: `Listar` hace dos `JOIN` |
| `Negocio/PiezasNegocio.cs` | `FamiliasNegocio.cs` | Hoy solo `Listar` |
| `Handlers/PiezasHandler.ashx` (+ `.cs`) | `FamiliasHandler` | Los nombres, y hoy solo `listar` |

## 3. SQL: la tabla `Piezas`

📖 [CREATE TABLE](https://learn.microsoft.com/es-es/sql/t-sql/statements/create-table-transact-sql) · [Restricciones de clave principal y clave externa](https://learn.microsoft.com/es-es/sql/relational-databases/tables/primary-and-foreign-key-constraints) · [Restricciones UNIQUE y CHECK](https://learn.microsoft.com/es-es/sql/relational-databases/tables/unique-constraints-and-check-constraints) · [char y varchar](https://learn.microsoft.com/es-es/sql/t-sql/data-types/char-and-varchar-transact-sql) · [nchar y nvarchar](https://learn.microsoft.com/es-es/sql/t-sql/data-types/nchar-and-nvarchar-transact-sql) · [Combinaciones (JOIN)](https://learn.microsoft.com/es-es/sql/relational-databases/performance/joins)

Primero intenta escribirla tú, a partir del diseño. Se ejecuta en SSMS con tu usuario de administrador (crear tablas no lo puede hacer `usrCursoWeb`). Si algo no te funciona, compara con [`sql/01-esquema.sql`](../sql/01-esquema.sql).

```sql
USE CursoWeb;
GO

CREATE TABLE dbo.Piezas
(
      PiezaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Piezas PRIMARY KEY
    , FamiliaId      INT               NOT NULL CONSTRAINT FK_Piezas_Familias REFERENCES dbo.Familias (FamiliaId)
    , NumeroParte    VARCHAR(50)       NOT NULL
    , Descripcion    NVARCHAR(250)     NOT NULL
    , Activo         BIT               NOT NULL CONSTRAINT DF_Piezas_Activo DEFAULT (1)
    , FechaAlta      DATETIME2(0)      NOT NULL CONSTRAINT DF_Piezas_FechaAlta DEFAULT (SYSDATETIME())
    , CONSTRAINT UQ_Piezas_NumeroParte UNIQUE (NumeroParte)
);
GO
```

Casi todo es igual a `Familias`. Lo que cambia:

| Pieza | Qué hace | Por qué este valor |
|---|---|---|
| `FamiliaId INT NOT NULL` + `FK_Piezas_Familias REFERENCES dbo.Familias` | La pieza apunta a su familia, y el valor tiene que existir en `Familias` | Es la relación de segundo nivel. `NOT NULL` porque no hay piezas sin familia. El nombre sigue la convención `FK_TablaHija_TablaPadre` y aparece en los mensajes de error |
| `NumeroParte VARCHAR(50)` | Texto de hasta 50 caracteres | Un número de parte lleva letras, números y guiones; no necesita todo Unicode, igual que el `RFC` de Clientes. Con `VARCHAR` **no** se escribe la `N` en los `INSERT` |
| `Descripcion NVARCHAR(250) NOT NULL` | Texto Unicode de hasta 250 | Es texto libre con acentos, como los nombres. A diferencia de la descripción de Familias, aquí es obligatoria |
| `CONSTRAINT UQ_Piezas_NumeroParte UNIQUE (NumeroParte)` | No permite dos filas con el mismo número de parte | Es lo nuevo del día. Va al final porque es una restricción de tabla sobre una columna ya definida. El nombre sigue la convención `UQ_Tabla_Columna` y aparece en el mensaje de error |

> Recuerda la regla del día 2: **con columnas `NVARCHAR`, la `N` siempre** en lo que escribas a mano (`N'...'`); con `VARCHAR`, sin `N`.

Inserta dos piezas con un `FamiliaId` que exista en tu tabla y consúltalas con los dos `JOIN`:

```sql
INSERT INTO dbo.Piezas (FamiliaId, NumeroParte, Descripcion)
VALUES
      (1, 'PZ-0001', N'Pieza de prueba uno')
    , (1, 'PZ-0002', N'Pieza de prueba dos');

SELECT
      c.Nombre        AS Cliente
    , f.Nombre        AS Familia
    , p.NumeroParte   AS NumeroParte
    , p.Descripcion   AS Pieza
FROM
    dbo.Piezas AS p
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId;
```

`Piezas` solo guarda el número de la familia. Para llegar al cliente hay que pasar por `Familias`: dos `JOIN` encadenados, cada uno con la llave que une a las dos tablas. Es exactamente lo que hará la capa de Datos.

Ahora intenta repetir un número de parte:

```sql
INSERT INTO dbo.Piezas (FamiliaId, NumeroParte, Descripcion)
VALUES (1, 'PZ-0001', N'Número de parte repetido');
```

SQL Server rechaza la fila, y su mensaje menciona la restricción `UQ_Piezas_NumeroParte`. Es lo correcto: por eso conviene ponerle nombre a las restricciones.

> ¿Y si la pieza `PZ-0001` estuviera eliminada (`Activo = 0`)? El `UNIQUE` **tampoco** deja repetir su número: no mira `Activo`. Lo tendremos en cuenta al escribir el mensaje de Negocio.

`usrCursoWeb` ya puede usar la tabla nueva: el permiso del día 2 es para todo el esquema `dbo`.

## 4. Servidor: el listado

Hoy solo construimos lo necesario para ver las piezas llegar al navegador. Cada archivo se copia de su equivalente de Familias: **la estructura es la misma**, solo cambian los nombres y el SQL.

### 1. El modelo

**Archivo nuevo:** clic derecho en la carpeta `Modelos` → *Agregar → Clase* → `Pieza.cs`. Reemplaza todo su contenido por:

```csharp
using System;

namespace CursoWeb.Modelos
{
    public class Pieza
    {
        public int PiezaId { get; set; }
        public int FamiliaId { get; set; }
        public string FamiliaNombre { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string NumeroParte { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
```

| `Familia.cs` | `Pieza.cs` |
|---|---|
| `FamiliaId`, `Nombre`, `Descripcion` | `PiezaId`, `NumeroParte`, `Descripcion` |
| `ClienteId`, `ClienteNombre` | Se quedan; se agregan `FamiliaId` y `FamiliaNombre` |

Una propiedad por columna, con su tipo de C#: `INT` → `int`, `VARCHAR` y `NVARCHAR` → `string`, `BIT` → `bool`, `DATETIME2` → `DateTime`. `FamiliaNombre`, `ClienteId` y `ClienteNombre` no son columnas de `Piezas`: llegan de los `JOIN`. Los pedimos desde ahora porque en el siguiente bloque, al editar una pieza, hay que seleccionar su cliente y su familia en los dos combos.

### 2. Datos

📖 [SqlConnection](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlconnection) · [SqlCommand](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand) · [SqlDataReader](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqldatareader) · [SqlDataReader.GetOrdinal](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqldatareader.getordinal) · [WHERE](https://learn.microsoft.com/es-es/sql/t-sql/queries/where-transact-sql) · [ORDER BY](https://learn.microsoft.com/es-es/sql/t-sql/queries/select-order-by-clause-transact-sql) · [Combinaciones (JOIN)](https://learn.microsoft.com/es-es/sql/relational-databases/performance/joins)

**Archivo nuevo:** clic derecho en la carpeta `Datos` → *Agregar → Clase* → `PiezasDatos.cs`. Reemplaza todo su contenido por:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class PiezasDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        public List<Pieza> Listar()
        {
            const string sql = @"
SELECT
      p.PiezaId       AS PiezaId
    , p.FamiliaId     AS FamiliaId
    , f.Nombre        AS FamiliaNombre
    , f.ClienteId     AS ClienteId
    , c.Nombre        AS ClienteNombre
    , p.NumeroParte   AS NumeroParte
    , p.Descripcion   AS Descripcion
    , p.Activo        AS Activo
    , p.FechaAlta     AS FechaAlta
FROM
    dbo.Piezas AS p
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    p.Activo = 1
ORDER BY
      c.Nombre
    , f.Nombre
    , p.NumeroParte;";

            var lista = new List<Pieza>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pieza
                        {
                            // Todas estas columnas son NOT NULL: se leen con su tipo exacto
                            PiezaId = dr.GetInt32(dr.GetOrdinal("PiezaId")),
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            FamiliaNombre = dr.GetString(dr.GetOrdinal("FamiliaNombre")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            NumeroParte = dr.GetString(dr.GetOrdinal("NumeroParte")),
                            Descripcion = dr.GetString(dr.GetOrdinal("Descripcion")),
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }
    }
}
```

| Pieza del SQL o del código | Qué hace | Por qué |
|---|---|---|
| Dos `INNER JOIN` | Piezas → Familias → Clientes | `FamiliaId` y `ClienteId` son `NOT NULL` con llave foránea: toda pieza tiene familia y toda familia tiene cliente. Por eso `INNER JOIN` no pierde ninguna fila |
| `f.ClienteId AS ClienteId` | El cliente sale de la familia | `Piezas` no tiene `ClienteId`, y a diferencia de `FamiliasDatos`, ya no es de la tabla principal |
| `WHERE p.Activo = 1` | Solo las piezas activas | La eliminación es lógica: las "eliminadas" siguen en la tabla, y el listado no debe mostrarlas |
| `ORDER BY` cliente, familia, número de parte | Ordena el resultado de forma ascendente (es lo predeterminado) | Sin `ORDER BY`, SQL Server no garantiza ningún orden. Así la tabla sale agrupada y siempre igual |
| `dr.GetInt32(dr.GetOrdinal("..."))` | Busca la columna por su nombre y lee su valor | Leer por nombre no depende del orden de las columnas del `SELECT`. `GetInt32`, `GetString`… exigen el tipo exacto: sirven porque todas las columnas son `NOT NULL` |

| Método | Qué cambia respecto a `FamiliasDatos.cs` |
|---|---|
| `Listar` | Dos `JOIN` en lugar de uno. `Descripcion` ya no acepta `NULL` en `Piezas`, así que se lee con `dr.GetString(...)` y no con `as string`. `ORDER BY` agrega el número de parte |

La estructura (`using`, `SqlConnection`, `SqlCommand`, `ExecuteReader`) es **exactamente la misma** que en Clientes y Familias. Solo está `Listar`: `Insertar`, `Actualizar` y `Desactivar` llegan en el siguiente bloque.

### 3. Negocio

**Archivo nuevo:** clic derecho en la carpeta `Negocio` → *Agregar → Clase* → `PiezasNegocio.cs`. Reemplaza todo su contenido por:

```csharp
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class PiezasNegocio
    {
        private readonly PiezasDatos datos = new PiezasDatos();

        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
        }
    }
}
```

| Pieza | Por qué |
|---|---|
| `new PiezasDatos()` | Negocio no toca SQL: le pide los datos a la capa de Datos |
| `Respuesta.Exito("", ...)` | Todas las respuestas del handler tienen la forma `{ ok, mensaje, datos }`. Listar no necesita mensaje, por eso va vacío |

Respecto a `FamiliasNegocio.cs`, solo queda `Listar`. Las reglas (número de parte obligatorio, largo máximo y que no se repita) llegan en el siguiente bloque, cuando exista `Guardar`.

### 4. Handler

📖 [IHttpHandler](https://learn.microsoft.com/es-es/dotnet/api/system.web.ihttphandler) · [HttpRequest.QueryString](https://learn.microsoft.com/es-es/dotnet/api/system.web.httprequest.querystring) · [Instrucción `switch`](https://learn.microsoft.com/es-es/dotnet/csharp/language-reference/statements/selection-statements) · [Newtonsoft: Serialization Settings](https://www.newtonsoft.com/json/help/html/SerializationSettings.htm)

**Archivo nuevo:** clic derecho en la carpeta `Handlers` → *Agregar → Nuevo elemento →* **Controlador genérico** → `PiezasHandler.ashx`. Visual Studio crea dos archivos: `PiezasHandler.ashx`, que **no se toca**, y `PiezasHandler.ashx.cs`. Reemplaza todo el contenido de este último por:

```csharp
using System;
using System.Web;
using CursoWeb.Modelos;
using CursoWeb.Negocio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CursoWeb.Handlers
{
    public class PiezasHandler : IHttpHandler
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
                var negocio = new PiezasNegocio();
                switch (context.Request.QueryString["accion"])
                {
                    case "listar":
                        respuesta = negocio.Listar();
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

        public bool IsReusable => false;
    }
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `QueryString["accion"]` + `switch` | Lee `?accion=listar` de la URL y elige qué hacer | Un solo handler atiende todas las acciones del catálogo. La rama `default` responde con un error claro si llega una acción que no existe |
| `CamelCasePropertyNamesContractResolver` | Escribe las propiedades del JSON en camelCase (`numeroParte`) | Es la convención de JavaScript: en la página se lee `renglon.numeroParte` |
| `DateFormatString` | Escribe las fechas como `yyyy-MM-ddTHH:mm:ss` | Un formato fijo y legible, que JavaScript convierte con `new Date(...)` |
| `try` / `catch` | Atrapa cualquier error del servidor y lo convierte en una respuesta `{ ok: false }` | La página siempre recibe un JSON, no una pantalla de error. El detalle (`ex.Message`) solo se muestra con `debug="true"`, o sea en desarrollo: en producción no se enseñan detalles internos al usuario |

| `FamiliasHandler.ashx.cs` | `PiezasHandler.ashx.cs` |
|---|---|
| Acciones `listar`, `guardar` y `eliminar` | Solo `listar` |
| `using System.IO` y el método `LeerCuerpo<T>`, que leen el JSON del cuerpo de la petición | No están todavía: `listar` es un `GET` y no recibe cuerpo. Se agregan con `guardar` |

## 5. Probar

1. Compila (**Ctrl+Shift+B**) y ejecuta.
2. Abre `Handlers/PiezasHandler.ashx?accion=listar` en el navegador: debes ver un JSON con `ok` en `true` y, en `datos`, las piezas que insertaste, con propiedades como `numeroParte` y `clienteNombre`.
3. Con `accion=otra` debe responder *"Acción no válida."*

| Si pasa esto | Revisa |
|---|---|
| Visual Studio no reconoce una clase nueva | Que el archivo esté incluido en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*) |
| `Invalid object name 'dbo.Piezas'` | Que la tabla esté creada en la base `CursoWeb` |
| `The SELECT permission was denied` | Que el `GRANT` del día 2 se haya ejecutado en la base `CursoWeb` |
| `datos` viene vacío (`[]`) | Que hayas insertado piezas con `Activo = 1` y con un `FamiliaId` que exista |

---

> **Bloque 2 · Combos dependientes y CRUD de Piezas**

Ahora la pantalla de Piezas. Lo nuevo del día son los **combos dependientes**: el combo de familias solo muestra las familias del cliente que elegiste. Dos ideas para entenderlos:

- Al cambiar el cliente, la página le **pregunta al servidor** (con AJAX) por las familias de ese cliente y llena el segundo combo. Es lo que en WinForms harías en el evento `SelectedIndexChanged` del primer `ComboBox`.
- AJAX es **asíncrono**: la respuesta llega después, y mientras tanto el código sigue. Por eso, al editar una pieza, su familia se selecciona *cuando llega la respuesta*, no antes.

## 6. Combos dependientes: el servidor

📖 [HttpRequest.QueryString](https://learn.microsoft.com/es-es/dotnet/api/system.web.httprequest.querystring) · [Int32.TryParse](https://learn.microsoft.com/es-es/dotnet/api/system.int32.tryparse) · [WHERE](https://learn.microsoft.com/es-es/sql/t-sql/queries/where-transact-sql) · [ORDER BY](https://learn.microsoft.com/es-es/sql/t-sql/queries/select-order-by-clause-transact-sql)

Se empieza por el servidor: así, cuando se escriba el JavaScript, ya hay a quién llamar. Pide las familias de **un** cliente: una acción nueva en el catálogo de Familias.

### Datos: `ListarPorCliente`

**Agregar** a `Datos/FamiliasDatos.cs`, después de `Listar`:

```csharp
public List<Familia> ListarPorCliente(int clienteId)
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
    f.ClienteId = @ClienteId
    AND f.Activo = 1
ORDER BY
    f.Nombre;";

    var lista = new List<Familia>();
    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;

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
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `WHERE f.ClienteId = @ClienteId` | Solo las familias de ese cliente | El valor llega como **parámetro**, nunca pegado al texto del SQL: así se evita la inyección de SQL, igual que en todo el curso |
| `AND f.Activo = 1` | Solo las familias activas | La eliminación es lógica: las eliminadas siguen en la tabla y no deben ofrecerse |
| `ORDER BY f.Nombre` | El combo sale en orden alfabético | Todas son del mismo cliente: ya no hace falta ordenar también por cliente |
| `cmd.Parameters.Add("@ClienteId", SqlDbType.Int)` | Pasa el `clienteId` al SQL | Se agrega antes de `cn.Open()`, como en `Insertar` |

Es `Listar` con un `WHERE` y un parámetro. El ciclo que lee los renglones es el mismo; por ahora se repite, como el resto del código de Datos.

### Negocio: `ListarPorCliente`

**Agregar** a `Negocio/FamiliasNegocio.cs`, después de `Listar`:

```csharp
public Respuesta ListarPorCliente(int clienteId)
{
    if (clienteId <= 0)
    {
        return Respuesta.Error("Selecciona un cliente.");
    }
    return Respuesta.Exito("", datos.ListarPorCliente(clienteId));
}
```

Negocio revisa antes de pedirle datos a Datos: si no llegó un cliente válido, responde con un error claro en lugar de consultar la base con un valor que no tiene sentido.

### Handler: la acción `listarPorCliente`

**Agregar** al `switch` de `Handlers/FamiliasHandler.ashx.cs`, entre los casos `listar` y `guardar`:

```csharp
case "listarPorCliente":
    int clienteId;
    respuesta = int.TryParse(context.Request.QueryString["clienteId"], out clienteId)
        ? negocio.ListarPorCliente(clienteId)
        : Respuesta.Error("Falta el cliente.");
    break;
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `QueryString["clienteId"]` | Lee `&clienteId=3` de la URL | Es un `GET`: los datos viajan en la URL, no en el cuerpo |
| `int.TryParse(..., out clienteId)` | Convierte el texto a número y dice si pudo | El valor de la URL siempre es **texto**, y puede faltar o no ser un número. `TryParse` regresa `false` en lugar de lanzar un error |
| `condición ? A : B` | Si se pudo convertir, pide la lista; si no, responde un error | El mismo operador que ya usa `guardar` |
| `"listarPorCliente"` | El nombre de la acción | El `switch` compara texto **con mayúsculas y minúsculas**: debe escribirse igual aquí y en el JavaScript |

Pruébalo en el navegador: `Handlers/FamiliasHandler.ashx?accion=listarPorCliente&clienteId=1` debe responder con las familias activas del cliente 1.

## 7. La página `Piezas.aspx`

📖 [Bootstrap: Select](https://getbootstrap.com/docs/5.3/forms/select/) · [MDN: `<select>`](https://developer.mozilla.org/es/docs/Web/HTML/Reference/Elements/select) · [MDN: `<textarea>`](https://developer.mozilla.org/es/docs/Web/HTML/Reference/Elements/textarea) · [Bootstrap: Modal](https://getbootstrap.com/docs/5.3/components/modal/)

**Reemplazar** todo el contenido de `Piezas.aspx` (hoy solo tiene un `<h1>`) por:

```aspx
<%@ Page Title="Piezas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Piezas.aspx.cs" Inherits="CursoWeb.Piezas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Piezas</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nueva pieza</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <table id="tblPiezas"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlPieza" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nueva pieza</h5>
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
                            <label for="cboFamilia" class="form-label">Familia</label>
                            <select id="cboFamilia" class="form-select" disabled></select>
                        </div>
                        <div class="col-12">
                            <label for="txtNumeroParte" class="form-label">Número de parte</label>
                            <input type="text" id="txtNumeroParte" class="form-control" maxlength="50" />
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
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar pieza</h5>
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
    <script src="js/piezas.js"></script>
</asp:Content>
```

| `Familias.aspx` | `Piezas.aspx` |
|---|---|
| Título, botón y tabla de Familias (`tblFamilias`, `btnNuevo`) | Los de Piezas: `tblPiezas`, y el modal se llama `mdlPieza` |
| Un combo (`cboCliente`), nombre y descripción | **Dos combos**: `cboCliente` y `cboFamilia`. Después, `txtNumeroParte` (`maxlength="50"`, como la columna) y `txtDescripcion` |
| El combo de cliente arranca con opciones | `cboFamilia` lleva el atributo **`disabled`**: no hay familias que mostrar hasta que se elija un cliente |
| Mismos modales de Bootstrap, mismo botón de eliminar | Iguales. El script es `js/piezas.js` |

La descripción de Piezas es **obligatoria** (en la tabla es `NOT NULL`), a diferencia de la de Familias.

## 8. `piezas.js`: los combos y el listado

📖 [jQuery: `.on()`](https://api.jquery.com/on/) · [jQuery: evento `change`](https://api.jquery.com/change/) · [jQuery: `$.ajax()`](https://api.jquery.com/jQuery.ajax/) · [jQuery: `.prop()`](https://api.jquery.com/prop/) · [jQuery: `.append()`](https://api.jquery.com/append/) · [Bootstrap Table: métodos](https://bootstrap-table.com/docs/api/methods/) · [Bootstrap Table: opciones de columna](https://bootstrap-table.com/docs/api/column-options/)

**Archivo nuevo:** clic derecho en la carpeta `js` → *Agregar → Nuevo elemento → Archivo JavaScript* → `piezas.js`. Contenido completo:

```javascript
let piezaIdEditando = 0;
let modalPieza;

$(function () {
    modalPieza = new bootstrap.Modal('#mdlPieza');

    $('#btnNuevo').on('click', nuevo);
    $('#cboCliente').on('change', cambiarCliente);
    $('#mdlPieza').on('shown.bs.modal', function () {
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

// Al cambiar el cliente, se piden al servidor sus familias
function cambiarCliente() {
    cargarFamilias(Number($('#cboCliente').val()) || 0, 0);
}

// Llena el combo de familias con las del cliente. Si llega familiaId (al editar), la deja seleccionada
function cargarFamilias(clienteId, familiaId) {
    const $combo = $('#cboFamilia');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una familia'));
    $combo.prop('disabled', true);
    ocultarMensaje('#divMensajeModal');
    if (!clienteId) {
        return;
    }

    $.ajax({
        url: 'Handlers/FamiliasHandler.ashx?accion=listarPorCliente&clienteId=' + clienteId,
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            // Si el cliente cambió mientras llegaba la respuesta, esta ya no sirve
            if (clienteId !== (Number($('#cboCliente').val()) || 0)) {
                return;
            }
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            if (r.datos.length === 0) {
                mostrarMensaje('Este cliente no tiene familias activas.', 'danger', '#divMensajeModal');
                return;
            }
            r.datos.forEach(function (f) {
                $combo.append($('<option>').val(f.familiaId).text(f.nombre));
            });
            $combo.prop('disabled', false);
            if (familiaId) {
                $combo.val(familiaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las familias (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
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
    modalPieza.show();
}

function limpiarCampos() {
    $('#cboCliente, #txtNumeroParte, #txtDescripcion').val('');
    cargarFamilias(0, 0);
    piezaIdEditando = 0;
    $('#lblTituloFormulario').text('Nueva pieza');
}

function inicializarTabla() {
    $('#tblPiezas').bootstrapTable({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
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
            { field: 'familiaNombre', title: 'Familia', sortable: true },
            { field: 'numeroParte', title: 'Número de parte', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha }
        ]
    });
}

function formatoFecha(valor) {
    return valor ? new Date(valor).toLocaleDateString('es-MX') : '';
}

```

| Función | Qué hace | Por qué |
|---|---|---|
| Arranque | Crea el modal, enlaza el botón y el **`change`** del combo de cliente, y carga clientes y tabla | `.on('change', ...)` ejecuta `cambiarCliente` cada vez que el usuario elige otro cliente |
| `cargarClientes` | Llena el combo de clientes | Igual que en Familias: se reutiliza `ClientesHandler.ashx?accion=listar` |
| `cambiarCliente` | Toma el cliente elegido y pide sus familias | `Number(...) \|\| 0` convierte el valor del `<select>` (siempre texto) en número, y deja `0` si no se eligió nada |
| **`cargarFamilias`** | Vacía el combo de familias, lo deshabilita, pide las del cliente y las agrega | Es lo nuevo del día. Lo detallamos abajo |
| `limpiarCampos` | Limpia el formulario y llama `cargarFamilias(0, 0)` | Con cliente `0` el combo de familias queda vacío y deshabilitado |
| `inicializarTabla` | Columnas: cliente, familia, número de parte, descripción y alta | Los nombres salen del JSON del listado (`familiaNombre`, `numeroParte`…) |

**Lo nuevo: `cargarFamilias(clienteId, familiaId)`**

| Pieza | Para qué |
|---|---|
| `$combo.empty()` y la opción *"Selecciona una familia"* | Deja el combo como nuevo antes de llenarlo, para no acumular las familias del cliente anterior |
| `$combo.prop('disabled', true)` | Mientras no lleguen las familias, no se puede elegir nada |
| `if (!clienteId) { return; }` | Sin cliente elegido no hay nada que pedir |
| `...accion=listarPorCliente&clienteId=` + `clienteId` | La petición al handler que acabamos de agregar |
| `if (clienteId !== ...) { return; }` dentro de `.done` | **Respuesta tardía:** si el usuario cambió de cliente mientras llegaba la respuesta, esa lista ya no corresponde. Se descarta |
| `r.datos.length === 0` | Un cliente sin familias activas: se avisa en lugar de dejar el combo vacío sin explicación |
| `$combo.val(familiaId)` | Se usa al **editar**: deja seleccionada la familia de la pieza, pero hasta que ya están cargadas las opciones. Hoy llega `0`: no hace nada |
| `.fail(...)` | Si el servidor no responde, el mensaje dice el código de estado |

### Probar los combos
1. Ejecuta y abre **Piezas**: la tabla muestra las piezas con su cliente y su familia.
2. **Nueva pieza**: el combo de familia está deshabilitado.
3. Elige un cliente: el combo de familia se habilita con **solo** sus familias.
4. Cambia a otro cliente: la lista de familias cambia.
5. Cierra el modal y ábrelo otra vez: el combo de familia vuelve a estar deshabilitado.

## 9. CRUD de Piezas: el servidor

📖 [SqlCommand.ExecuteScalar](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand.executescalar) · [SCOPE_IDENTITY](https://learn.microsoft.com/es-es/sql/t-sql/functions/scope-identity-transact-sql) · [UPDATE (Transact-SQL)](https://learn.microsoft.com/es-es/sql/t-sql/queries/update-transact-sql) · [SqlCommand.ExecuteNonQuery](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand.executenonquery) · [COUNT (Transact-SQL)](https://learn.microsoft.com/es-es/sql/t-sql/functions/count-transact-sql) · [String.IsNullOrWhiteSpace](https://learn.microsoft.com/es-es/dotnet/api/system.string.isnullorwhitespace) · [String.Trim](https://learn.microsoft.com/es-es/dotnet/api/system.string.trim) · [IHttpHandler](https://learn.microsoft.com/es-es/dotnet/api/system.web.ihttphandler) · [HttpRequest.InputStream](https://learn.microsoft.com/es-es/dotnet/api/system.web.httprequest.inputstream) · [MDN: métodos HTTP](https://developer.mozilla.org/es/docs/Web/HTTP/Reference/Methods) · [MDN: 405](https://developer.mozilla.org/es/docs/Web/HTTP/Reference/Status/405)

Falta guardar, editar y eliminar, y la regla nueva del día: **que no se repita el número de parte**. Es el mismo recorrido de Familias: se empieza por el servidor.

### Datos

**Agregar** a `Datos/PiezasDatos.cs`, después de `Listar`:

```csharp
public int Insertar(Pieza p)
{
    const string sql = @"
INSERT INTO dbo.Piezas (FamiliaId, NumeroParte, Descripcion)
VALUES (@FamiliaId, @NumeroParte, @Descripcion);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS PiezaId;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = p.FamiliaId;
        cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = p.NumeroParte;
        cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = p.Descripcion;

        cn.Open();
        return (int)cmd.ExecuteScalar();
    }
}

public int Actualizar(Pieza p)
{
    const string sql = @"
UPDATE p
SET
      p.FamiliaId     = @FamiliaId
    , p.NumeroParte   = @NumeroParte
    , p.Descripcion   = @Descripcion
FROM
    dbo.Piezas AS p
WHERE
    p.PiezaId = @PiezaId
    AND p.Activo = 1;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = p.PiezaId;
        cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = p.FamiliaId;
        cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = p.NumeroParte;
        cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = p.Descripcion;

        cn.Open();
        return cmd.ExecuteNonQuery();
    }
}

public int Desactivar(int piezaId)
{
    const string sql = @"
UPDATE p
SET
    p.Activo = 0
FROM
    dbo.Piezas AS p
WHERE
    p.PiezaId = @PiezaId
    AND p.Activo = 1;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
        cn.Open();
        return cmd.ExecuteNonQuery();
    }
}

// Cuenta las piezas con ese número de parte, activas o eliminadas, sin contar la que se está editando
public bool ExisteNumeroParte(string numeroParte, int piezaId)
{
    const string sql = @"
SELECT
    COUNT(*) AS Total
FROM
    dbo.Piezas AS p
WHERE
    p.NumeroParte = @NumeroParte
    AND p.PiezaId <> @PiezaId;";

    using (var cn = new SqlConnection(cadena))
    using (var cmd = new SqlCommand(sql, cn))
    {
        cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = numeroParte;
        cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;

        cn.Open();
        return (int)cmd.ExecuteScalar() > 0;
    }
}
```

| Método | Qué cambia respecto a `FamiliasDatos.cs` |
|---|---|
| `Insertar` | `@NumeroParte` es `SqlDbType.VarChar, 50` (la columna es `VARCHAR`) y `@Descripcion` es `NVarChar, 250`. La descripción es obligatoria, así que ya no lleva `(object)... ?? DBNull.Value` |
| `Actualizar` | Actualiza `FamiliaId`: una pieza se puede pasar a otra familia. No actualiza el cliente, porque la pieza no lo guarda |
| `Desactivar` | `PiezaId` en lugar de `FamiliaId`; lo demás, igual |
| **`ExisteNumeroParte`** | Nuevo. Lo explicamos abajo |

**Lo nuevo: `ExisteNumeroParte`**

| Pieza | Qué hace | Por qué |
|---|---|---|
| `COUNT(*)` | Cuenta cuántas piezas cumplen la condición | Solo importa saber si hay alguna, no traer sus datos |
| `p.NumeroParte = @NumeroParte` | Busca ese número de parte | Sin `AND p.Activo = 1`: el `UNIQUE` de la base también cuenta las piezas **eliminadas**, y esta revisión debe coincidir con él |
| `AND p.PiezaId <> @PiezaId` | No cuenta la pieza que se está editando | Sin esto, guardar una pieza **sin cambiar** su número diría que ya existe, porque se encuentra a sí misma. En un alta, `PiezaId` es `0` y no excluye a nadie |
| `(int)cmd.ExecuteScalar() > 0` | Devuelve `true` si hay alguna | `ExecuteScalar` regresa la primera columna del primer renglón: aquí, el conteo |

### Negocio

En `Negocio/PiezasNegocio.cs`, primero se agrega un `using` (para la lista de errores).

**Antes:**

```csharp
using CursoWeb.Datos;
using CursoWeb.Modelos;
```
**Después:**

```csharp
using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;
```
**Agregar** a la clase, después de `Listar`:

```csharp
public Respuesta Guardar(Pieza p)
{
    Normalizar(p);

    var errores = Validar(p);
    if (errores.Count > 0)
    {
        return Respuesta.Error(string.Join(" ", errores));
    }

    if (datos.ExisteNumeroParte(p.NumeroParte, p.PiezaId))
    {
        return Respuesta.Error("Ya existe una pieza con ese número de parte (puede estar eliminada).");
    }

    if (p.PiezaId == 0)
    {
        p.PiezaId = datos.Insertar(p);
        return Respuesta.Exito("Pieza guardada.", p);
    }

    if (datos.Actualizar(p) == 0)
    {
        return Respuesta.Error("La pieza no existe o fue eliminada.");
    }
    return Respuesta.Exito("Pieza actualizada.", p);
}

public Respuesta Eliminar(int piezaId)
{
    if (datos.Desactivar(piezaId) == 0)
    {
        return Respuesta.Error("La pieza no existe o ya fue eliminada.");
    }
    return Respuesta.Exito("Pieza eliminada.");
}

private static void Normalizar(Pieza p)
{
    p.NumeroParte = Limpiar(p.NumeroParte);
    p.Descripcion = Limpiar(p.Descripcion);
}

private static string Limpiar(string valor)
{
    if (string.IsNullOrWhiteSpace(valor))
    {
        return null;
    }
    return valor.Trim();
}

private static List<string> Validar(Pieza p)
{
    var errores = new List<string>();

    if (p.FamiliaId <= 0)
    {
        errores.Add("Selecciona una familia.");
    }

    if (p.NumeroParte == null)
    {
        errores.Add("El número de parte es obligatorio.");
    }
    else if (p.NumeroParte.Length > 50)
    {
        errores.Add("El número de parte no puede pasar de 50 caracteres.");
    }

    if (p.Descripcion == null)
    {
        errores.Add("La descripción es obligatoria.");
    }
    else if (p.Descripcion.Length > 250)
    {
        errores.Add("La descripción no puede pasar de 250 caracteres.");
    }

    return errores;
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `Normalizar` y `Limpiar` | Quitan espacios de los extremos y convierten lo vacío en `null` | Son **idénticos** a los de Familias. Con `null`, `Validar` solo pregunta una cosa: si falta el valor |
| `Validar` | Familia elegida, número de parte obligatorio (máximo 50) y descripción obligatoria (máximo 250) | Los largos son los de las columnas: así el error lo da Negocio con un mensaje claro y no SQL Server con uno técnico |
| `ExisteNumeroParte(...)` en `Guardar` | Revisa el número de parte **después de validar** | Necesita un número ya válido. Es la revisión amable: el usuario recibe un mensaje que entiende |
| El mensaje *"...(puede estar eliminada)"* | Dice que el número puede estar ocupado por una pieza eliminada | El `UNIQUE` no mira `Activo`: el número de una pieza eliminada sigue ocupado |
| `PiezaId == 0` | Decide entre insertar y actualizar | La misma acción `guardar` de Clientes y Familias |

El `UNIQUE` de la base sigue siendo la **última defensa**: si dos usuarios guardaran el mismo número de parte al mismo tiempo, la revisión de Negocio podría pasar en los dos y SQL Server rechazaría al segundo.

### Handler

En `Handlers/PiezasHandler.ashx.cs`, tres cambios.

**1. Un `using` nuevo**, para leer el cuerpo de la petición.

**Antes:**

```csharp
using System;
using System.Web;
```
**Después:**

```csharp
using System;
using System.IO;
using System.Web;
```
**2. Agregar** al `switch`, entre `listar` y `default`:

```csharp
case "guardar":
    if (context.Request.HttpMethod != "POST")
    {
        context.Response.StatusCode = 405;
        respuesta = Respuesta.Error("Para guardar se usa POST.");
        break;
    }

    var pieza = LeerCuerpo<Pieza>(context);
    respuesta = pieza == null
        ? Respuesta.Error("No llegaron los datos de la pieza.")
        : negocio.Guardar(pieza);
    break;

case "eliminar":
    if (context.Request.HttpMethod != "POST")
    {
        context.Response.StatusCode = 405;
        respuesta = Respuesta.Error("Para eliminar se usa POST.");
        break;
    }

    var aEliminar = LeerCuerpo<Pieza>(context);
    respuesta = aEliminar == null
        ? Respuesta.Error("No llegó la pieza a eliminar.")
        : negocio.Eliminar(aEliminar.PiezaId);
    break;
```
**3. Agregar** a la clase, antes de `IsReusable`:

```csharp
private static T LeerCuerpo<T>(HttpContext context)
{
    using (var lector = new StreamReader(context.Request.InputStream))
    {
        return JsonConvert.DeserializeObject<T>(lector.ReadToEnd(), Json);
    }
}
```

Es `FamiliasHandler` con otros nombres: lo que cambia datos solo acepta `POST` (si no, responde `405`), y el objeto llega como JSON en el cuerpo de la petición. `guardar` recibe una `Pieza` completa; `eliminar` solo usa su `PiezaId`.

## 10. `piezas.js`: guardar, editar y eliminar

📖 [jQuery: `$.ajax()`](https://api.jquery.com/jQuery.ajax/) · [jQuery: `.val()`](https://api.jquery.com/val/) · [Bootstrap: Modal](https://getbootstrap.com/docs/5.3/components/modal/) · [Bootstrap Table: métodos](https://bootstrap-table.com/docs/api/methods/) · [Bootstrap Table: opciones de columna](https://bootstrap-table.com/docs/api/column-options/) · [MDN: `JSON.stringify`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify)

Seis cambios en `js/piezas.js`.

**1. Variables.**

**Antes:**

```javascript
let piezaIdEditando = 0;
let modalPieza;
```
**Después:**

```javascript
let piezaIdEditando = 0;
let piezaAEliminar = null;
let modalPieza;
let modalConfirmar;
```
**2. Arranque.** Enlaza el botón **Guardar** y el de confirmar la eliminación, y crea el modal de confirmación.

**Antes:**

```javascript
$(function () {
    modalPieza = new bootstrap.Modal('#mdlPieza');

    $('#btnNuevo').on('click', nuevo);
    $('#cboCliente').on('change', cambiarCliente);
    $('#mdlPieza').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    inicializarTabla();
});
```
**Después:**

```javascript
$(function () {
    modalPieza = new bootstrap.Modal('#mdlPieza');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#cboCliente').on('change', cambiarCliente);
    $('#mdlPieza').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    inicializarTabla();
});
```
**3. Agregar** después de `cargarFamilias`:

```javascript
function obtenerPieza() {
    return {
        piezaId: piezaIdEditando,
        familiaId: Number($('#cboFamilia').val()) || 0,
        numeroParte: $('#txtNumeroParte').val().trim(),
        descripcion: $('#txtDescripcion').val().trim()
    };
}

function validarPieza(p, clienteId) {
    const errores = [];
    if (!clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!p.familiaId) {
        errores.push('Selecciona una familia.');
    }
    if (!p.numeroParte) {
        errores.push('El número de parte es obligatorio.');
    }
    if (!p.descripcion) {
        errores.push('La descripción es obligatoria.');
    }
    return errores;
}
```
**4. Agregar** después de `nuevo`:

```javascript
function guardar() {
    ocultarMensaje('#divMensajeModal');
    const pieza = obtenerPieza();
    const errores = validarPieza(pieza, Number($('#cboCliente').val()) || 0);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarPieza(pieza);
}

function enviarPieza(pieza) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(pieza),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalPieza.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            $('#tblPiezas').bootstrapTable('refresh');
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}
```
**5. Tabla.** Se agrega la columna **Acciones**.

**Antes:**

```javascript
function inicializarTabla() {
    $('#tblPiezas').bootstrapTable({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
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
            { field: 'familiaNombre', title: 'Familia', sortable: true },
            { field: 'numeroParte', title: 'Número de parte', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha }
        ]
    });
}
```
**Después:**

```javascript
function inicializarTabla() {
    $('#tblPiezas').bootstrapTable({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
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
            { field: 'familiaNombre', title: 'Familia', sortable: true },
            { field: 'numeroParte', title: 'Número de parte', sortable: true },
            { field: 'descripcion', title: 'Descripción' },
            { field: 'fechaAlta', title: 'Alta', sortable: true, formatter: formatoFecha },
            { field: 'piezaId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
        ]
    });
}
```
**6. Agregar** al final del archivo:

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

function editar(p) {
    piezaIdEditando = p.piezaId;
    $('#cboCliente').val(p.clienteId);
    cargarFamilias(p.clienteId, p.familiaId);
    $('#txtNumeroParte').val(p.numeroParte);
    $('#txtDescripcion').val(p.descripcion);
    $('#lblTituloFormulario').text('Editar pieza');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalPieza.show();
}

function eliminar(p) {
    piezaAEliminar = p;
    $('#lblConfirmar').text('¿Eliminar la pieza "' + p.numeroParte + '" de ' + p.familiaNombre + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ piezaId: piezaAEliminar.piezaId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                $('#tblPiezas').bootstrapTable('refresh');
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
```

| Función | Qué cambia respecto a `familias.js` |
|---|---|
| `obtenerPieza` | Lee solo la **familia** (`cboFamilia`), el número de parte y la descripción. El cliente no viaja al servidor: la pieza solo guarda su familia |
| `validarPieza` | Recibe también el cliente elegido, porque no forma parte del objeto. Valida cliente, familia, número de parte y descripción |
| `enviarPieza` | `PiezasHandler.ashx?accion=guardar`. Es lo mismo que `enviarFamilia` |
| `editar` | **Es lo nuevo.** Selecciona el cliente, pide sus familias con `cargarFamilias(p.clienteId, p.familiaId)` y la familia queda seleccionada *cuando llegan* |
| `eliminar` / `confirmarEliminar` | El texto de confirmación usa el número de parte y la familia. Lo demás, igual |

**Por qué `editar` no hace `$('#cboFamilia').val(p.familiaId)` directamente:** en ese momento el combo de familias todavía no tiene opciones, porque la respuesta del servidor no ha llegado. Por eso `cargarFamilias` recibe el `familiaId` y lo selecciona *dentro del `.done`*, cuando las opciones ya existen.

### Probar
1. **Nueva pieza** → elige cliente y familia, escribe un número de parte y una descripción → **Guardar**. Aparece en la tabla.
2. Guarda sin llenar nada: los mensajes dicen qué falta (cliente, familia, número de parte y descripción).
3. Repite un número de parte que ya existe: *"Ya existe una pieza con ese número de parte (puede estar eliminada)."*
4. **Editar** una pieza: el cliente y la familia aparecen seleccionados. Guarda **sin cambiar** el número: se guarda sin decir que ya existe.
5. **Editar** y cambia la pieza a **otra familia** del mismo cliente: se guarda y la tabla lo refleja.
6. **Eliminar** → confirmar: desaparece de la tabla. Intenta dar de alta otra pieza con **el mismo número de parte**: sigue ocupado.

| Si pasa esto | Revisa |
|---|---|
| El combo de familias no se llena | Que el nombre de la acción sea exactamente `listarPorCliente` en el handler y en `piezas.js` (el `switch` distingue mayúsculas), y la URL en Network (F12) |
| El combo de familias se queda deshabilitado | Que el cliente tenga familias activas; si no, el mensaje lo dice |
| Responde *"Acción no válida."* | El nombre de la acción en la URL no coincide con ningún `case` |
| Responde *"Para guardar se usa POST."* | La petición se hizo con `GET`: revisa `type: 'POST'` en `enviarPieza` |
| Al editar, la familia no queda seleccionada | Que `editar` llame `cargarFamilias(p.clienteId, p.familiaId)` con los dos valores |
| Al editar una pieza de un cliente eliminado, el combo de cliente aparece vacío | Es un caso límite conocido: el combo solo trae clientes activos |
| Visual Studio no reconoce `ExisteNumeroParte` u otro método nuevo | Que el método esté dentro de la clase, antes de la llave que la cierra |
| Acentos raros (`DescripciÃ³n`) | Guarda `Piezas.aspx` desde Visual Studio con *UTF-8 con firma* |
