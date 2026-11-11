# Día 4 · Piezas

## Objetivos
- Diseñar y construir un catálogo nuevo aplicando lo aprendido.
- Resolver una relación de dos niveles: Cliente → Familia → Pieza.

## Temas
- Análisis del requerimiento y diseño de la solución.
- Tabla `Piezas` con llave foránea a `Familias` y restricción `UNIQUE` para el número de parte.
- Combos dependientes: al elegir cliente se cargan sus familias.
- CRUD de Piezas.
- Revisión de código y buenas prácticas de organización.

## Ejercicios
1. Diseña la tabla, las capas y el handler de Piezas antes de programar.
2. Crea la tabla `Piezas` y comprueba la relación Cliente → Familia → Pieza con un `JOIN`.
3. Implementa el CRUD de Piezas.
4. Valida que el número de parte no se repita.

## Resultado esperado
Catálogo de Piezas funcionando con combos dependientes de Cliente y Familia.

## Material
- [Paso a paso del día](paso-a-paso.md)
