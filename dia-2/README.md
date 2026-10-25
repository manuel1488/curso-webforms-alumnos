# Día 2 · Clientes de punta a punta

## Objetivos
- Usar JavaScript y jQuery para reaccionar a eventos de la página.
- Separar el acceso a datos y la lógica de negocio en capas.
- Guardar un cliente usando AJAX, un HTTP Handler y JSON.

## Temas
- JavaScript: variables, funciones, objetos, arreglos y JSON.
- jQuery: selectores, eventos, lectura y escritura de valores, `$.ajax`.
- SQL Server: crear la base `CursoWeb` y la tabla `Clientes` con T-SQL (`CREATE TABLE`, llave primaria, `IDENTITY`, valores por defecto).
- ADO.NET: `SqlConnection`, `SqlCommand` y **parámetros** (nunca concatenar valores en el SQL).
- Capa de Datos y capa de Negocio.
- HTTP Handlers (`.ashx`).
- Bootstrap Table: carga de datos desde el handler.

## Ejercicios
1. Valida en JavaScript que el nombre del cliente no esté vacío antes de enviar.
2. Crea en SSMS la base `CursoWeb` y la tabla `Clientes`. Inserta dos registros y consulta con `SELECT`.
3. Implementa el alta de clientes desde la página hasta SQL Server.
4. Muestra el listado de clientes en Bootstrap Table.

## Resultado esperado
Puedes dar de alta un cliente desde la página y verlo en el listado sin recargar.
