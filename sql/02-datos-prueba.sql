USE CursoWeb;
GO

INSERT INTO dbo.Clientes (Nombre, RFC, Contacto, Correo)
VALUES
      (N'Autopartes del Bajío',        'ABA010101AA1', N'Laura Méndez',  'laura@ejemplo.com')
    , (N'Componentes Industriales GTO', 'CIG020202BB2', N'Jorge Ramírez', 'jorge@ejemplo.com')
    , (N'Ensambles León',              'ELE030303CC3', N'Ana Torres',    'ana@ejemplo.com');

INSERT INTO dbo.Familias (ClienteId, Nombre, Descripcion)
VALUES
      (1, N'Cerraduras', N'Cerraduras de puerta')
    , (1, N'Manijas',    N'Manijas interiores y exteriores')
    , (2, N'Soportes',   N'Soportes metálicos')
    , (3, N'Arneses',    N'Arneses eléctricos');

INSERT INTO dbo.Piezas (FamiliaId, NumeroParte, Descripcion)
VALUES
      (1, 'CER-1001', N'Cerradura delantera izquierda')
    , (1, 'CER-1002', N'Cerradura delantera derecha')
    , (2, 'MAN-2001', N'Manija exterior cromada')
    , (3, 'SOP-3001', N'Soporte de motor')
    , (4, 'ARN-4001', N'Arnés de tablero');
GO

-- Comprobación
SELECT
      c.Nombre        AS Cliente
    , f.Nombre        AS Familia
    , p.NumeroParte   AS NumeroParte
    , p.Descripcion   AS Pieza
FROM
    dbo.Piezas AS p
INNER JOIN
    dbo.Familias AS f ON f.FamiliaId = p.FamiliaId
INNER JOIN
    dbo.Clientes AS c ON c.ClienteId = f.ClienteId
ORDER BY
      c.Nombre
    , f.Nombre
    , p.NumeroParte;
