using CursoWeb.Datos;
using CursoWeb.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CursoWeb.Negocio
{
    public class ClientesNegocio
    {
        private readonly ClientesDatos datos = new ClientesDatos();

        public Respuesta Listar()
        {
            return Respuesta.Exito("", datos.Listar());
        }

        public Respuesta Guardar(Cliente c)
        {
            Normalizar(c);

            var errores = Validar(c);
            if (errores.Count > 0)
            {
                return Respuesta.Error(string.Join(" ", errores));
            }

            c.ClienteId = datos.Insertar(c);
            return Respuesta.Exito("Cliente guardado.", c);
        }

        private static void Normalizar(Cliente c)
        {
            c.Nombre = Limpiar(c.Nombre);
            c.RFC = Limpiar(c.RFC)?.ToUpper();
            c.Contacto = Limpiar(c.Contacto);
            c.Correo = Limpiar(c.Correo);
        }

        private static string Limpiar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }
            return valor.Trim();
        }

        private static List<string> Validar(Cliente c)
        {
            var errores = new List<string>();

            if (c.Nombre == null)
            {
                errores.Add("El nombre es obligatorio.");
            }
            else if (c.Nombre.Length > 150)
            {
                errores.Add("El nombre no puede pasar de 150 caracteres.");
            }

            if (c.RFC != null && (c.RFC.Length < 12 || c.RFC.Length > 13))
            {
                errores.Add("El RFC debe tener 12 o 13 caracteres.");
            }

            if (c.Contacto != null && c.Contacto.Length > 100)
            {
                errores.Add("El contacto no puede pasar de 100 caracteres.");
            }

            if (c.Correo != null && (!c.Correo.Contains("@") || c.Correo.Length > 150))
            {
                errores.Add("El correo no es válido.");
            }

            return errores;
        }
    }
}