# Paso a paso · Día 2

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Preparar el proyecto, JavaScript y jQuery**

## 1. Preparar el proyecto: las librerías del día

Al inicio del día agregamos las tres librerías que vamos a usar:

| Librería | Para qué | Se usa en |
|---|---|---|
| jQuery | Manejar la página desde JavaScript | Bloque 1 |
| Newtonsoft.Json | Convertir objetos de C# a JSON en el handler | Bloque 2 |
| Bootstrap Table | Mostrar el listado de clientes | Bloque 3 |

### 1.1 jQuery y Bootstrap Table (archivos locales)

📖 [jQuery API](https://api.jquery.com/) · [Bootstrap Table: Getting Started](https://bootstrap-table.com/docs/getting-started/introduction/)

1. Copia las carpetas [`plantillas/lib/jquery`](../plantillas/lib/jquery/) y [`plantillas/lib/bootstrap-table`](../plantillas/lib/bootstrap-table/) a la carpeta `lib` de tu proyecto, e inclúyelas en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*).
2. En `Site.Master`, en `<head>`, **después** de `bootstrap.min.css`:
   ```html
   <link href="lib/bootstrap-table/bootstrap-table.min.css" rel="stylesheet" />
   ```
3. Al final del `<body>`, el orden completo queda así:
   ```html
   <script src="lib/jquery/jquery-3.7.1.min.js"></script>
   <script src="lib/bootstrap/js/bootstrap.bundle.min.js"></script>
   <script src="lib/bootstrap-table/bootstrap-table.min.js"></script>
   <script src="lib/bootstrap-table/locale/bootstrap-table-es-MX.min.js"></script>

   <asp:ContentPlaceHolder ID="Scripts" runat="server">
   </asp:ContentPlaceHolder>
   ```

El orden importa:
- **jQuery primero**: Bootstrap Table y el código de cada página dependen de él.
- **Bootstrap Table después de Bootstrap**, y su archivo de idioma (`es-MX`) al final, porque lo modifica.
- **El `ContentPlaceHolder` `Scripts` al último**, para que el código de cada página ya encuentre todo cargado.

### 1.2 Newtonsoft.Json (NuGet)

📖 [Instalar paquetes en Visual Studio con NuGet](https://learn.microsoft.com/es-es/nuget/consume-packages/install-use-packages-visual-studio) · [Newtonsoft.Json](https://www.newtonsoft.com/json)

Newtonsoft.Json convierte objetos de C# a JSON (y al revés). Se instala desde NuGet, el administrador de paquetes de .NET: se descarga una vez y al publicar el sitio viaja en la carpeta `bin/`.

1. Clic derecho en el proyecto `CursoWeb` → **Administrar paquetes NuGet...**
2. Pestaña **Examinar**, busca `Newtonsoft.Json`. Revisa que sea el del autor verificado (✔) y la versión **13.0.4**, y da clic en **Instalar**:

   ![Administrador de paquetes NuGet con Newtonsoft.Json seleccionado](img/nuget-01-buscar-instalar.png)

3. Al terminar, `packages.config` tiene una línea nueva:

   ![packages.config con Newtonsoft.Json 13.0.4](img/nuget-02-packages-config.png)

> Necesitas acceso a `nuget.org` solo para instalarlo. Si abres una solución descargada (por ejemplo `final/`), Visual Studio vuelve a descargar los paquetes al compilar.

### 1.3 Comprobar

F5 y en F12 → **Console**:

```javascript
$.fn.jquery
$.fn.bootstrapTable.Constructor.VERSION
```

Deben responder `'3.7.1'` y `'1.28.0'`. Newtonsoft.Json se comprueba en `packages.config`.
