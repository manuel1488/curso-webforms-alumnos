using CursoWeb.Modelos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CursoWeb.Datos
{
    public class ClientesDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        public List<Cliente> Listar()
        {
            const string sql = @"
SELECT
    C.ClienteID     AS ClienteId
    ,C.Nombre       AS Nombre
    ,C.RFC          AS RFC
    ,C.Contacto     AS Contacto
    ,C.Correo       AS Correo
    ,C.Activo       AS Activo
    ,C.FechaAlta    AS FechaAlta
FROM
    dbo.Clientes AS C
WHERE
    C.Activo = 1
ORDER BY
    C.Nombre
";

            var lista = new List<Cliente>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using(var dr = cmd.ExecuteReader())
                {
                    while(dr.Read())
                    {
                        lista.Add(new Cliente
                        {
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                            RFC = dr["RFC"] as string,
                            Contacto = dr["Contacto"] as string,
                            Correo = dr["Correo"] as string,
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }

            return lista;
        }

        public int Insertar(Cliente c)
        {
            const string sql = @"
INSERT INTO dbo.Clientes (Nombre, RFC, Contacto, Correo)
VALUES (@Nombre, @RFC, @Contacto, @Correo);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS ClienteId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@RFC", SqlDbType.VarChar, 13).Value = (object)c.RFC ?? DBNull.Value;
                cmd.Parameters.Add("@Contacto", SqlDbType.NVarChar, 100).Value = (object)c.Contacto ?? DBNull.Value;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = (object)c.Correo ?? DBNull.Value;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }


        public int Actualizar(Cliente c)
        {
            const string sql = @"
UPDATE c
SET
      c.Nombre     = @Nombre
    , c.RFC        = @RFC
    , c.Contacto   = @Contacto
    , c.Correo     = @Correo
FROM
    dbo.Clientes AS c
WHERE
    c.ClienteId = @ClienteId
    AND c.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = c.ClienteId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@RFC", SqlDbType.VarChar, 13).Value = (object)c.RFC ?? DBNull.Value;
                cmd.Parameters.Add("@Contacto", SqlDbType.NVarChar, 100).Value = (object)c.Contacto ?? DBNull.Value;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = (object)c.Correo ?? DBNull.Value;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int clienteId)
        {
            const string sql = @"
UPDATE c
SET
    c.Activo = 0
FROM
    dbo.Clientes AS c
WHERE
    c.ClienteId = @ClienteId
    AND c.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }



    }
}