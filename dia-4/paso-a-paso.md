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
