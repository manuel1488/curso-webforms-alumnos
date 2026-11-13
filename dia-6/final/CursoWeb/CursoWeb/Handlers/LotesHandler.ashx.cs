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
