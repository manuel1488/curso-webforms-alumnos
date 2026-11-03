# Ejemplos · Día 1

## El contador: WinForms vs. Web Forms

El mismo código en los dos mundos, para ver en vivo que **el servidor web no recuerda nada entre peticiones**. La explicación está en [`../apuntes.md`](../apuntes.md#sin-estado-el-contador).

### WinForms (`ContadorWinForms/`)

1. Abre `ContadorWinForms/ContadorWinForms.csproj` en Visual Studio.
2. Ejecuta con **F5**.
3. Da tres clics en **Sumar**: verás **1, 2, 3**. El formulario sigue vivo en memoria y la variable `contador` conserva su valor.

### Web Forms

La versión web la creamos juntos en el bloque 2, dentro del proyecto `CursoWeb` (página `Contador.aspx`). Con el mismo código, tres clics muestran **1, 1, 1**: cada clic es una petición nueva y ASP.NET crea la página desde cero.

Al terminar el día queda en `../final/` para que la compares.
