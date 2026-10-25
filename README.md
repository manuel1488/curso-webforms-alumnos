# Desarrollo de Aplicaciones Web con ASP.NET Web Forms

Repositorio del curso. Aquí encontrarás la guía de cada día, los scripts de base de datos y, al terminar cada sesión, la solución completa de ese día.

## Cómo usar este repositorio

1. Antes del primer día, sigue la [guía de instalación](docs/instalacion.md).
2. Cada día tiene su carpeta (`dia-1` … `dia-6`) con objetivos, ejercicios y el resultado esperado.
3. Al cierre de cada sesión se publica `dia-N/final/`, la aplicación como debe quedar ese día.
4. Si te atrasas, abre la solución `final/` del día anterior y continúa desde ahí.
5. Si algo falla, revisa primero [troubleshooting](docs/troubleshooting.md).
6. Las tablas se crean a mano en clase. En [`sql/`](sql/README.md) están los scripts completos para comparar o ponerte al corriente.

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
