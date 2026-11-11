# Día 3 · CRUD completo y Familias

## Objetivos
- Completar modificar y eliminar en Clientes.
- Construir el catálogo de Familias relacionado con Clientes.
- Manejar errores y validaciones con una respuesta JSON uniforme.

## Temas
- Botones de acción dentro de Bootstrap Table.
- Modales de Bootstrap para alta y edición.
- Tabla `Familias` en SQL Server con llave foránea a `Clientes`.
- Relación Cliente → Familia: llenar un `select` desde el servidor.
- Respuesta estándar del handler: `{ ok, mensaje, datos }`.
- Validaciones en cliente y en servidor.

## Ejercicios
1. Agrega editar y eliminar en Clientes, con confirmación antes de eliminar.
2. Crea la tabla `Familias` con su llave foránea y comprueba la relación con un `JOIN`.
3. Construye Familias reutilizando la arquitectura de Clientes.
4. Impide guardar una familia sin cliente seleccionado.

## Resultado esperado
CRUD completo de Clientes y de Familias, con modales y mensajes de error claros.

## Material
- [Paso a paso del día](paso-a-paso.md)
