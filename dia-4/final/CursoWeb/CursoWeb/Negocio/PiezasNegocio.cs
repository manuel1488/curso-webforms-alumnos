using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class PiezasNegocio
    {
        private readonly PiezasDatos datos = new PiezasDatos();

        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
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
