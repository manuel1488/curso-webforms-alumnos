# Ejemplo · Catálogo de Piezas

Los archivos completos del catálogo de Piezas, por si te atoras al construirlo. Primero intenta hacerlo siguiendo el [paso a paso](../../paso-a-paso.md); si algo no te funciona, compara con estos.

| Archivo | Dónde va en tu proyecto |
|---|---|
| `Pieza.cs` | `Modelos/` |
| `PiezasDatos.cs` | `Datos/` |
| `PiezasNegocio.cs` | `Negocio/` |
| `PiezasHandler.ashx` y `PiezasHandler.ashx.cs` | `Handlers/` |
| `Piezas.aspx` | Raíz del proyecto (reemplaza el contenido de la tuya) |
| `piezas.js` | `js/` |
| `FamiliasDatos.cs`, `FamiliasNegocio.cs` y `FamiliasHandler.ashx.cs` | `Datos/`, `Negocio/` y `Handlers/`: reemplazan los tuyos, porque agregan `ListarPorCliente` y la acción `listarPorCliente` |

Si copias los archivos en lugar de crearlos con *Agregar → Nuevo elemento*, inclúyelos en el proyecto: **Mostrar todos los archivos** → clic derecho → **Incluir en el proyecto**. Si no, Visual Studio no los compila.
