using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class FamiliasDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        public List<Familia> Listar()
        {
            const string sql = @"
SELECT
      f.FamiliaId     AS FamiliaId
    , f.ClienteId     AS ClienteId
    , c.Nombre        AS ClienteNombre
    , f.Nombre        AS Nombre
    , f.Descripcion   AS Descripcion
    , f.Activo        AS Activo
    , f.FechaAlta     AS FechaAlta
FROM
    dbo.Familias AS f
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    f.Activo = 1
ORDER BY
      c.Nombre
    , f.Nombre;";

            var lista = new List<Familia>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Familia
                        {
                            // NOT NULL en la tabla: se leen con su tipo exacto
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                            // Acepta NULL
                            Descripcion = dr["Descripcion"] as string,
                            // NOT NULL
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public List<Familia> ListarPorCliente(int clienteId)
        {
            const string sql = @"
SELECT
      f.FamiliaId     AS FamiliaId
    , f.ClienteId     AS ClienteId
    , c.Nombre        AS ClienteNombre
    , f.Nombre        AS Nombre
    , f.Descripcion   AS Descripcion
    , f.Activo        AS Activo
    , f.FechaAlta     AS FechaAlta
FROM
    dbo.Familias AS f
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    f.ClienteId = @ClienteId
    AND f.Activo = 1
ORDER BY
    f.Nombre;";

            var lista = new List<Familia>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Familia
                        {
                            // NOT NULL en la tabla: se leen con su tipo exacto
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            Nombre = dr.GetString(dr.GetOrdinal("Nombre")),
                            // Acepta NULL
                            Descripcion = dr["Descripcion"] as string,
                            // NOT NULL
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public int Insertar(Familia f)
        {
            const string sql = @"
INSERT INTO dbo.Familias (ClienteId, Nombre, Descripcion)
VALUES (@ClienteId, @Nombre, @Descripcion);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS FamiliaId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = f.ClienteId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = f.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = (object)f.Descripcion ?? DBNull.Value;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int Actualizar(Familia f)
        {
            const string sql = @"
UPDATE f
SET
      f.ClienteId     = @ClienteId
    , f.Nombre        = @Nombre
    , f.Descripcion   = @Descripcion
FROM
    dbo.Familias AS f
WHERE
    f.FamiliaId = @FamiliaId
    AND f.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = f.FamiliaId;
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = f.ClienteId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = f.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = (object)f.Descripcion ?? DBNull.Value;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int familiaId)
        {
            const string sql = @"
UPDATE f
SET
    f.Activo = 0
FROM
    dbo.Familias AS f
WHERE
    f.FamiliaId = @FamiliaId
    AND f.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = familiaId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
