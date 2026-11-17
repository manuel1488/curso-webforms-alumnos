# Paso a paso · Día 6 · Publicación

Lo que hacemos juntos en la segunda mitad del día: dejar la aplicación publicada y corriendo en IIS, sin Visual Studio. Trabajas **sobre el proyecto del día 6**: el de [`inicio/`](inicio/) con el Excel que agregaste en clase.

Antes de empezar, comprueba que Piezas y Producción exportan a Excel y que la aplicación compila. Si algo falla, compara con [`final/`](final/).

---

> **Bloque 2 · Conceptos de publicación y publicar a una carpeta**

## 1. De desarrollo a producción

📖 [Microsoft: implementación en prueba en IIS local](https://learn.microsoft.com/es-es/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/deploying-to-iis) (tutorial de 2019 con Visual Studio 2017: sirve de referencia, algunos pasos cambian)

Hasta hoy la aplicación corre en IIS Express, en tu máquina, con tu usuario. **Publicar** es dejarla en un servidor para que otros la usen. Cambian tres cosas: quién la ejecuta, cómo se compila y dónde está la base de datos.

| Tema | En desarrollo | En producción |
|---|---|---|
| Servidor | IIS Express, con tu usuario de Windows | IIS, con la **identidad del Application Pool** |
| `debug` en `Web.config` | `true` | `false` |
| Errores | Se ve el mensaje técnico | `customErrors` oculta el detalle |
| Cadena de conexión | Tu base y tu usuario de desarrollo | La de producción |
| Librerías | Las del proyecto | Las de `bin/`: el servidor no necesita internet ni NuGet |

`connectionStrings.config` no se sube a Git, pero **si está incluido en el proyecto sí se copia al publicar**, con la contraseña de desarrollo. En el servidor se reemplaza por la de producción.

## 2. Publicar a una carpeta

1. En Visual Studio: clic derecho en el proyecto **CursoWeb** → *Publicar…* → destino **Carpeta**.
2. Ruta: `C:\sitios\CursoWeb`. Configuración **Release**. El porqué de esta ruta está en el recuadro de abajo.
3. **Publicar** y abre la carpeta:

| Qué revisar | Qué debe verse |
|---|---|
| `bin/` | Las DLL del proyecto y de las librerías (`CursoWeb.dll`, `ClosedXML.dll`…) |
| `.aspx`, `.ashx`, `Site.Master` | Sí están |
| Archivos `.cs` | **No** están: ya se compilaron dentro de `CursoWeb.dll` |
| `lib/`, `css/`, `js/` | Sí están |
| `Web.config` | Sin `debug="true"` |

**¿Por qué `C:\sitios\CursoWeb` y no `C:\inetpub`?**

Al activar IIS, Windows crea `C:\inetpub`. Dentro está `C:\inetpub\wwwroot`, la carpeta del *Default Web Site* (el sitio que IIS trae por omisión, en el puerto 80). En muchos servidores las aplicaciones se publican ahí, en una subcarpeta, así que es normal encontrarlas en esa ruta.

En el curso usamos otra carpeta por tres razones:

| Razón | Explicación |
|---|---|
| Publicar sin ser administrador | Escribir en `C:\inetpub` normalmente exige permisos de administrador. Con `C:\sitios` Visual Studio publica con tu usuario de siempre, sin abrirlo como administrador |
| No mezclar con el sitio por omisión | `C:\inetpub\wwwroot` ya es de otro sitio. Una carpeta propia deja claro qué archivos son de `CursoWeb` y es fácil de borrar y volver a publicar |
| Sacar el contenido web del disco del sistema | Es una recomendación habitual de seguridad para IIS: mantener los sitios fuera de la carpeta del sistema. En un servidor real suele ser otro disco |

La ruta no cambia nada de lo demás: IIS sirve el sitio desde donde le indiques en **Ruta de acceso física**, y los permisos de `IIS AppPool\CursoWeb` se dan sobre esa carpeta, esté donde esté. Si algún día publicas en `C:\inetpub\wwwroot\CursoWeb`, solo cambia la ruta en el publicar, en el sitio de IIS, en el `icacls` y en la ruta del `connectionStrings.config` publicado.

---

> **Bloque 3 · IIS y cierre**

## 3. IIS: Application Pool, sitio y permisos de carpeta

📖 [Microsoft: identidades del grupo de aplicaciones](https://learn.microsoft.com/es-es/iis/manage/configuring-security/application-pool-identities)

Necesitas IIS con **ASP.NET 4.8** activado (*Activar o desactivar las características de Windows*) y permisos de administrador. Si no puedes instalarlo, sigue la clase en la pantalla del instructor.

**Application Pool.** *Administrador de IIS → Grupos de aplicaciones → Agregar grupo de aplicaciones…*

| Campo | Valor | Por qué |
|---|---|---|
| Nombre | `CursoWeb` | La identidad se llamará `IIS AppPool\CursoWeb` |
| Versión de .NET CLR | `v4.0.30319` | Es el runtime de .NET Framework 4.x |
| Modo de canalización | **Integrado** | El modo normal de las aplicaciones actuales |

**Sitio.** *Sitios → Agregar sitio web…*

| Campo | Valor |
|---|---|
| Nombre del sitio | `CursoWeb` |
| Grupo de aplicaciones | `CursoWeb` |
| Ruta de acceso física | `C:\sitios\CursoWeb` (la carpeta publicada) |
| Puerto | `8081` |

**Permisos de la carpeta.** La identidad del pool necesita leerla:

```powershell
icacls "C:\sitios\CursoWeb" /grant "IIS AppPool\CursoWeb:(OI)(CI)RX"
```

## 4. Permisos en SQL Server

📖 [Microsoft: `CREATE LOGIN`](https://learn.microsoft.com/es-es/sql/t-sql/statements/create-login-transact-sql) · [Microsoft: `GRANT` permisos de esquema](https://learn.microsoft.com/es-es/sql/t-sql/statements/grant-schema-permissions-transact-sql)

La identidad del pool también necesita entrar a SQL Server, con los **mismos permisos mínimos** que `usrCursoWeb`. En SSMS, como administrador:

```sql
USE master;
GO
CREATE LOGIN [IIS APPPOOL\CursoWeb] FROM WINDOWS;
GO

USE CursoWeb;
GO
CREATE USER [IIS APPPOOL\CursoWeb] FOR LOGIN [IIS APPPOOL\CursoWeb];
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO [IIS APPPOOL\CursoWeb];
GO
```

| Pieza | Qué hace | Por qué |
|---|---|---|
| `CREATE LOGIN … FROM WINDOWS` | Da de alta en el **servidor** a una identidad de Windows | La identidad es la del pool: no hay contraseña que guardar ni filtrar |
| `CREATE USER … FOR LOGIN` | La mapea a un usuario **dentro de la base** | Login = entrar al servidor; usuario = estar en una base |
| `GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo` | Los mismos permisos de `usrCursoWeb` | Mínimo privilegio, sin `DELETE`. Cubre también la tabla `Lotes` |

Luego, en la **carpeta publicada** (no en el proyecto), cambia `C:\sitios\CursoWeb\connectionStrings.config` para usar la identidad del pool:

```xml
<add name="CursoWeb" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=CursoWeb;Integrated Security=SSPI"
     providerName="System.Data.SqlClient"/>
```

Si falla el acceso por identidad, deja el `connectionStrings.config` publicado como se copió (con `usrCursoWeb`) y sigue: la aplicación funciona igual. En producción se usa la identidad del pool.

## 5. Probar la aplicación publicada y qué sigue

Abre `http://localhost:8081/`. La aplicación corre **sin Visual Studio**.

1. `Default.aspx` carga el menú con estilos.
2. Clientes lista, guarda, edita y elimina.
3. **Exportar a Excel** en Piezas y en Producción descarga el archivo, con y sin filtro.
4. Producción registra un lote y lo muestra en la tabla.
5. F12 → Network: las llamadas a los handlers regresan 200.

| Síntoma | Qué revisar |
|---|---|
| **HTTP 500.19** | ASP.NET 4.8 activado y el pool en `v4.0.30319` |
| **403** o acceso denegado | Permisos de carpeta de `IIS AppPool\CursoWeb` |
| La tabla sale vacía y hay un 500 en Network | La cadena de conexión publicada y el login: busca *Login failed for user 'IIS APPPOOL\CursoWeb'* |
| «Ocurrió un error en el servidor» | Es el `catch` del handler: con `debug="false"` no muestra el detalle. Prueba con `usrCursoWeb` |
| Estilos o scripts no cargan | Que `lib/`, `css/` y `js/` estén en la carpeta publicada |
| «No se puede cargar el archivo o ensamblado» | Que `bin/` tenga las DLL de ClosedXML |
| Producción falla con un error de tabla | Que `sql/04-lotes.sql` se ejecutó en la base `CursoWeb` |

**Qué sigue.** El mismo `Listar()` de Clientes con Entity Framework 6 en lugar de ADO.NET, las migraciones de base de datos y Web API 2 como forma moderna de hacer lo que hicimos con los `.ashx`. No se instala nada hoy: queda como camino para seguir aprendiendo.
