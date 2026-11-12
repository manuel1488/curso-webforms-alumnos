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
                        respuesta = negocio.Listar();
                        break;

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
