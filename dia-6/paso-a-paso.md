# Paso a paso · Día 6 · Ejercicio integrador

Seguimos con el ejercicio integrador que arrancó el día 5 (requerimiento, tabla `Lotes` y modelo `Lote`: [secciones 7 a 10 del paso a paso del día 5](../dia-5/paso-a-paso.md)). Hoy construyes las capas que faltan, una por una, con el instructor. Este paso a paso es el material para compararlo con lo tuyo o para continuar si te atrasas: cada sección trae el archivo completo, qué hace y por qué. Compila (**Ctrl+Shift+B**) al terminar cada capa antes de seguir con la siguiente.

La solución completa del proyecto está en [`final/`](final/).

---

## 1. Datos

📖 [SqlCommand.ExecuteScalar](https://learn.microsoft.com/es-es/dotnet/api/system.data.sqlclient.sqlcommand.executescalar) · [SCOPE_IDENTITY](https://learn.microsoft.com/es-es/sql/t-sql/functions/scope-identity-transact-sql) · [Tipos de datos de fecha y hora](https://learn.microsoft.com/es-es/sql/t-sql/data-types/date-transact-sql) · [IS NULL](https://learn.microsoft.com/es-es/sql/t-sql/queries/is-null-transact-sql)

**Archivo nuevo:** clic derecho en la carpeta `Datos` → *Agregar → Clase* → `LotesDatos.cs`. Reemplaza todo su contenido por:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class LotesDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        // piezaId en 0 significa "todas"; desde y hasta en null significan "sin límite"
        public List<Lote> Listar(int piezaId, DateTime? desde, DateTime? hasta)
        {
            const string sql = @"
SELECT
      l.LoteId            AS LoteId
    , l.PiezaId           AS PiezaId
    , p.NumeroParte       AS NumeroParte
    , p.FamiliaId         AS FamiliaId
    , f.Nombre            AS FamiliaNombre
    , f.ClienteId         AS ClienteId
    , c.Nombre            AS ClienteNombre
    , l.NumeroLote        AS NumeroLote
    , l.Cantidad          AS Cantidad
    , l.FechaProduccion   AS FechaProduccion
    , l.Turno             AS Turno
    , l.Activo            AS Activo
    , l.FechaAlta         AS FechaAlta
FROM
    dbo.Lotes AS l
INNER JOIN
    dbo.Piezas AS p ON p.PiezaId = l.PiezaId
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    l.Activo = 1
AND (@PiezaId = 0 OR l.PiezaId = @PiezaId)
AND (@Desde IS NULL OR l.FechaProduccion >= @Desde)
AND (@Hasta IS NULL OR l.FechaProduccion <= @Hasta)
ORDER BY
      l.FechaProduccion DESC
    , l.LoteId DESC;";

            var lista = new List<Lote>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = (object)desde ?? DBNull.Value;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = (object)hasta ?? DBNull.Value;

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Lote
                        {
                            // Todas estas columnas son NOT NULL: se leen con su tipo exacto
                            LoteId = dr.GetInt32(dr.GetOrdinal("LoteId")),
                            PiezaId = dr.GetInt32(dr.GetOrdinal("PiezaId")),
                            NumeroParte = dr.GetString(dr.GetOrdinal("NumeroParte")),
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            FamiliaNombre = dr.GetString(dr.GetOrdinal("FamiliaNombre")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            NumeroLote = dr.GetString(dr.GetOrdinal("NumeroLote")),
                            Cantidad = dr.GetInt32(dr.GetOrdinal("Cantidad")),
                            FechaProduccion = dr.GetDateTime(dr.GetOrdinal("FechaProduccion")),
                            Turno = dr.GetByte(dr.GetOrdinal("Turno")),
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public int Insertar(Lote l)
        {
            const string sql = @"
INSERT INTO dbo.Lotes (PiezaId, NumeroLote, Cantidad, FechaProduccion, Turno)
VALUES (@PiezaId, @NumeroLote, @Cantidad, @FechaProduccion, @Turno);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS LoteId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = l.PiezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = l.NumeroLote;
                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = l.Cantidad;
                cmd.Parameters.Add("@FechaProduccion", SqlDbType.Date).Value = l.FechaProduccion;
                cmd.Parameters.Add("@Turno", SqlDbType.TinyInt).Value = (byte)l.Turno;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int Actualizar(Lote l)
        {
            const string sql = @"
UPDATE l
SET
      l.PiezaId          = @PiezaId
    , l.NumeroLote       = @NumeroLote
    , l.Cantidad         = @Cantidad
    , l.FechaProduccion  = @FechaProduccion
    , l.Turno            = @Turno
FROM
    dbo.Lotes AS l
WHERE
    l.LoteId = @LoteId
    AND l.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = l.LoteId;
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = l.PiezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = l.NumeroLote;
                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = l.Cantidad;
                cmd.Parameters.Add("@FechaProduccion", SqlDbType.Date).Value = l.FechaProduccion;
                cmd.Parameters.Add("@Turno", SqlDbType.TinyInt).Value = (byte)l.Turno;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int loteId)
        {
            const string sql = @"
UPDATE l
SET
    l.Activo = 0
FROM
    dbo.Lotes AS l
WHERE
    l.LoteId = @LoteId
    AND l.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = loteId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Cuenta los lotes con ese número para esa pieza, activos o eliminados, sin contar el que se está editando
        public bool ExisteNumeroLote(int piezaId, string numeroLote, int loteId)
        {
            const string sql = @"
SELECT
    COUNT(*) AS Total
FROM
    dbo.Lotes AS l
WHERE
    l.PiezaId = @PiezaId
    AND l.NumeroLote = @NumeroLote
    AND l.LoteId <> @LoteId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = numeroLote;
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = loteId;

                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
```

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

## 2. Negocio

📖 [DateTime.Today](https://learn.microsoft.com/es-es/dotnet/api/system.datetime.today) · [Nullable&lt;T&gt;](https://learn.microsoft.com/es-es/dotnet/api/system.nullable-1)

**Archivo nuevo:** clic derecho en la carpeta `Negocio` → *Agregar → Clase* → `LotesNegocio.cs`. Reemplaza todo su contenido por:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class LotesNegocio
    {
        private readonly LotesDatos datos = new LotesDatos();

        // piezaId en 0 significa "todas"; desde y hasta en null significan "sin límite"
        public Respuesta Listar(int piezaId, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                return Respuesta.Error("La fecha inicial no puede ser mayor que la final.");
            }
            return Respuesta.Exito("", datos.Listar(piezaId, desde, hasta));
        }

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

        public Respuesta Guardar(Lote l)
        {
            Normalizar(l);

            var errores = Validar(l);
            if (errores.Count > 0)
            {
                return Respuesta.Error(string.Join(" ", errores));
            }

            if (datos.ExisteNumeroLote(l.PiezaId, l.NumeroLote, l.LoteId))
            {
                return Respuesta.Error("Ya existe ese número de lote para la pieza (puede estar eliminado).");
            }

            if (l.LoteId == 0)
            {
                l.LoteId = datos.Insertar(l);
                return Respuesta.Exito("Lote guardado.", l);
            }

            if (datos.Actualizar(l) == 0)
            {
                return Respuesta.Error("El lote no existe o fue eliminado.");
            }
            return Respuesta.Exito("Lote actualizado.", l);
        }

        public Respuesta Eliminar(int loteId)
        {
            if (datos.Desactivar(loteId) == 0)
            {
                return Respuesta.Error("El lote no existe o ya fue eliminado.");
            }
            return Respuesta.Exito("Lote eliminado.");
        }

        private static void Normalizar(Lote l)
        {
            l.NumeroLote = Limpiar(l.NumeroLote);
        }

        private static string Limpiar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            return valor.Trim();
        }

        private static List<string> Validar(Lote l)
        {
            var errores = new List<string>();

            if (l.PiezaId <= 0)
            {
                errores.Add("Selecciona una pieza.");
            }

            if (l.NumeroLote == null)
            {
                errores.Add("El número de lote es obligatorio.");
            }
            else if (l.NumeroLote.Length > 50)
            {
                errores.Add("El número de lote no puede pasar de 50 caracteres.");
            }

            if (l.Cantidad <= 0)
            {
                errores.Add("La cantidad debe ser mayor a cero.");
            }

            if (l.FechaProduccion == default(DateTime))
            {
                errores.Add("La fecha de producción es obligatoria.");
            }
            else if (l.FechaProduccion.Date > DateTime.Today)
            {
                errores.Add("La fecha de producción no puede ser futura.");
            }

            if (l.Turno < 1 || l.Turno > 3)
            {
                errores.Add("El turno debe ser 1, 2 o 3.");
            }

            return errores;
        }
    }
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `Listar` valida el rango | Si `desde` es mayor que `hasta`, responde un error | La pantalla ya lo avisa, pero Negocio es quien decide |
| `Validar`: fecha futura | `FechaProduccion.Date > DateTime.Today` | La regla que no puede vivir en la tabla |
| `FechaProduccion == default(DateTime)` | Detecta que no llegó fecha | Una fecha que falta llega como `0001-01-01` |
| `Cantidad <= 0`, `Turno < 1 || Turno > 3` | Reglas de rango | También viven en los `CHECK` de la tabla: aquí dan un mensaje claro |
| `ExisteNumeroLote` antes de guardar | Mensaje claro si el lote ya existe para esa pieza | El `UNIQUE` sigue siendo la última defensa |
| `ExportarExcel` | Los mismos datos que `Listar` | Es el de Piezas con otras columnas |

## 3. Handler

**Archivo nuevo:** clic derecho en la carpeta `Handlers` → *Agregar → Nuevo elemento →* **Controlador genérico** → `LotesHandler.ashx`. Visual Studio crea dos archivos:

- `LotesHandler.ashx`: una sola línea que apunta a la clase. **No se toca.**
- `LotesHandler.ashx.cs`: reemplaza todo su contenido por:

```csharp
using System;
using System.Globalization;
using System.IO;
using System.Web;
using CursoWeb.Modelos;
using CursoWeb.Negocio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CursoWeb.Handlers
{
    public class LotesHandler : IHttpHandler
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
                var negocio = new LotesNegocio();
                switch (context.Request.QueryString["accion"])
                {
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
                        if (context.Request.HttpMethod != "POST")
                        {
                            context.Response.StatusCode = 405;
                            respuesta = Respuesta.Error("Para guardar se usa POST.");
                            break;
                        }

                        var lote = LeerCuerpo<Lote>(context);
                        respuesta = lote == null
                            ? Respuesta.Error("No llegaron los datos del lote.")
                            : negocio.Guardar(lote);
                        break;

                    case "eliminar":
                        if (context.Request.HttpMethod != "POST")
                        {
                            context.Response.StatusCode = 405;
                            respuesta = Respuesta.Error("Para eliminar se usa POST.");
                            break;
                        }

                        var aEliminar = LeerCuerpo<Lote>(context);
                        respuesta = aEliminar == null
                            ? Respuesta.Error("No llegó el lote a eliminar.")
                            : negocio.Eliminar(aEliminar.LoteId);
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

        // Lee un número de la URL. Si falta o no es número, regresa 0
        private static int LeerEntero(HttpContext context, string nombre)
        {
            int valor;
            int.TryParse(context.Request.QueryString[nombre], out valor);
            return valor;
        }

        // Lee una fecha de la URL con formato aaaa-mm-dd. Si falta o no es válida, regresa null
        private static DateTime? LeerFecha(HttpContext context, string nombre)
        {
            DateTime fecha;
            if (DateTime.TryParseExact(context.Request.QueryString[nombre], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return fecha;
            }
            return null;
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

| Pieza | Qué hace | Por qué |
|---|---|---|
| `LeerFecha` | Lee `aaaa-mm-dd` de la URL y regresa `null` si falta o no es válida | `<input type="date">` entrega ese formato. Una fecha que no llega es «sin límite» |
| `TryParseExact` con `CultureInfo.InvariantCulture` | Convierte solo ese formato | No depende de la configuración regional del servidor |
| `LeerEntero` | El de Piezas | Igual: `0` si falta |
| `exportar` | Idéntico al de Piezas | `ContentType`, `Content-Disposition` y `OutputStream`, con `return;` |

## 4. La página y el menú

**Archivo nuevo:** *Agregar → Nuevo elemento → **Formulario Web Forms con página maestra***, elige `Site.Master`, nombre `Produccion.aspx`. Reemplaza **todo el archivo** por:

```aspx
﻿<%@ Page Title="Producción" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Produccion.aspx.cs" Inherits="CursoWeb.Produccion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="mb-0">Producción</h1>
        <button type="button" id="btnNuevo" class="btn btn-primary">Nuevo lote</button>
    </div>
    <div id="divMensaje" class="alert d-none" role="alert"></div>

    <!-- Filtro del listado -->
    <div class="row g-3 align-items-end mb-3">
        <div class="col-md-4">
            <label for="cboFiltroPieza" class="form-label">Pieza</label>
            <select id="cboFiltroPieza" class="form-select"></select>
        </div>
        <div class="col-md-2">
            <label for="txtDesde" class="form-label">Desde</label>
            <input type="date" id="txtDesde" class="form-control" />
        </div>
        <div class="col-md-2">
            <label for="txtHasta" class="form-label">Hasta</label>
            <input type="date" id="txtHasta" class="form-control" />
        </div>
        <div class="col-md-4">
            <button type="button" id="btnBuscar" class="btn btn-primary">Buscar</button>
            <button type="button" id="btnLimpiarFiltro" class="btn btn-outline-secondary">Limpiar</button>
            <button type="button" id="btnExportar" class="btn btn-success">Exportar a Excel</button>
        </div>
    </div>

    <table id="tblLotes"></table>

    <!-- Modal: alta y edición -->
    <div class="modal fade" id="mdlLote" tabindex="-1" aria-labelledby="lblTituloFormulario" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="lblTituloFormulario" class="modal-title">Nuevo lote</h5>
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
                            <label for="cboPieza" class="form-label">Pieza</label>
                            <select id="cboPieza" class="form-select" disabled></select>
                        </div>
                        <div class="col-12">
                            <label for="txtNumeroLote" class="form-label">Número de lote</label>
                            <input type="text" id="txtNumeroLote" class="form-control" maxlength="50" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtCantidad" class="form-label">Cantidad</label>
                            <input type="number" id="txtCantidad" class="form-control" min="1" step="1" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtFecha" class="form-label">Fecha</label>
                            <input type="date" id="txtFecha" class="form-control" />
                        </div>
                        <div class="col-md-4">
                            <label for="cboTurno" class="form-label">Turno</label>
                            <select id="cboTurno" class="form-select">
                                <option value="">Selecciona</option>
                                <option value="1">1</option>
                                <option value="2">2</option>
                                <option value="3">3</option>
                            </select>
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
                    <h5 id="lblTituloConfirmar" class="modal-title">Eliminar lote</h5>
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
    <script src="js/produccion.js"></script>
</asp:Content>
```

Y el menú, en `Site.Master`. Antes:

```html
                        <a class="nav-link" href="Piezas.aspx">Piezas</a>
```

Después:

```html
                        <a class="nav-link" href="Piezas.aspx">Piezas</a>
                        <a class="nav-link" href="Produccion.aspx">Producción</a>
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `<input type="date">` | Calendario del navegador | Entrega `aaaa-mm-dd` siempre, sin importar la región |
| `type="number" min="1"` en la cantidad | Solo enteros desde 1 | Ayuda al usuario; la regla real sigue en Negocio |
| `cboTurno` con tres `<option>` fijos | Las opciones no vienen del servidor | Solo hay tres turnos: no hace falta un handler |
| Tres combos con `disabled` | `cboFamilia` y `cboPieza` empiezan deshabilitados | Cada uno espera al anterior |
| Ids distintos del filtro y del modal | `cboFiltroPieza`, `txtDesde`, `txtHasta` en el filtro; `cboCliente`, `cboFamilia`, `cboPieza`, `txtFecha` en el modal | Controles diferentes, con prefijo que dice qué son |

## 5. JavaScript

📖 [jQuery: `.on()`](https://api.jquery.com/on/) · [jQuery: `$.ajax()`](https://api.jquery.com/jQuery.ajax/) · [jQuery: `.val()`](https://api.jquery.com/val/) · [jQuery: `.prop()`](https://api.jquery.com/prop/) · [jQuery: `.append()`](https://api.jquery.com/append/) · [Bootstrap Table: métodos (`refresh`)](https://bootstrap-table.com/docs/api/methods/)

**Archivo nuevo:** clic derecho en la carpeta `js` → *Agregar → Nuevo elemento → Archivo JavaScript* → `produccion.js`. Contenido completo:

```javascript
let loteIdEditando = 0;
let loteAEliminar = null;
let modalLote;
let modalConfirmar;
let filtroAplicado = ''; // los filtros con los que se pidió la tabla; el Excel usa los mismos

$(function () {
    modalLote = new bootstrap.Modal('#mdlLote');
    modalConfirmar = new bootstrap.Modal('#mdlConfirmar');

    // La fecha de producción no puede ser futura: el calendario no deja elegir días posteriores a hoy
    $('#txtFecha, #txtDesde, #txtHasta').attr('max', hoy());

    $('#btnNuevo').on('click', nuevo);
    $('#btnGuardar').on('click', guardar);
    $('#btnConfirmarEliminar').on('click', confirmarEliminar);
    $('#cboCliente').on('change', cambiarCliente);
    $('#cboFamilia').on('change', cambiarFamilia);
    $('#btnBuscar').on('click', buscar);
    $('#btnLimpiarFiltro').on('click', limpiarFiltro);
    $('#btnExportar').on('click', exportar);
    $('#mdlLote').on('shown.bs.modal', function () {
        $('#cboCliente').trigger('focus');
    });

    cargarClientes();
    cargarPiezasFiltro();
    inicializarTabla();
});

// La fecha de hoy con el formato aaaa-mm-dd que usa <input type="date">
function hoy() {
    const d = new Date();
    return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
}

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

// Llena el combo de piezas del filtro con todas las piezas activas, reutilizando el handler de Piezas
function cargarPiezasFiltro() {
    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=listar',
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            const $combo = $('#cboFiltroPieza');
            $combo.empty();
            $combo.append($('<option>').val('0').text('Todas las piezas'));
            if (r.ok) {
                r.datos.forEach(function (p) {
                    $combo.append($('<option>').val(p.piezaId).text(p.numeroParte + ' · ' + p.descripcion));
                });
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las piezas del filtro (' + xhr.status + ').', 'danger');
        });
}

// Al cambiar el cliente, se piden al servidor sus familias
function cambiarCliente() {
    cargarFamilias(Number($('#cboCliente').val()) || 0, 0, 0);
}

// Al cambiar la familia, se piden al servidor sus piezas
function cambiarFamilia() {
    cargarPiezas(Number($('#cboFamilia').val()) || 0, 0);
}

// Llena el combo de familias con las del cliente. Al editar llegan también la familia y la pieza que deben quedar seleccionadas
function cargarFamilias(clienteId, familiaId, piezaId) {
    const $combo = $('#cboFamilia');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una familia'));
    $combo.prop('disabled', true);
    cargarPiezas(0, 0);
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
                cargarPiezas(familiaId, piezaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las familias (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
}

// Llena el combo de piezas con las de la familia. Al editar llega también la pieza que debe quedar seleccionada
function cargarPiezas(familiaId, piezaId) {
    const $combo = $('#cboPieza');
    $combo.empty();
    $combo.append($('<option>').val('').text('Selecciona una pieza'));
    $combo.prop('disabled', true);
    if (!familiaId) {
        return;
    }

    $.ajax({
        url: 'Handlers/PiezasHandler.ashx?accion=listar&familiaId=' + familiaId,
        type: 'GET',
        dataType: 'json'
    })
        .done(function (r) {
            // Si la familia cambió mientras llegaba la respuesta, esta ya no sirve
            if (familiaId !== (Number($('#cboFamilia').val()) || 0)) {
                return;
            }
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            if (r.datos.length === 0) {
                mostrarMensaje('Esta familia no tiene piezas activas.', 'danger', '#divMensajeModal');
                return;
            }
            r.datos.forEach(function (p) {
                $combo.append($('<option>').val(p.piezaId).text(p.numeroParte + ' · ' + p.descripcion));
            });
            $combo.prop('disabled', false);
            if (piezaId) {
                $combo.val(piezaId);
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudieron cargar las piezas (' + xhr.status + ').', 'danger', '#divMensajeModal');
        });
}

// Los filtros que hay en los controles, listos para agregarse a una URL; 0 y vacío significan "sin filtro"
function parametrosFiltro() {
    return '&piezaId=' + (Number($('#cboFiltroPieza').val()) || 0)
        + '&desde=' + $('#txtDesde').val()
        + '&hasta=' + $('#txtHasta').val();
}

// La URL del listado lleva los filtros con los que se pidió la tabla
function urlListado() {
    return 'Handlers/LotesHandler.ashx?accion=listar' + filtroAplicado;
}

// Pide la tabla otra vez, con los filtros que hay en los controles
function recargarTabla() {
    filtroAplicado = parametrosFiltro();
    $('#tblLotes').bootstrapTable('refresh', { url: urlListado() });
}

function buscar() {
    ocultarMensaje();
    const desde = $('#txtDesde').val();
    const hasta = $('#txtHasta').val();
    // Las fechas aaaa-mm-dd se pueden comparar como texto
    if (desde && hasta && desde > hasta) {
        mostrarMensaje('La fecha inicial no puede ser mayor que la final.', 'danger');
        return;
    }
    recargarTabla();
}

function limpiarFiltro() {
    $('#cboFiltroPieza').val('0');
    $('#txtDesde, #txtHasta').val('');
    buscar();
}

// Descarga el Excel con los mismos filtros que tiene la tabla
function exportar() {
    window.location = 'Handlers/LotesHandler.ashx?accion=exportar' + filtroAplicado;
}

function obtenerLote() {
    return {
        loteId: loteIdEditando,
        piezaId: Number($('#cboPieza').val()) || 0,
        numeroLote: $('#txtNumeroLote').val().trim(),
        cantidad: Number($('#txtCantidad').val()) || 0,
        fechaProduccion: $('#txtFecha').val(),
        turno: Number($('#cboTurno').val()) || 0
    };
}

function validarLote(l, clienteId, familiaId) {
    const errores = [];
    if (!clienteId) {
        errores.push('Selecciona un cliente.');
    }
    if (!familiaId) {
        errores.push('Selecciona una familia.');
    }
    if (!l.piezaId) {
        errores.push('Selecciona una pieza.');
    }
    if (!l.numeroLote) {
        errores.push('El número de lote es obligatorio.');
    }
    if (l.cantidad <= 0) {
        errores.push('La cantidad debe ser mayor a cero.');
    }
    if (!l.fechaProduccion) {
        errores.push('La fecha de producción es obligatoria.');
    } else if (l.fechaProduccion > hoy()) {
        errores.push('La fecha de producción no puede ser futura.');
    }
    if (!l.turno) {
        errores.push('Selecciona un turno.');
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
    modalLote.show();
}

function guardar() {
    ocultarMensaje('#divMensajeModal');
    const lote = obtenerLote();
    const errores = validarLote(lote, Number($('#cboCliente').val()) || 0, Number($('#cboFamilia').val()) || 0);
    if (errores.length > 0) {
        mostrarMensaje(errores.join(' '), 'danger', '#divMensajeModal');
        return;
    }
    enviarLote(lote);
}

function enviarLote(lote) {
    $('#btnGuardar').prop('disabled', true);

    $.ajax({
        url: 'Handlers/LotesHandler.ashx?accion=guardar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(lote),
        dataType: 'json'
    })
        .done(function (r) {
            if (!r.ok) {
                mostrarMensaje(r.mensaje, 'danger', '#divMensajeModal');
                return;
            }
            modalLote.hide();
            mostrarMensaje(r.mensaje, 'success');
            limpiarCampos();
            recargarTabla();
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger', '#divMensajeModal');
        })
        .always(function () {
            $('#btnGuardar').prop('disabled', false);
        });
}

function limpiarCampos() {
    $('#cboCliente, #txtNumeroLote, #txtCantidad, #txtFecha, #cboTurno').val('');
    cargarFamilias(0, 0, 0);
    loteIdEditando = 0;
    $('#lblTituloFormulario').text('Nuevo lote');
}

function inicializarTabla() {
    $('#tblLotes').bootstrapTable({
        url: urlListado(),
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
            { field: 'numeroLote', title: 'Lote', sortable: true },
            { field: 'cantidad', title: 'Cantidad', align: 'right', sortable: true },
            { field: 'fechaProduccion', title: 'Fecha', sortable: true, formatter: formatoFecha },
            { field: 'turno', title: 'Turno', align: 'center', sortable: true },
            { field: 'loteId', title: 'Acciones', align: 'right', formatter: formatoAcciones, events: eventosAcciones }
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

function editar(l) {
    loteIdEditando = l.loteId;
    $('#cboCliente').val(l.clienteId);
    cargarFamilias(l.clienteId, l.familiaId, l.piezaId);
    $('#txtNumeroLote').val(l.numeroLote);
    $('#txtCantidad').val(l.cantidad);
    $('#txtFecha').val(l.fechaProduccion.substring(0, 10));
    $('#cboTurno').val(l.turno);
    $('#lblTituloFormulario').text('Editar lote');
    ocultarMensaje();
    ocultarMensaje('#divMensajeModal');
    modalLote.show();
}

function eliminar(l) {
    loteAEliminar = l;
    $('#lblConfirmar').text('¿Eliminar el lote "' + l.numeroLote + '" de la pieza ' + l.numeroParte + '?');
    ocultarMensaje();
    modalConfirmar.show();
}

function confirmarEliminar() {
    modalConfirmar.hide();

    $.ajax({
        url: 'Handlers/LotesHandler.ashx?accion=eliminar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ loteId: loteAEliminar.loteId }),
        dataType: 'json'
    })
        .done(function (r) {
            mostrarMensaje(r.mensaje, r.ok ? 'success' : 'danger');
            if (r.ok) {
                recargarTabla();
            }
        })
        .fail(function (xhr) {
            mostrarMensaje('No se pudo comunicar con el servidor (' + xhr.status + ').', 'danger');
        });
}
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `cargarFamilias(clienteId, familiaId, piezaId)` | Llena familias y, al editar, encadena `cargarPiezas` | Tres niveles: la pieza no se puede seleccionar hasta que lleguen las familias. Es la misma asincronía del día 4, un nivel más |
| `cargarPiezas(familiaId, piezaId)` | Pide las piezas con `PiezasHandler?accion=listar&familiaId=` | **Reutiliza** el filtro de Piezas del día 5: `clienteId` no llega y vale `0` («todos») |
| Descartar la respuesta tardía en los dos niveles | Compara con el combo actual | Mismo cuidado que en Piezas |
| `cargarPiezasFiltro` | Todas las piezas, para el filtro | En el filtro basta un combo; los tres niveles son del alta |
| `hoy()` y el atributo `max` | La fecha de hoy con formato `aaaa-mm-dd` | Evita elegir una fecha futura sin escribirla a mano. Negocio la valida de todos modos |
| `desde > hasta` como texto | Compara fechas `aaaa-mm-dd` | Ese formato ordena igual como texto que como fecha |
| `filtroAplicado` | Los filtros con los que se pidió la tabla | El Excel coincide con lo que se ve, igual que en Piezas |
| `l.fechaProduccion.substring(0, 10)` al editar | Se queda con `aaaa-mm-dd` del JSON | El JSON trae `2026-11-10T00:00:00` y el `<input type="date">` solo entiende los diez primeros caracteres |

## 6. Probar

1. Ejecuta y abre **Producción**: la tabla muestra los lotes (vacía al principio).
2. **Nuevo lote**: **Familia** y **Pieza** están deshabilitadas. Elige un cliente: se habilita **Familia**. Elige una familia: se habilita **Pieza** con solo sus piezas.
3. Guarda un lote válido: aparece en la tabla.
4. Prueba las validaciones: cantidad `0`, fecha futura escrita a mano, turno sin elegir, y el mismo número de lote para la misma pieza (debe decir que ya existe; con otra pieza sí se permite).
5. **Editar**: cliente, familia y pieza quedan seleccionados. Cambia la cantidad y guarda.
6. **Eliminar** → confirmar: desaparece.
7. **Filtro**: por pieza; por rango de fechas; con «desde» posterior a «hasta» avisa.
8. **Exportar a Excel**: con y sin filtro. Revisa los acentos y la columna de fecha.

Si algo falla:

| Si pasa esto | Revisa |
|---|---|
| El combo de pieza no se llena | Que la URL sea `PiezasHandler.ashx?accion=listar&familiaId=…` (F12 → Network) y que esa familia tenga piezas activas |
| Al editar, la pieza no queda seleccionada | Que `editar` llame `cargarFamilias(l.clienteId, l.familiaId, l.piezaId)` con los tres valores |
| La fecha aparece vacía al editar | Que se recorte con `substring(0, 10)` |
| El filtro por fechas no filtra | Que las fechas viajen como `aaaa-mm-dd` y que `LeerFecha` use ese formato |
| La tabla sale vacía pero el handler sí responde | Los `field` de las columnas contra el JSON (camelCase: `numeroLote`, no `NumeroLote`) |
| Visual Studio no reconoce `Lote`, `LotesDatos`… | Que el archivo esté incluido en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*) |
| Acentos raros (`ProducciÃ³n`) | Guarda `Produccion.aspx` con *UTF-8 con firma* |
