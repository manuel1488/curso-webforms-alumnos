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
