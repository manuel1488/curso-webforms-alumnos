using System;
using System.IO;
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
                        respuesta = negocio.Listar(LeerEntero(context, "clienteId"), LeerEntero(context, "familiaId"));
                        break;

                    case "exportar":
                        byte[] archivo = negocio.ExportarExcel(LeerEntero(context, "clienteId"), LeerEntero(context, "familiaId"));
                        context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        context.Response.AppendHeader("Content-Disposition", "attachment; filename=Piezas_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");
                        context.Response.OutputStream.Write(archivo, 0, archivo.Length);
                        return;

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
