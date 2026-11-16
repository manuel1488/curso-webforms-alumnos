using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using CursoWeb.Modelos;

namespace CursoWeb.Datos
{
    public class LotesDatos
    {
        private readonly string cadena = ConfigurationManager.ConnectionStrings["CursoWeb"].ConnectionString;

        // piezaId en 0 significa "todas"; desde y hasta en null significan "sin límite"
        public List<Lote> Listar(int piezaId, DateTime? desde, DateTime? hasta)
        {
            const string sql = @"
SELECT
      l.LoteId            AS LoteId
    , l.PiezaId           AS PiezaId
    , p.NumeroParte       AS NumeroParte
    , p.FamiliaId         AS FamiliaId
    , f.Nombre            AS FamiliaNombre
    , f.ClienteId         AS ClienteId
    , c.Nombre            AS ClienteNombre
    , l.NumeroLote        AS NumeroLote
    , l.Cantidad          AS Cantidad
    , l.FechaProduccion   AS FechaProduccion
    , l.Turno             AS Turno
    , l.Activo            AS Activo
    , l.FechaAlta         AS FechaAlta
FROM
    dbo.Lotes AS l
INNER JOIN
    dbo.Piezas AS p ON p.PiezaId = l.PiezaId
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
WHERE
    l.Activo = 1
AND (@PiezaId = 0 OR l.PiezaId = @PiezaId)
AND (@Desde IS NULL OR l.FechaProduccion >= @Desde)
AND (@Hasta IS NULL OR l.FechaProduccion <= @Hasta)
ORDER BY
      l.FechaProduccion DESC
    , l.LoteId DESC;";

            var lista = new List<Lote>();
            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = (object)desde ?? DBNull.Value;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = (object)hasta ?? DBNull.Value;

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Lote
                        {
                            // Todas estas columnas son NOT NULL: se leen con su tipo exacto
                            LoteId = dr.GetInt32(dr.GetOrdinal("LoteId")),
                            PiezaId = dr.GetInt32(dr.GetOrdinal("PiezaId")),
                            NumeroParte = dr.GetString(dr.GetOrdinal("NumeroParte")),
                            FamiliaId = dr.GetInt32(dr.GetOrdinal("FamiliaId")),
                            FamiliaNombre = dr.GetString(dr.GetOrdinal("FamiliaNombre")),
                            ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId")),
                            ClienteNombre = dr.GetString(dr.GetOrdinal("ClienteNombre")),
                            NumeroLote = dr.GetString(dr.GetOrdinal("NumeroLote")),
                            Cantidad = dr.GetInt32(dr.GetOrdinal("Cantidad")),
                            FechaProduccion = dr.GetDateTime(dr.GetOrdinal("FechaProduccion")),
                            Turno = dr.GetByte(dr.GetOrdinal("Turno")),
                            Activo = dr.GetBoolean(dr.GetOrdinal("Activo")),
                            FechaAlta = dr.GetDateTime(dr.GetOrdinal("FechaAlta"))
                        });
                    }
                }
            }
            return lista;
        }

        public int Insertar(Lote l)
        {
            const string sql = @"
INSERT INTO dbo.Lotes (PiezaId, NumeroLote, Cantidad, FechaProduccion, Turno)
VALUES (@PiezaId, @NumeroLote, @Cantidad, @FechaProduccion, @Turno);

SELECT
    CAST(SCOPE_IDENTITY() AS INT) AS LoteId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = l.PiezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = l.NumeroLote;
                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = l.Cantidad;
                cmd.Parameters.Add("@FechaProduccion", SqlDbType.Date).Value = l.FechaProduccion;
                cmd.Parameters.Add("@Turno", SqlDbType.TinyInt).Value = (byte)l.Turno;

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int Actualizar(Lote l)
        {
            const string sql = @"
UPDATE l
SET
      l.PiezaId          = @PiezaId
    , l.NumeroLote       = @NumeroLote
    , l.Cantidad         = @Cantidad
    , l.FechaProduccion  = @FechaProduccion
    , l.Turno            = @Turno
FROM
    dbo.Lotes AS l
WHERE
    l.LoteId = @LoteId
    AND l.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = l.LoteId;
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = l.PiezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = l.NumeroLote;
                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = l.Cantidad;
                cmd.Parameters.Add("@FechaProduccion", SqlDbType.Date).Value = l.FechaProduccion;
                cmd.Parameters.Add("@Turno", SqlDbType.TinyInt).Value = (byte)l.Turno;

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Desactivar(int loteId)
        {
            const string sql = @"
UPDATE l
SET
    l.Activo = 0
FROM
    dbo.Lotes AS l
WHERE
    l.LoteId = @LoteId
    AND l.Activo = 1;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = loteId;
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Cuenta los lotes con ese número para esa pieza, activos o eliminados, sin contar el que se está editando
        public bool ExisteNumeroLote(int piezaId, string numeroLote, int loteId)
        {
            const string sql = @"
SELECT
    COUNT(*) AS Total
FROM
    dbo.Lotes AS l
WHERE
    l.PiezaId = @PiezaId
    AND l.NumeroLote = @NumeroLote
    AND l.LoteId <> @LoteId;";

            using (var cn = new SqlConnection(cadena))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PiezaId", SqlDbType.Int).Value = piezaId;
                cmd.Parameters.Add("@NumeroLote", SqlDbType.VarChar, 50).Value = numeroLote;
                cmd.Parameters.Add("@LoteId", SqlDbType.Int).Value = loteId;

                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
