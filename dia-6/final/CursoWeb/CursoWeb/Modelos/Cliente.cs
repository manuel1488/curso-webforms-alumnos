using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CursoWeb.Modelos
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; }
        public string RFC {  get; set; }
        public string Contacto { get; set; }
        public string Correo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}