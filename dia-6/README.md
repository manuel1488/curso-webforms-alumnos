# Material opcional · Filtro de Piezas y «Producción por lote»

Esta carpeta **no se imparte en clase**. Es para que practiques por tu cuenta después del curso.

## Qué hay aquí

| Material | Qué es |
|---|---|
| [`opcional-filtro-excel-con-filtro-y-lotes.md`](opcional-filtro-excel-con-filtro-y-lotes.md) | Filtro por cliente y familia en el listado de Piezas, exportación a Excel que respeta el filtro y el arranque del integrador (requerimiento, tabla `Lotes` y modelo `Lote`) |
| [`paso-a-paso.md`](paso-a-paso.md) | El resto del integrador «Producción por lote»: datos, negocio, handler, página y JavaScript |
| [`final/`](final/) | La solución completa del integrador |

## Cómo usarlo

1. Descarga el repositorio actualizado (*Git → Extraer* en Visual Studio, o `git pull origin main`).
2. **Ejecuta [`sql/04-lotes.sql`](../sql/04-lotes.sql) en SSMS.** Crea la tabla `Lotes`; sin ella la solución de `final/` no funciona. En clase se ejecuta junto con el instructor.
3. Para abrir `final/`: copia `connectionStrings.ejemplo.config` como `connectionStrings.config` (en la misma carpeta) y escribe tu contraseña, como en el día 2.
4. Para practicar: parte de tu proyecto del día 5, haz primero el filtro y el Excel con filtro (secciones 1 a 6 del primer archivo) y luego intenta el integrador tú solo con el requerimiento y la tabla; compara con el paso a paso solo al terminar cada capa.

> Algunos enlaces internos del primer archivo apuntan a `../dia-4/` y a `../dia-6/paso-a-paso.md`; si alguno no abre, busca el archivo en esas carpetas.

## Resultado esperado
Piezas con filtro por cliente y familia, y una pantalla **Producción** que registra, consulta, edita, elimina y exporta lotes.
