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
