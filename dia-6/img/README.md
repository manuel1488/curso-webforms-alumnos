# Capturas del día 6 (publicación)

Tómalas mientras haces tu propia publicación. Usa **Win + Shift + S**, recorta solo la ventana y, si hace falta, marca con un recuadro rojo lo que el alumno debe ver o seleccionar. Guárdalas en PNG, en esta carpeta, con estos nombres exactos (el `paso-a-paso.md` ya los referencia).

Antes de capturar, oculta o evita que salgan: contraseñas, el nombre real de tu equipo o dominio y rutas personales (`C:\Users\tu-nombre`). Si el nombre del equipo sale en la ventana, recórtalo o difumínalo.

| Archivo | Qué capturar |
|---|---|
| `00-vs-menu-publicar.png` | Menú contextual del proyecto CursoWeb con **Publicar…** resaltado (*tomada*) |
| `01a-vs-publicar-destino.png` | Ventana Publicar, paso **Destino** (*tomada*) |
| `01b-vs-publicar-ubicacion.png` | Ventana Publicar, paso **Ubicación** con `C:\sitios\CursoWeb` (*tomada*) |
| `01-vs-publicar-carpeta.png` | Resumen del perfil: ubicación de destino y configuración **Release** (*tomada*) |
| `01c-vs-publicacion-correcta.png` | Publicación correcta y ventana Salida (*tomada*) |
| `02-carpeta-publicada.png` | El Explorador de archivos en `C:\sitios\CursoWeb` con `bin`, `css`, `js`, `lib`, los `.aspx` y `Web.config` visibles (*tomada*) |
| `03a-iis-interfaz.png` | Administrador de IIS (`inetmgr`) con el árbol de Conexiones expandido (equipo, Grupos de aplicaciones, Sitios), un sitio seleccionado y el panel Acciones visibles. Pon encima, si quieres, tres recuadros que nombren las zonas |
| `03-iis-agregar-pool.png` | IIS, diálogo **Agregar grupo de aplicaciones** con nombre `CursoWeb`, `v4.0.30319` e **Integrado** |
| `03b-iis-pool-identidad.png` | IIS, **Configuración avanzada** del pool `CursoWeb`, con **Identidad** en `ApplicationPoolIdentity` a la vista |
| `04-iis-agregar-sitio.png` | IIS, diálogo **Agregar sitio web** con nombre, pool, ruta física y puerto `8081` |
| `04a`, `04b`, `04c` | Seleccionar grupo, Conectar como y Probar configuración del sitio (*tomadas*) |
| `05a-carpeta-seguridad-heredada.png` | Pestaña Seguridad con los permisos heredados de `C:\` (*tomada*) |
| `05b-carpeta-seguridad-avanzada.png` | Opciones avanzadas, columna *Heredada de* (*tomada*) |
| `05c-error-sin-permisos.png` | Error 500.19 con `0x80070005` al quitar la herencia (*tomada*) |
| `05e-carpeta-seguridad-final.png` | Seguridad con el grupo `CursoWeb` y el usuario que publica (*tomada*) |
| `05f-error-401-3.png` | Error 401.3 por el usuario anónimo (*tomada*) |
| `05g-iis-autenticacion-anonima.png` | Autenticación anónima con `IUSR` (*tomada*) |
| `06-ssms-login-pool.png` | SSMS, *Seguridad → Inicios de sesión*, con `IIS APPPOOL\CursoWeb` en la lista, **después** de crearlo |
| `07-app-publicada.png` | El navegador en `http://localhost:8081/` con la aplicación cargada y la barra de direcciones visible (*tomada*) |
| `07b-app-clientes.png` | `Clientes.aspx` publicada, con datos (*tomada*) |

## Orden recomendado

Sigue el `paso-a-paso.md` desde cero, en una máquina donde `C:\sitios\CursoWeb` **no exista** ni exista el sitio `CursoWeb` en IIS: así cada diálogo sale vacío, como lo verá el alumno. Si ya los tienes creados, bórralos primero (sitio, pool y carpeta) y vuelve a empezar.

1. Publica a carpeta: capturas `01` y `02`.
2. IIS: captura `03` antes de aceptar el diálogo; luego abre la configuración avanzada del pool para `03b`; captura `04` antes de aceptar el sitio.
3. Ejecuta `icacls`: captura `05` (clic derecho en la carpeta → Propiedades → Seguridad).
4. Ejecuta el `CREATE LOGIN` en SSMS, actualiza el Explorador de objetos y captura `06`.
5. Abre `http://localhost:8081/`: captura `07`.

## Después

Revisa que ninguna captura muestre datos que no debe y que el texto se lea al tamaño con que GitHub las muestra. Si cambia algo en una pantalla del flujo, vuelve a tomar solo esa captura con el mismo nombre.
