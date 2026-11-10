-- Usuario dedicado de la aplicación (día 2)
-- Requiere la autenticación mixta activa:
--   SELECT SERVERPROPERTY('IsIntegratedSecurityOnly') AS SoloWindows;  -- debe dar 0
--
-- CHECK_POLICY = OFF y la contraseña 'cursoweb' son SOLO para el equipo de
-- desarrollo del curso. En un servidor real: CHECK_POLICY = ON y una contraseña fuerte.

USE master;
GO
CREATE LOGIN usrCursoWeb WITH PASSWORD = N'cursoweb', CHECK_POLICY = OFF;
GO

USE CursoWeb;
GO
CREATE USER usrCursoWeb FOR LOGIN usrCursoWeb;

-- Solo lo que la aplicación hace: sin DELETE (eliminación lógica) y sin crear ni borrar tablas
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO usrCursoWeb;
GO
