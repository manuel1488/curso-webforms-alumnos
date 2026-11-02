# Git básico para el curso

Git guarda versiones de tu código; GitHub es donde viven los repositorios en la nube. Hoy es una herramienta básica en cualquier equipo de desarrollo. En el curso vemos lo indispensable desde la terminal (CLI) y, al final, su equivalente en Visual Studio.

> Abre una terminal con *Visual Studio → Ver → Terminal*, o PowerShell en Windows.

## Tus dos repositorios

| Repositorio | Carpeta | Qué haces con él |
|---|---|---|
| El del curso | `C:\cursos\curso-webforms-alumnos` | Solo **traer lo nuevo** (`pull`). No hagas commits aquí. |
| Tu proyecto | `C:\cursos\CursoWeb` | Programas, haces **commits** y lo subes (`push`) a **tu propio** repo en GitHub. |

## 1. Configuración (una sola vez por equipo)

Git firma cada commit con tu nombre y correo:

```powershell
git config --global user.name "Tu Nombre"
git config --global user.email "tu-correo@ejemplo.com"
git config --global init.defaultBranch main
```

Para revisar lo que quedó:

```powershell
git config --global -l
```

`-l` es la forma corta de `--list`. Si solo quieres ver un valor: `git config --global user.name`.

## 2. Tu proyecto en Git

```powershell
cd C:\cursos\CursoWeb
git init
Copy-Item C:\cursos\curso-webforms-alumnos\plantillas\VisualStudio.gitignore .gitignore
git status
git add .
git commit -m "Día 1: Master Page y Clientes maquetado"
```

| Comando | Qué hace |
|---|---|
| `git init` | Convierte la carpeta en un repositorio (crea la carpeta oculta `.git`) |
| `Copy-Item ... .gitignore` | Copia a tu proyecto el `.gitignore` que viene en el repo del curso |
| `git status` | Muestra qué archivos cambiaron y cuáles se van a guardar |
| `git add .` | Prepara todos los cambios para el siguiente commit |
| `git commit -m "..."` | Guarda un punto en el historial con un mensaje |

### El `.gitignore`

Le dice a Git qué **no** guardar: archivos que Visual Studio genera solo (`bin/`, `obj/`, `.vs/`, `packages/`) o que son preferencias de cada usuario. Así el repositorio solo lleva tu código. **Cópialo antes del primer `git add`**; si no, esos archivos entran al historial.

El del curso está en `plantillas/VisualStudio.gitignore` y es una versión reducida de la [plantilla oficial de GitHub](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore). Visual Studio también lo crea por ti si inicializas el repositorio con *Git → Crear repositorio Git*, eligiendo la plantilla *Default (VisualStudio)*.

## 3. Subirlo a GitHub (`push`)

1. En GitHub crea un repositorio **vacío y privado**, por ejemplo `curso-webforms-practica`. No le agregues README ni `.gitignore`.
2. Conecta tu repositorio local con el de GitHub y súbelo:

```powershell
git remote add origin https://github.com/TU_USUARIO/curso-webforms-practica.git
git push -u origin main
```

De ahí en adelante, después de cada commit basta con:

```powershell
git push
```

### `git push origin main` vs. `git push -u origin main`

| Comando | Qué hace |
|---|---|
| `git push origin main` | Sube tu rama `main` al remoto `origin`. Cada vez tienes que escribir `origin main`. |
| `git push -u origin main` | Hace lo mismo y además **recuerda** que tu `main` local va con `main` de `origin` (`-u` = *upstream*). Desde ahí bastan `git push` y `git pull` sin más. |

Por eso el `-u` se usa solo en el primer push.

> En el curso trabajamos **directo en `main`** para mantenerlo simple. En un equipo de trabajo normalmente se usan ramas y *pull requests*.

## 4. El repositorio del curso (`clone`, `fetch`, `pull`)

Si todavía no lo tienes en tu equipo:

```powershell
cd C:\cursos
git clone https://github.com/manuel1488/curso-webforms-alumnos.git
```

Cada día, para traer la solución publicada:

```powershell
cd C:\cursos\curso-webforms-alumnos
git fetch
git status
git pull origin main
```

| Comando | Qué hace |
|---|---|
| `git clone <url>` | Descarga un repositorio completo de GitHub a tu equipo |
| `git fetch` | Revisa qué hay de nuevo en GitHub **sin tocar tus archivos** |
| `git status` | Después de `fetch`, te dice si estás atrás (*behind*) |
| `git pull origin main` | Trae los cambios de `main` en GitHub y los aplica en tu carpeta |

## 5. Rutina de cada día

```powershell
# Al terminar el día, en tu proyecto
cd C:\cursos\CursoWeb
git add .
git commit -m "Día 2: alta y listado de clientes"
git push

# Para traer la solución del día
cd C:\cursos\curso-webforms-alumnos
git pull origin main
```

## 6. Cuando algo sale mal

| Quiero… | Comando |
|---|---|
| Ver qué cambié desde el último commit | `git diff` |
| Deshacer los cambios de un archivo | `git restore ruta\del\archivo` |
| Ver mis commits | `git log --oneline` |

## 7. Lo mismo desde Visual Studio

| CLI | Visual Studio |
|---|---|
| `git init` | *Git → Crear repositorio Git* (incluye la plantilla `.gitignore`) |
| `git status` / `git diff` | *Ver → Cambios de Git* |
| `git add .` + `git commit` | *Cambios de Git* → mensaje → **Confirmar todo** |
| `git push` | *Git → Insertar* (*Push*) |
| `git fetch` | *Git → Recuperar* (*Fetch*) |
| `git pull` | *Git → Extraer* (*Pull*) |
| `git clone` | *Git → Clonar repositorio* |
| `git log` | *Git → Ver historial de la rama* |

## 8. Conventional Commits (solo para que sepas que existe)

Es una convención para escribir los mensajes de commit con un prefijo que dice qué tipo de cambio es:

```
feat: alta de clientes
fix: validación del correo en clientes
docs: instrucciones de instalación
```

Muchos equipos y herramientas la usan para generar historiales de versiones automáticamente. **En el curso no la usaremos**, pero conviene que la conozcas: https://www.conventionalcommits.org/es/v1.0.0/

## Vocabulario

| Término | Qué es |
|---|---|
| Repositorio | Carpeta cuyo historial de cambios guarda Git |
| Commit | Punto de guardado con un mensaje que describe el cambio |
| `main` | La rama principal del repositorio |
| Remoto (`origin`) | La copia del repositorio en GitHub |
| Clonar | Descargar un repositorio de GitHub a tu equipo |
| Fetch | Revisar qué hay nuevo en el remoto sin aplicarlo |
| Pull | Traer los cambios del remoto y aplicarlos |
| Push | Subir tus commits al remoto |

> Si en tu empresa no tienes permitido usar GitHub, descarga el material del curso con *Code → Download ZIP* desde el navegador y omite la sección 3. Los commits locales sí los puedes usar: no salen de tu equipo.
