using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class PiezasDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        public List<Pieza> Listar(int clienteId, int familiaId)
        {
            const string sql = @"
SELECT
      p.PiezaId       AS PiezaId
    , p.FamiliaId     AS FamiliaId
    , f.Nombre        AS FamiliaNombre
    , f.ClienteId     AS ClienteId
    , c.Nombre        AS ClienteNombre
    , p.NumeroParte   AS NumeroParte
    , p.Descripcion   AS Descripcion
    , p.Activo        AS Activo
    , p.FechaAlta     AS FechaAlta
FROM
    dbo.Piezas AS p
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    p.Activo = 1
AND (@ClienteId = 0 OR f.ClienteId = @ClienteId)
AND (@FamiliaId = 0 OR p.FamiliaId = @FamiliaId)
ORDER BY
      c.Nombre
    , f.Nombre
    , p.NumeroParte;";

            var lista = new List<Pieza>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = familiaId;
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pieza
                        {
                            // Todas estas columnas son NOT NULL: se leen con su tipo exacto
                            PiezaId = dr.GetInt32(dr.GetOrdinal("PiezaId")),
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            FamiliaNombre = dr.GetString(dr.GetOrdinal("FamiliaNombre")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            NumeroParte = dr.GetString(dr.GetOrdinal("NumeroParte")),
                            Descripcion = dr.GetString(dr.GetOrdinal("Descripcion")),
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public int Insertar(Pieza p)
        {
            const string sql = @"
INSERT INTO dbo.Piezas (FamiliaId, NumeroParte, Descripcion)
VALUES (@FamiliaId, @NumeroParte, @Descripcion);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS PiezaId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = p.FamiliaId;
                cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = p.NumeroParte;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = p.Descripcion;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int Actualizar(Pieza p)
        {
            const string sql = @"
UPDATE p
SET
      p.FamiliaId     = @FamiliaId
    , p.NumeroParte   = @NumeroParte
    , p.Descripcion   = @Descripcion
FROM
    dbo.Piezas AS p
WHERE
    p.PiezaId = @PiezaId
    AND p.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = p.PiezaId;
                cmd.Parameters.Add("@FamiliaId", SqlDbType.Int).Value = p.FamiliaId;
                cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = p.NumeroParte;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 250).Value = p.Descripcion;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int piezaId)
        {
            const string sql = @"
UPDATE p
SET
    p.Activo = 0
FROM
    dbo.Piezas AS p
WHERE
    p.PiezaId = @PiezaId
    AND p.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Cuenta las piezas con ese número de parte, activas o eliminadas, sin contar la que se está editando
        public bool ExisteNumeroParte(string numeroParte, int piezaId)
        {
            const string sql = @"
SELECT
    COUNT(*) AS Total
FROM
    dbo.Piezas AS p
WHERE
    p.NumeroParte = @NumeroParte
    AND p.PiezaId <> @PiezaId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@NumeroParte", SqlDbType.VarChar, 50).Value = numeroParte;
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;

                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
