# Día 6 · Excel con ClosedXML, repaso del filtro y del integrador «Producción por lote»

En esta sesión **todos parten del mismo proyecto**: la carpeta [`inicio/`](inicio/). Ya trae el filtro de Piezas y el integrador «Producción por lote» construidos, y **no trae el Excel**: ese se desarrolla en clase. También se repasan el filtro y el integrador.

## De dónde se parte

1. Descarga el repositorio actualizado (*Git → Extraer* en Visual Studio, o `git pull origin main`).
2. Abre la solución de [`inicio/CursoWeb/`](inicio/CursoWeb/). **No uses tu proyecto de días anteriores**: aunque hayas hecho todo bien, el punto de partida es este para que todos tengamos los mismos archivos.
3. En la carpeta del proyecto copia `connectionStrings.ejemplo.config` como `connectionStrings.config` (en la misma carpeta) y escribe tu contraseña, como en el día 2.
4. **Ejecuta [`sql/04-lotes.sql`](../sql/04-lotes.sql) en SSMS.** Crea la tabla `Lotes`. La pantalla **Producción** no abre sin ella. En clase se ejecuta junto con el instructor.
5. Restaura los paquetes NuGet y compila (**Ctrl+Shift+B**).

## Qué incluye `inicio/`

| Incluye | Detalle |
|---|---|
| Clientes, Familias y Piezas | Los CRUD de los días 2 a 4 |
| Filtro de Piezas | Combos de cliente y familia, botones **Buscar** y **Limpiar** |
| Producción por lote | Pantalla `Produccion.aspx` con sus capas: modelo `Lote`, `LotesDatos`, `LotesNegocio`, `LotesHandler` y `produccion.js`, además del filtro por pieza y fechas |

## Qué NO incluye (lo que falta)

| Falta | Qué se hace en clase |
|---|---|
| Tabla `Lotes` en tu base de datos | Se crea ejecutando `sql/04-lotes.sql` |
| ClosedXML | Se instala por NuGet |
| Exportar a Excel | `ExportarExcel`, la acción `exportar` del handler, el botón y la descarga en Piezas, y después en el integrador |
| Publicación en IIS | Se publica el sitio en una carpeta y se configura en IIS |

## Material de apoyo

| Material | Qué es |
|---|---|
| [`paso-a-paso.md`](paso-a-paso.md) | Cómo se construyó el integrador «Producción por lote», capa por capa. Sirve para repasarlo o para consultar el código de cada archivo |
| [`opcional-filtro-excel-con-filtro-y-lotes.md`](opcional-filtro-excel-con-filtro-y-lotes.md) | Cómo se construyó el filtro de Piezas, el Excel que respeta el filtro, y el requerimiento, la tabla y el modelo del integrador |
| [`final/`](final/) | La solución completa, con Excel. Se libera al cierre de la sesión |

> Algunos enlaces internos del segundo archivo apuntan a `../dia-4/` y a `../dia-6/paso-a-paso.md`; si alguno no abre, busca el archivo en esas carpetas.

## Resultado esperado

Piezas con filtro por cliente y familia, y una pantalla **Producción** que registra, consulta, edita y elimina lotes. Al terminar la sesión, ambas exportan a Excel y el sitio queda publicado.
