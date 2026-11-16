using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CursoWeb.Modelos
{
    public class Respuesta
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public object Datos { get; set; }

        public static Respuesta Exito(string mensaje, object datos = null)
        {
            return new Respuesta { Ok = true, Mensaje = mensaje, Datos = datos };
        }

        public static Respuesta Error(string mensaje)
        {
            return new Respuesta { Ok = false, Mensaje = mensaje };
        }
    }
}