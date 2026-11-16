using System;

namespace CursoWeb.Modelos
{
    public class Familia
    {
        public int FamiliaId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
