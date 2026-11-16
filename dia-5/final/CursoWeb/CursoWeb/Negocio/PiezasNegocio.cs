using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class PiezasNegocio
    {
        private readonly PiezasDatos datos = new PiezasDatos();

        // clienteId y familiaId en 0 significan "todos"
        public Respuesta Listar(int clienteId, int familiaId)
        {
            return Respuesta.Exito("", datos.Listar(clienteId, familiaId));
        }

        // Genera el Excel de las piezas con los mismos filtros del listado y regresa los bytes del archivo
        public byte[] ExportarExcel(int clienteId, int familiaId)
        {
            var piezas = datos.Listar(clienteId, familiaId);

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.AddWorksheet("Piezas");

                hoja.Cell(1, 1).Value = "Cliente";
                hoja.Cell(1, 2).Value = "Familia";
                hoja.Cell(1, 3).Value = "Número de parte";
                hoja.Cell(1, 4).Value = "Descripción";
                hoja.Cell(1, 5).Value = "Alta";
                hoja.Row(1).Style.Font.Bold = true;

                int fila = 2;
                foreach (var p in piezas)
                {
                    hoja.Cell(fila, 1).Value = p.ClienteNombre;
                    hoja.Cell(fila, 2).Value = p.FamiliaNombre;
                    hoja.Cell(fila, 3).Value = p.NumeroParte;
                    hoja.Cell(fila, 4).Value = p.Descripcion;
                    hoja.Cell(fila, 5).Value = p.FechaAlta;
                    fila++;
                }

                hoja.Column(5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                hoja.Columns().AdjustToContents();

                using (var memoria = new MemoryStream())
                {
                    libro.SaveAs(memoria);
                    return memoria.ToArray();
                }
            }
        }

        public Respuesta Guardar(Pieza p)
        {
            Normalizar(p);

            var errores = Validar(p);
            if (errores.Count > 0)
            {
                return Respuesta.Error(string.Join(" ", errores));
            }

            if (datos.ExisteNumeroParte(p.NumeroParte, p.PiezaId))
            {
                return Respuesta.Error("Ya existe una pieza con ese número de parte (puede estar eliminada).");
            }

            if (p.PiezaId == 0)
            {
                p.PiezaId = datos.Insertar(p);
                return Respuesta.Exito("Pieza guardada.", p);
            }

            if (datos.Actualizar(p) == 0)
            {
                return Respuesta.Error("La pieza no existe o fue eliminada.");
            }
            return Respuesta.Exito("Pieza actualizada.", p);
        }

        public Respuesta Eliminar(int piezaId)
        {
            if (datos.Desactivar(piezaId) == 0)
            {
                return Respuesta.Error("La pieza no existe o ya fue eliminada.");
            }
            return Respuesta.Exito("Pieza eliminada.");
        }

        private static void Normalizar(Pieza p)
        {
            p.NumeroParte = Limpiar(p.NumeroParte);
            p.Descripcion = Limpiar(p.Descripcion);
        }

        private static string Limpiar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            return valor.Trim();
        }

        private static List<string> Validar(Pieza p)
        {
            var errores = new List<string>();

            if (p.FamiliaId <= 0)
            {
                errores.Add("Selecciona una familia.");
            }

            if (p.NumeroParte == null)
            {
                errores.Add("El número de parte es obligatorio.");
            }
            else if (p.NumeroParte.Length > 50)
            {
                errores.Add("El número de parte no puede pasar de 50 caracteres.");
            }

            if (p.Descripcion == null)
            {
                errores.Add("La descripción es obligatoria.");
            }
            else if (p.Descripcion.Length > 250)
            {
                errores.Add("La descripción no puede pasar de 250 caracteres.");
            }

            return errores;
        }
    }
}
