# Paso a paso · Día 1

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/`.

## 1. Crear el proyecto

1. En Visual Studio: *Archivo → Nuevo → Proyecto*.
2. Busca **Aplicación web ASP.NET (.NET Framework)**. Debe decir **C#**.
3. Configura:
   - Nombre del proyecto: `CursoWeb`
   - Ubicación: tu carpeta de trabajo (por ejemplo `C:\cursos`)
   - Framework: **.NET Framework 4.8**
4. En la siguiente ventana:
   - Plantilla: **Vacío**
   - En *Agregar carpetas y referencias*: marca **Web Forms**
   - Desmarca **Configurar para HTTPS**
5. Clic en *Crear*.

> Usamos la plantilla vacía para que cada archivo del proyecto lo creemos nosotros y sepamos para qué sirve.

### Qué generó Visual Studio

| Archivo | Para qué sirve |
|---|---|
| `CursoWeb.slnx` (o `.sln`) | La solución: agrupa los proyectos, igual que en WinForms |
| `CursoWeb.csproj` | El proyecto: sus archivos y referencias |
| `Web.config` | La configuración de la aplicación. Como `App.config`, pero vive en el servidor y el navegador nunca lo ve |
| `Web.Debug.config` / `Web.Release.config` | Ajustes al `Web.config` al publicar (día 6) |
| `packages.config` | Los paquetes NuGet del proyecto |
| `bin\` y `obj\` | Se generan al compilar; no se guardan en Git |

Si presionas **F5** ahora, el navegador muestra **HTTP Error 403.14 – Forbidden**. Es normal: todavía no hay ninguna página.

## 2. Primera página

1. Clic derecho en el proyecto → *Agregar → Nuevo elemento → Formulario Web Forms*, con el nombre `Default.aspx`.
2. En `Default.aspx`, dentro de `<div>`:
   ```aspx
   <h1>Hola desde el servidor</h1>
   <p>Fecha y hora del servidor: <asp:Label ID="lblFecha" runat="server"></asp:Label></p>
   ```
3. En `Default.aspx.cs`, dentro de `Page_Load`:
   ```csharp
   lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
   ```
4. **F5** y recarga varias veces: la hora cambia en cada petición.

### Anatomía de una página Web Forms

Cada página son tres archivos, con el mismo patrón que un formulario de WinForms:

| Web Forms | WinForms | Qué es |
|---|---|---|
| `Default.aspx` | Lo que ves en el diseñador | El marcado: HTML más controles de servidor |
| `Default.aspx.cs` | `Form1.cs` | El **code-behind**: tu código C# |
| `Default.aspx.designer.cs` | `Form1.Designer.cs` | Declaraciones de los controles; lo genera Visual Studio, no se edita |

La primera línea del `.aspx` une el marcado con su clase:

```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CursoWeb.Default" %>
```

- **`runat="server"`**: el servidor procesa ese elemento y lo convierte en un objeto que puedes usar desde C#. Sin `runat`, es HTML normal que el servidor no ve.
- **`ID`**: el nombre del control en C#, como `(Name)` en WinForms.
- **`<form runat="server">`**: los controles `asp:` tienen que estar dentro; solo hay uno por página.
- **`Page_Load`** se ejecuta en **cada** petición. `IsPostBack` indica si la petición vino de un botón de la misma página.

### Los controles se convierten en HTML

Abre el código fuente con **Ctrl+U**:

- El `asp:Label` llegó como `<span id="lblFecha">` y no hay nada de C#.
- El `form runat="server"` se convirtió en `<form method="post" action="./Default.aspx">`: los botones de servidor envían la página a sí misma (*postback*).
- Aparecen dos campos ocultos que agrega el `form runat="server"`: `__VIEWSTATE` y `__VIEWSTATEGENERATOR`.

| Control | HTML que genera | En WinForms se parece a |
|---|---|---|
| `asp:Label` | `<span>` | `Label` |
| `asp:TextBox` | `<input type="text">` | `TextBox` |
| `asp:Button` | `<input type="submit">` | `Button` |
| `asp:DropDownList` | `<select>` | `ComboBox` |
| `asp:CheckBox` | `<input type="checkbox">` | `CheckBox` |
| `asp:HyperLink` | `<a>` | `LinkLabel` |
| `asp:Panel` | `<div>` | `Panel` |
| `asp:HiddenField` | `<input type="hidden">` | — |
| `asp:GridView` | `<table>` | `DataGridView` |

En el curso usaremos pocos controles `asp:`. La mayor parte será HTML normal con jQuery y handlers, para controlar exactamente qué viaja entre el navegador y el servidor.

> El `id` que llega al navegador puede no ser igual al `ID` que escribiste. Dentro de una Master Page, `lblFecha` llega como algo parecido a `MainContent_lblFecha`.

### IIS Express

La página corre en `http://localhost:<puerto>/Default.aspx`; el puerto lo asigna Visual Studio a cada proyecto. `Default.aspx` es el documento por defecto, así que `http://localhost:<puerto>/` también funciona.
