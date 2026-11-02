# Desarrollo de Aplicaciones Web con ASP.NET Web Forms

Repositorio del curso. Aquí encontrarás la guía de cada día, los scripts de base de datos y, al terminar cada sesión, la solución completa de ese día.

## Cómo usar este repositorio

1. Antes del primer día, sigue la [guía de instalación](docs/instalacion.md).
2. Cada día tiene su carpeta (`dia-1` … `dia-6`) con objetivos, ejercicios y el resultado esperado.
3. Al cierre de cada sesión se publica `dia-N/final/`, la aplicación como debe quedar ese día.
4. Si te atrasas, abre la solución `final/` del día anterior y continúa desde ahí.
5. Si algo falla, revisa primero [troubleshooting](docs/troubleshooting.md).
6. Las tablas se crean a mano en clase. En [`sql/`](sql/README.md) están los scripts completos para comparar o ponerte al corriente.

## Contenido por día

| Día | Tema | Qué se ve | Al terminar el día |
|---|---|---|---|
| [1](dia-1/README.md) | Fundamentos e interfaz | De WinForms a la web, herramientas del navegador (F12), proyecto Web Forms, Master Page, HTML, CSS y Bootstrap | La aplicación abre con menú y la pantalla de Clientes maquetada |
| [2](dia-2/README.md) | Clientes de punta a punta | JavaScript y jQuery, tabla Clientes en SQL Server, capas de Datos y Negocio, HTTP Handler, AJAX y JSON, Bootstrap Table | Alta y listado de clientes sin recargar la página |
| [3](dia-3/README.md) | CRUD completo y Familias | Editar y eliminar, modales, tabla Familias con relación a Clientes, validaciones y manejo de errores | CRUD de Clientes y de Familias |
| [4](dia-4/README.md) | Piezas | Diseño de la solución, tabla Piezas, combos dependientes Cliente → Familia, buenas prácticas | Catálogo de Piezas completo |
| [5](dia-5/README.md) | Consolidación y exportación | Repaso del flujo completo, generación de Excel, descarga desde un handler, exportación filtrada | Exportación a Excel y requerimiento del ejercicio integrador |
| [6](dia-6/README.md) | Integrador y publicación | Ejercicio integrador, publicación desde Visual Studio, IIS y Application Pool, desarrollo vs. producción | La aplicación completa corriendo en IIS |

## Proyecto del curso

Una aplicación con tres catálogos relacionados:

```
Clientes → Familias → Piezas
```

Flujo de cada funcionalidad:

```
Página (HTML/Bootstrap) → jQuery → AJAX → Handler (.ashx) → Negocio → Datos → SQL Server
```

## Horario

6 días · 9:00 a 14:30 h · dos recesos de 15 min (10:45 y 12:45).
