# Desarrollo de Aplicaciones Web con ASP.NET Web Forms

Repositorio del curso. Aquí encontrarás la guía de cada día, los scripts de base de datos y, al terminar cada sesión, la solución completa de ese día.

## Cómo usar este repositorio

1. Antes del primer día, sigue la [guía de instalación](docs/instalacion.md). Incluye cómo clonar este repositorio desde Visual Studio.
2. Cada día tiene su carpeta (`dia-1` … `dia-6`) con objetivos, ejercicios y el resultado esperado.
3. Al cierre de cada sesión se publica `dia-N/final/`, la aplicación como debe quedar ese día. Descárgala en Visual Studio con *Git → Extraer* (ver [Git básico](docs/git-minimo.md)).
4. Si te atrasas, abre la solución `final/` del día anterior y continúa desde ahí.
5. Si algo falla, revisa primero [troubleshooting](docs/troubleshooting.md).
6. Las tablas se crean a mano en clase. En [`sql/`](sql/README.md) están los scripts completos para comparar o ponerte al corriente.

## Contenido por día

| Día | Tema | Qué se ve | Al terminar el día |
|---|---|---|---|
| [1](dia-1/README.md) | Fundamentos e interfaz | De WinForms a la web, herramientas del navegador (F12), proyecto Web Forms, Master Page, HTML, CSS, Bootstrap y Git básico | La aplicación abre con menú y la pantalla de Clientes maquetada |
| [2](dia-2/README.md) | Clientes de punta a punta | JavaScript y jQuery, tabla Clientes en SQL Server, capas de Datos y Negocio, HTTP Handler, AJAX y JSON, Bootstrap Table | Alta y listado de clientes sin recargar la página |
| [3](dia-3/README.md) | CRUD completo y Familias | Editar y eliminar, modales, tabla Familias con relación a Clientes, validaciones y manejo de errores | CRUD de Clientes y de Familias |
| [4](dia-4/README.md) | Piezas | Diseño de la solución, tabla Piezas, combos dependientes Cliente → Familia, buenas prácticas | Catálogo de Piezas completo |
| [5](dia-5/README.md) | Consolidación y exportación | Repaso del flujo completo, generación de Excel, descarga desde un handler, exportación filtrada | Exportación a Excel y requerimiento del ejercicio integrador |
| [6](dia-6/README.md) | Integrador y publicación | Ejercicio integrador, publicación desde Visual Studio, IIS y Application Pool, desarrollo vs. producción | La aplicación completa corriendo en IIS |

## La aplicación que vamos a construir

### El problema
En una planta, cada cliente tiene sus propias piezas, y esas piezas se agrupan en familias (por ejemplo, *cerraduras* o *manijas*). Esa información suele vivir en hojas de Excel repartidas entre varias personas, sin una fuente única ni validaciones.

### Qué hace la aplicación
Es el **catálogo maestro de piezas por cliente**: un solo lugar, accesible desde el navegador, donde se registra qué piezas existen, a qué familia pertenecen y de qué cliente son. Es la base sobre la que después se puede registrar la producción y la trazabilidad; eso es justo lo que harás en el ejercicio integrador.

```
Clientes → Familias → Piezas
```

| Pantalla | Qué permite |
|---|---|
| **Clientes** | Alta, consulta, edición y baja de clientes |
| **Familias** | Lo mismo, eligiendo a qué cliente pertenece cada familia |
| **Piezas** | Lo mismo, eligiendo cliente y luego familia (combos dependientes); el número de parte no se puede repetir |
| **Exportación** | Descargar a Excel las piezas filtradas por cliente y familia |

Las bajas son lógicas: el registro no se borra, queda marcado como inactivo.

### Un recorrido de uso
1. Doy de alta el cliente *Autopartes del Bajío*.
2. Le creo la familia *Cerraduras*.
3. Agrego la pieza `CER-1001`, *Cerradura delantera izquierda*, eligiendo el cliente y la familia.
4. Consulto las piezas de ese cliente y las exporto a Excel.

### Cómo funciona por dentro
Cada acción de una pantalla sigue el mismo camino:

```
Página (HTML/Bootstrap) → jQuery → AJAX → Handler (.ashx) → Negocio → Datos → SQL Server
```

El modelo completo de la base de datos está en el [diagrama entidad-relación](sql/README.md#diagrama-entidad-relación).

### Fuera de alcance
Autenticación de usuarios y permisos. Una aplicación real los necesita, pero no forman parte de este curso.

## Horario

6 días · 9:00 a 14:30 h · dos recesos de 15 min (10:45 y 12:45).
