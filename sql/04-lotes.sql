-- Tabla Lotes del ejercicio integrador "Producción por lote" (material opcional, carpeta dia-6)
-- Relacionada con Piezas. Ejecútala en SSMS antes de abrir la solución de dia-6/final.
-- Requiere el esquema completo (Clientes, Familias y Piezas).
-- usrCursoWeb ya tiene permiso: el del día 2 es para todo el esquema dbo.
USE CursoWeb;
GO

CREATE TABLE dbo.Lotes
(
      LoteId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Lotes PRIMARY KEY
    , PiezaId          INT               NOT NULL CONSTRAINT FK_Lotes_Piezas REFERENCES dbo.Piezas (PiezaId)
    , NumeroLote       VARCHAR(50)       NOT NULL
    , Cantidad         INT               NOT NULL CONSTRAINT CK_Lotes_Cantidad CHECK (Cantidad > 0)
    , FechaProduccion  DATE              NOT NULL
    , Turno            TINYINT           NOT NULL CONSTRAINT CK_Lotes_Turno CHECK (Turno IN (1, 2, 3))
    , Activo           BIT               NOT NULL CONSTRAINT DF_Lotes_Activo DEFAULT (1)
    , FechaAlta        DATETIME2(0)      NOT NULL CONSTRAINT DF_Lotes_FechaAlta DEFAULT (SYSDATETIME())
    , CONSTRAINT UQ_Lotes_Pieza_NumeroLote UNIQUE (PiezaId, NumeroLote)
);
GO

-- Comprobar la relación de tres niveles: Cliente → Familia → Pieza → Lote
SELECT
      c.Nombre            AS Cliente
    , f.Nombre            AS Familia
    , p.NumeroParte       AS NumeroParte
    , l.NumeroLote        AS NumeroLote
    , l.Cantidad          AS Cantidad
    , l.FechaProduccion   AS FechaProduccion
    , l.Turno             AS Turno
FROM
    dbo.Lotes AS l
INNER JOIN
    dbo.Piezas AS p ON p.PiezaId = l.PiezaId
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId;
GO
