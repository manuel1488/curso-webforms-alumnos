using System.Collections.Generic;
using CursoWeb.Datos;
using CursoWeb.Modelos;

namespace CursoWeb.Negocio
{
    public class FamiliasNegocio
    {
        private readonly FamiliasDatos datos = new FamiliasDatos();

        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
        }

        public Respuesta ListarPorCliente(int clienteId)
        {
            if (clienteId <= 0)
            {
                return Respuesta.Error("Selecciona un cliente.");
            }
            return Respuesta.Exito("", datos.ListarPorCliente(clienteId));
        }

        public Respuesta Guardar(Familia f)
        {
            Normalizar(f);

            var errores = Validar(f);
            if (errores.Count > 0)
            {
                return Respuesta.Error(string.Join(" ", errores));
            }

            if (f.FamiliaId == 0)
            {
                f.FamiliaId = datos.Insertar(f);
                return Respuesta.Exito("Familia guardada.", f);
            }

            if (datos.Actualizar(f) == 0)
            {
                return Respuesta.Error("La familia no existe o fue eliminada.");
            }
            return Respuesta.Exito("Familia actualizada.", f);
        }

        public Respuesta Eliminar(int familiaId)
        {
            if (datos.Desactivar(familiaId) == 0)
            {
                return Respuesta.Error("La familia no existe o ya fue eliminada.");
            }
            return Respuesta.Exito("Familia eliminada.");
        }

        private static void Normalizar(Familia f)
        {
            f.Nombre = Limpiar(f.Nombre);
            f.Descripcion = Limpiar(f.Descripcion);
        }

        private static string Limpiar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            return valor.Trim();
        }

        private static List<string> Validar(Familia f)
        {
            var errores = new List<string>();

            if (f.ClienteId <= 0)
            {
                errores.Add("Selecciona un cliente.");
            }

            if (f.Nombre == null)
            {
                errores.Add("El nombre es obligatorio.");
            }
            else if (f.Nombre.Length > 100)
            {
                errores.Add("El nombre no puede pasar de 100 caracteres.");
            }

            if (f.Descripcion != null && f.Descripcion.Length > 250)
            {
                errores.Add("La descripción no puede pasar de 250 caracteres.");
            }

            return errores;
        }
    }
}
