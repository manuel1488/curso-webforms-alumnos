# Día 5 · Consolidación y exportación

## Objetivos
- Cerrar los tres catálogos y repasar el flujo completo.
- Exportar información filtrada a Excel.

## Temas
- Filtro en el servidor: el listado de Piezas por cliente y familia.
- Instalar una librería con NuGet (ClosedXML).
- Repaso: el recorrido de una petición desde el botón hasta SQL Server y de regreso.
- Generación de archivos Excel en el servidor.
- Descarga desde JavaScript a través de un handler.
- Exportación que respeta los filtros de la pantalla.
- Arranque del ejercicio integrador: requerimiento, tabla `Lotes` y modelo `Lote` (el resto se hace el día 6).

## Requerimiento del ejercicio integrador: Producción por lote

**Contexto.** En planta se necesita saber cuántas piezas se produjeron, en qué lote y en qué turno, para poder rastrear un problema de calidad hasta su origen.

**Requerimiento.** Construye una pantalla **Producción** que permita:

1. Registrar un lote: cliente → familia → pieza (combos dependientes), número de lote, cantidad, fecha y turno (1, 2 o 3).
2. Consultar lotes filtrando por pieza y rango de fechas.
3. Editar y eliminar un lote.
4. Exportar a Excel la consulta filtrada.

**Reglas.**
- El número de lote no se repite para la misma pieza.
- La cantidad debe ser mayor a cero.
- La fecha no puede ser futura.

**Entregable.** La funcionalidad completa siguiendo la misma arquitectura del curso: página, handler, negocio, datos y tabla en SQL Server. Tú diseñas la tabla.

## Ejercicios
1. Exporta a Excel el listado de Piezas.
2. Haz que la exportación respete el cliente y la familia seleccionados.
3. Escribe la tabla `Lotes` y la clase `Lote` del ejercicio integrador.

## Material
- [Paso a paso del día](paso-a-paso.md) (el integrador arranca en las secciones 7 a 10)

## Resultado esperado
Un botón que descarga un Excel con la información filtrada. Al final del día tienes la tabla `Lotes` creada y el modelo `Lote` listo para continuar el integrador.
