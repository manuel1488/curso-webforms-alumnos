# Paso a paso · Día 2

Lo que construimos juntos en clase. Si te atrasas o algo no te funciona, compáralo con la solución en `final/` al cierre del día.

---

> **Bloque 1 · Preparar el proyecto, JavaScript y jQuery**

## 1. Preparar el proyecto: las librerías del día

Al inicio del día agregamos las tres librerías que vamos a usar:

| Librería | Para qué | Se usa en |
|---|---|---|
| jQuery | Manejar la página desde JavaScript | Bloque 1 |
| Newtonsoft.Json | Convertir objetos de C# a JSON en el handler | Bloque 2 |
| Bootstrap Table | Mostrar el listado de clientes | Bloque 3 |

### 1.1 jQuery y Bootstrap Table (archivos locales)

📖 [jQuery API](https://api.jquery.com/) · [Bootstrap Table: Getting Started](https://bootstrap-table.com/docs/getting-started/introduction/)

1. Copia las carpetas [`plantillas/lib/jquery`](../plantillas/lib/jquery/) y [`plantillas/lib/bootstrap-table`](../plantillas/lib/bootstrap-table/) a la carpeta `lib` de tu proyecto, e inclúyelas en el proyecto (*Mostrar todos los archivos → Incluir en el proyecto*).
2. En `Site.Master`, en `<head>`, **después** de `bootstrap.min.css`:
   ```html
   <link href="lib/bootstrap-table/bootstrap-table.min.css" rel="stylesheet" />
   ```
3. Al final del `<body>`, el orden completo queda así:
   ```html
   <script src="lib/jquery/jquery-3.7.1.min.js"></script>
   <script src="lib/bootstrap/js/bootstrap.bundle.min.js"></script>
   <script src="lib/bootstrap-table/bootstrap-table.min.js"></script>
   <script src="lib/bootstrap-table/locale/bootstrap-table-es-MX.min.js"></script>

   <asp:ContentPlaceHolder ID="Scripts" runat="server">
   </asp:ContentPlaceHolder>
   ```

El orden importa:
- **jQuery primero**: Bootstrap Table y el código de cada página dependen de él.
- **Bootstrap Table después de Bootstrap**, y su archivo de idioma (`es-MX`) al final, porque lo modifica.
- **El `ContentPlaceHolder` `Scripts` al último**, para que el código de cada página ya encuentre todo cargado.

### 1.2 Newtonsoft.Json (NuGet)

📖 [Instalar paquetes en Visual Studio con NuGet](https://learn.microsoft.com/es-es/nuget/consume-packages/install-use-packages-visual-studio) · [Newtonsoft.Json](https://www.newtonsoft.com/json)

Newtonsoft.Json convierte objetos de C# a JSON (y al revés). Se instala desde NuGet, el administrador de paquetes de .NET: se descarga una vez y al publicar el sitio viaja en la carpeta `bin/`.

1. Clic derecho en el proyecto `CursoWeb` → **Administrar paquetes NuGet...**
2. Pestaña **Examinar**, busca `Newtonsoft.Json`. Revisa que sea el del autor verificado (✔) y la versión **13.0.4**, y da clic en **Instalar**:

   ![Administrador de paquetes NuGet con Newtonsoft.Json seleccionado](img/nuget-01-buscar-instalar.png)

3. Al terminar, `packages.config` tiene una línea nueva:

   ![packages.config con Newtonsoft.Json 13.0.4](img/nuget-02-packages-config.png)

> Necesitas acceso a `nuget.org` solo para instalarlo. Si abres una solución descargada (por ejemplo `final/`), Visual Studio vuelve a descargar los paquetes al compilar.

### 1.3 Comprobar

F5 y en F12 → **Console**:

```javascript
$.fn.jquery
$.fn.bootstrapTable.Constructor.VERSION
```

Deben responder `'3.7.1'` y `'1.28.0'`. Newtonsoft.Json se comprueba en `packages.config`.

## 2. JavaScript en la consola

Todos los ejercicios van en F12 → **Console**, con `Clientes.aspx` abierta. Cada uno construye una pieza que después usaremos en `js/clientes.js`.

| # | Ejercicio | Pieza que construye | Dónde se usa después |
|---|---|---|---|
| 1 | Variables y textos | Mensajes al usuario | Validación y respuesta del servidor |
| 2 | Objetos | El objeto `cliente` | Lo que se envía al handler |
| 3 | JSON | `stringify` y la respuesta `{ ok, mensaje, datos }` | Ida y vuelta con el handler |
| 4 | Arreglos | La lista de clientes | Lo que recibe Bootstrap Table |
| 5 | Funciones | `validarCliente()` | El botón Guardar |
| 6 | La página desde JavaScript | Leer las cajas y escuchar el clic | El puente a jQuery |

### Ejercicio 1 · Variables y textos

📖 [`let`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/let) · [`const`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/const) · [`var`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/var) · [Plantillas de texto](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Template_literals)

```javascript
let nombre = 'Autopartes del Bajío';
const maxNombre = 150;
typeof nombre
typeof maxNombre
`El nombre tiene ${nombre.length} de ${maxNombre} caracteres`
```

Las comillas invertidas permiten meter variables dentro del texto con `${}`.

**`let`, `const` y `var`.** En C#, una variable vive dentro de sus llaves. En JavaScript eso es cierto para `let` y `const`, pero no para `var`:

```javascript
if (true) {
    var a = 1;
    let b = 2;
}
a
b
const limite = 150;
limite = 200;
```

`a` responde `1` porque `var` se "sale" del `if`; `b` da `ReferenceError` porque `let` solo existe dentro de sus llaves; y `limite = 200` da `TypeError` porque `const` no se puede reasignar.

| | `var` | `let` | `const` |
|---|---|---|---|
| Alcance | La función completa | Solo su bloque `{ }` | Solo su bloque `{ }` |
| Se puede reasignar | Sí | Sí | No |
| Se puede volver a declarar | Sí | No | No |
| Cuándo usarla | Evítala; la verás en código viejo | Cuando el valor va a cambiar | **Por defecto** |

Regla del curso: `const` siempre, `let` solo si el valor cambia, y `var` nunca.

> Hoy `var` está **desaconsejado**. `let` y `const` llegaron en 2015 (ES2015) justamente para corregir sus problemas de alcance, y las guías de estilo y los analizadores de código lo marcan como error (en ESLint, la regla `no-var`). Sigue funcionando solo para no romper código viejo, así que lo verás en ejemplos antiguos de internet. 📖 [ESLint: `no-var`](https://eslint.org/docs/latest/rules/no-var)

### Ejercicio 2 · Objetos

📖 [Trabajar con objetos](https://developer.mozilla.org/es/docs/Web/JavaScript/Guide/Working_with_objects)

```javascript
const cliente = {
    clienteId: 0,
    nombre: 'Autopartes del Bajío',
    rfc: 'ABA010101AA1',
    contacto: 'Laura Méndez',
    correo: 'laura@ejemplo.com'
};
cliente.nombre
cliente.rfc = 'ABA010101AA2';
cliente
```

- Las propiedades se llaman igual que las columnas de la tabla `Clientes`, en camelCase. `clienteId: 0` significa "cliente nuevo".
- `cliente` es `const` y aun así pudimos cambiar `rfc`: `const` protege la variable, no el contenido del objeto.

### Ejercicio 3 · JSON

📖 [`JSON.stringify`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify) · [`JSON.parse`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/JSON/parse)

```javascript
const texto = JSON.stringify(cliente);
texto

const respuesta = JSON.parse('{"ok":true,"mensaje":"Cliente guardado","datos":{"clienteId":3,"nombre":"Autopartes del Bajío"}}');
respuesta.ok
respuesta.mensaje
respuesta.datos.clienteId
```

`stringify` convierte el objeto en el texto que viaja al servidor; `parse` hace lo inverso con lo que regresa. Todos nuestros handlers responderán con la forma `{ ok, mensaje, datos }`.

### Ejercicio 4 · Arreglos

📖 [Array](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/Array) · [`forEach`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/Array/forEach) · [`filter`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/Array/filter) · [`find`](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Global_Objects/Array/find) · [Funciones flecha](https://developer.mozilla.org/es/docs/Web/JavaScript/Reference/Functions/Arrow_functions)

```javascript
const clientes = [
    { clienteId: 1, nombre: 'Autopartes del Bajío', rfc: 'ABA010101AA1' },
    { clienteId: 2, nombre: 'Componentes Industriales GTO', rfc: 'CIG020202BB2' }
];
clientes.length
clientes.push({ clienteId: 3, nombre: 'Ensambles León', rfc: 'ELE030303CC3' });
clientes.forEach(c => console.log(c.nombre));
clientes.filter(c => c.nombre.includes('GTO'))
clientes.find(c => c.clienteId === 2)
```

- El listado que mande el handler será un arreglo como este, y Bootstrap Table lo recibe tal cual.
- `c => ...` es una función flecha: lo mismo que una lambda de C#. `filter` es como `Where` de LINQ y `find` como `FirstOrDefault`.

### Ejercicio 5 · Funciones

📖 [Funciones](https://developer.mozilla.org/es/docs/Web/JavaScript/Guide/Functions)

```javascript
function validarCliente(c) {
    const errores = [];
    if (!c.nombre.trim()) {
        errores.push('El nombre es obligatorio.');
    }
    if (c.nombre.length > 150) {
        errores.push('El nombre no puede pasar de 150 caracteres.');
    }
    if (c.correo && !c.correo.includes('@')) {
        errores.push('El correo no es válido.');
    }
    return errores;
}

validarCliente(cliente)
validarCliente({ nombre: '   ', correo: 'sin-arroba' })
```

Regresa un arreglo de errores: vacío significa válido. Esta función la usaremos tal cual en `clientes.js`. El servidor también valida: la validación en JavaScript es comodidad, no seguridad.

### Ejercicio 6 · La página desde JavaScript

📖 [`getElementById`](https://developer.mozilla.org/es/docs/Web/API/Document/getElementById) · [`addEventListener`](https://developer.mozilla.org/es/docs/Web/API/EventTarget/addEventListener)

```javascript
document.getElementById('txtNombre').value = 'Prueba desde la consola';
document.getElementById('txtNombre').value

document.getElementById('btnGuardar').addEventListener('click', () => {
    console.log('Clic en Guardar:', document.getElementById('txtNombre').value);
});
```

Escribe algo en **Nombre** y da clic en **Guardar**: aparece en la consola.

`txtNombre` se encontró con su `id` tal cual, sin el `MainContent_` del día 1, porque es un `<input>` normal y no un control `asp:`. Con jQuery, `document.getElementById('txtNombre').value` se escribe `$('#txtNombre').val()`: es lo que sigue.
