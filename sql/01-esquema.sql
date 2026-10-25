-- Base de datos del curso: Clientes → Familias → Piezas
IF DB_ID('CursoWeb') IS NULL
    CREATE DATABASE CursoWeb;
GO
USE CursoWeb;
GO

CREATE TABLE dbo.Clientes
(
      ClienteId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY
    , Nombre         NVARCHAR(150)     NOT NULL
    , RFC            VARCHAR(13)       NULL
    , Contacto       NVARCHAR(100)     NULL
    , Correo         VARCHAR(150)      NULL
    , Activo         BIT               NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT (1)
    , FechaAlta      DATETIME2(0)      NOT NULL CONSTRAINT DF_Clientes_FechaAlta DEFAULT (SYSDATETIME())
);
GO

CREATE TABLE dbo.Familias
(
      FamiliaId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Familias PRIMARY KEY
    , ClienteId      INT               NOT NULL CONSTRAINT FK_Familias_Clientes REFERENCES dbo.Clientes (ClienteId)
    , Nombre         NVARCHAR(100)     NOT NULL
    , Descripcion    NVARCHAR(250)     NULL
    , Activo         BIT               NOT NULL CONSTRAINT DF_Familias_Activo DEFAULT (1)
    , FechaAlta      DATETIME2(0)      NOT NULL CONSTRAINT DF_Familias_FechaAlta DEFAULT (SYSDATETIME())
);
GO

CREATE TABLE dbo.Piezas
(
      PiezaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Piezas PRIMARY KEY
    , FamiliaId      INT               NOT NULL CONSTRAINT FK_Piezas_Familias REFERENCES dbo.Familias (FamiliaId)
    , NumeroParte    VARCHAR(50)       NOT NULL
    , Descripcion    NVARCHAR(250)     NOT NULL
    , Activo         BIT               NOT NULL CONSTRAINT DF_Piezas_Activo DEFAULT (1)
    , FechaAlta      DATETIME2(0)      NOT NULL CONSTRAINT DF_Piezas_FechaAlta DEFAULT (SYSDATETIME())
    , CONSTRAINT UQ_Piezas_NumeroParte UNIQUE (NumeroParte)
);
GO
