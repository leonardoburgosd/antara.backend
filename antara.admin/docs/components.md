# Componentes UI Reutilizables

## Librería (`src/components/ui/`)

Todos los componentes de la librería se exportan desde `src/components/ui/index.js`. Úsalos con:

```js
import { Button, Modal, Toast } from '../../../components/ui'
// o importación individual:
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
```

---

## Catálogo de componentes

### `<Avatar>`
Muestra la foto de perfil de un usuario o sus iniciales como fallback.

```jsx
<Avatar name="Leo García" src={user.fotoPerfil} size="md" />
// size: "sm" | "md" | "lg"
```

---

### `<Badge>`
Etiqueta de estado con colores.

```jsx
<Badge variant="emerald">Con letra</Badge>
<Badge variant="amber">Deezer</Badge>
// variant: "emerald" | "amber" | "indigo" | "red" | "default"
```

---

### `<Button>`
Botón con variantes de estilo.

```jsx
<Button onClick={handleClick} variant="primary" disabled={loading}>
  Guardar
</Button>
// variant: "primary" | "secondary" | "ghost" | "danger"
```

---

### `<ConfirmDialog>`
Diálogo de confirmación modal para acciones destructivas.

```jsx
<ConfirmDialog
  open={confirmar}
  title="Eliminar pista"
  description="Esta acción no se puede deshacer."
  onConfirm={handleEliminar}
  onCancel={() => setConfirmar(false)}
/>
```

---

### `<EmptyState>`
Estado vacío estándar para listas sin datos.

```jsx
<EmptyState
  icon={IconMusic}
  title="No hay pistas"
  description="Escanea tus rutas de audio para encontrar canciones."
  action={<button onClick={onEscanear}>Escanear</button>}
/>
```

---

### `<ErrorBanner>`
Banner de error con botón de reintento.

```jsx
<ErrorBanner message={error} onRetry={cargar} />
```

---

### `<Input>`
Campo de texto estándar.

```jsx
<Input
  value={nombre}
  onChange={(e) => setNombre(e.target.value)}
  placeholder="Nombre de la ruta"
  label="Nombre"
  error={formError.nombre}
/>
```

---

### `<Modal>`
Contenedor de modal genérico con overlay.

```jsx
<Modal open={abierto} onClose={() => setAbierto(false)} title="Editar ruta">
  {/* contenido */}
</Modal>
```

---

### `<Select>`
Select personalizado.

```jsx
<Select
  value={proveedor}
  onChange={(e) => setProveedor(e.target.value)}
  options={[
    { value: 'todos', label: 'Todos' },
    { value: 'deezer', label: 'Deezer' },
  ]}
/>
```

---

### `<Skeleton>` y `<SkeletonTable>`
Placeholders de carga animados.

```jsx
// Para tablas:
<SkeletonTable rows={6} cols={5} />

// Para elementos genéricos:
<Skeleton className="h-10 w-full rounded-2xl" />
```

---

### `<Spinner>`
Spinner de carga inline.

```jsx
<Spinner size="sm" />
// size: "sm" | "md" | "lg"
```

---

### `<Toast>`
Notificación temporal en la parte inferior de la pantalla.

```jsx
// En la página padre:
const [toast, setToast] = useState('')

<Toast message={toast} onDismiss={() => setToast('')} />

// Para mostrar:
setToast('Metadatos actualizados correctamente.')
```

El Toast se auto-descarta después de unos segundos.

---

## Sistema de iconos (`Icons.jsx`)

Todos los iconos son componentes SVG inline en `src/components/ui/Icons.jsx`.

```jsx
import { IconMusic, IconPlay, IconPause, IconVolume } from '../../../components/ui/Icons'

<IconMusic className="h-5 w-5 text-white/40" />
```

### Cómo agregar un nuevo icono

1. Encuentra el SVG del icono (Heroicons, Lucide, etc.)
2. Agrega el componente en `Icons.jsx`:

```jsx
export const IconNuevo = ({ className = '' }) => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth={1.5}
    className={className}
  >
    {/* paths del SVG */}
  </svg>
)
```

> [!IMPORTANT]
> Todos los iconos usan `stroke="currentColor"` y `fill="none"` (outline style). No uses iconos filled (sólidos) porque rompen la consistencia visual.

---

## Cuándo crear un componente en `components/ui/` vs dentro del feature

| Criterio | `components/ui/` | `features/X/components/` |
|---|---|---|
| ¿Se usa en más de un feature? | ✅ Sí | — |
| ¿Es un elemento de UI genérico? | ✅ Sí | — |
| ¿Tiene lógica específica del dominio? | ❌ No | ✅ Sí |
| ¿Solo se usa en este feature? | ❌ No | ✅ Sí |
| Ejemplo | `<Modal>`, `<Input>`, `<Toast>` | `<MiniPlayer>`, `<ModalMetadatos>` |

---

## ❌ Errores comunes con componentes

- **Crear un componente genérico en `features/X/components/`.** Si se reutiliza, va en `components/ui/`.
- **Agregar estado global a un componente de `ui/`.** Los componentes de UI son stateless o tienen estado local mínimo (ej: hover).
- **Usar CSS inline para algo que Tailwind puede hacer.** La única excepción es para valores dinámicos como gradientes (`style={{ background: \`linear-gradient(...)\` }}`).
- **Usar iconos con `fill` sólido.** Todos los iconos usan `stroke="currentColor"`.

