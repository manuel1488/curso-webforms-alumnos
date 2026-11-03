# Demo · El contador en WinForms y en Web Forms

El mismo código en los dos mundos, para comprobar que **el servidor web no recuerda nada entre peticiones**. La explicación está en [`../apuntes.md`](../apuntes.md#sin-estado-el-contador).

Los proyectos se crean en clase; aquí están los archivos completos para reemplazar los que genera Visual Studio.

## 1. WinForms · [`winforms/`](winforms/)

| Archivo | Contenido |
|---|---|
| [`Form1.cs`](winforms/Form1.cs) | La clase del formulario con el contador |
| [`Form1.Designer.cs`](winforms/Form1.Designer.cs) | El `Label` y el `Button` ya acomodados |

1. Nuevo proyecto: **Aplicación de Windows Forms (.NET Framework)**, en C#, con el nombre `ContadorWinForms`.
2. Reemplaza el contenido de `Form1.cs` y `Form1.Designer.cs` con los de esta carpeta.
3. **F5** y tres clics: **1, 2, 3**. El formulario sigue vivo en memoria y la variable conserva su valor.

> Si tu proyecto tiene otro nombre, cambia `namespace ContadorWinForms` en los dos archivos por el nombre de tu proyecto.

## 2. Web Forms · [`webforms/`](webforms/)

| Archivo | Contenido |
|---|---|
| [`Contador.aspx`](webforms/Contador.aspx) | La página con el `Label` y el `Button` |
| [`Contador.aspx.cs`](webforms/Contador.aspx.cs) | El code-behind con el contador |
| [`Contador.aspx.designer.cs`](webforms/Contador.aspx.designer.cs) | Las declaraciones de los controles (Visual Studio lo regenera solo) |

1. En tu proyecto `CursoWeb`: clic derecho → *Agregar → Nuevo elemento → Formulario Web Forms*, con el nombre `Contador.aspx`.
2. Reemplaza el contenido de los tres archivos con los de esta carpeta.
3. Clic derecho en `Contador.aspx` → *Ver en el explorador* y tres clics: **1, 1, 1**. Cada clic es una petición nueva: ASP.NET crea la página desde cero, ejecuta el evento, genera el HTML y la destruye.

> Abre **F12 → Network** mientras das clic: cada clic aparece como una petición `POST` a `Contador.aspx`.
