# Día 5 · Excel, integrador y publicación

## Objetivos
- Exportar información a Excel desde un handler.
- Aplicar sin ayuda lo aprendido en un ejercicio integrador.
- Publicar la aplicación en IIS.

## Temas
- Instalar una librería con NuGet (ClosedXML).
- Generación de archivos Excel en el servidor.
- Descarga desde JavaScript a través de un handler.
- Ejercicio integrador: exportar Clientes a Excel.
- Presentación del integrador opcional «Producción por lote» y creación de su tabla (`sql/04-lotes.sql`).
- Desarrollo vs. producción: `debug`, errores personalizados y cadenas de conexión.
- Publicación desde Visual Studio, Application Pool, sitio y permisos en IIS y en SQL Server.

## Ejercicios
1. Exporta a Excel el listado de Piezas.
2. **Integrador:** exporta a Excel el listado de Clientes, con Piezas como modelo.
3. Ejecuta `sql/04-lotes.sql` en SSMS para dejar lista la tabla del integrador opcional.
4. Publica la aplicación en una carpeta, crea el sitio en IIS y ábrela desde el navegador.

## Material
- [Paso a paso del día](paso-a-paso.md)
- Material opcional: [`dia-6/`](../dia-6/README.md) (filtro de Piezas y ejercicio «Producción por lote»)

## Resultado esperado
Un botón que descarga un Excel en Piezas y en Clientes, y la aplicación corriendo en IIS con los tres catálogos y la exportación.
