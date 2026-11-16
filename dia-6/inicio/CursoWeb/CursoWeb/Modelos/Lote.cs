using System;

namespace CursoWeb.Modelos
{
    public class Lote
    {
        public int LoteId { get; set; }
        public int PiezaId { get; set; }
        public string NumeroParte { get; set; }
        public int FamiliaId { get; set; }
        public string FamiliaNombre { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public string NumeroLote { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaProduccion { get; set; }
        public int Turno { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
