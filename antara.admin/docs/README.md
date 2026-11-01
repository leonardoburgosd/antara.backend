# Antara Admin — Documentación Técnica

**Panel de administración** de la biblioteca musical Antara, desarrollado con React 19 + Vite 8 + Tailwind CSS 4.

> [!NOTE]
> Esta documentación describe la arquitectura, convenciones y flujos reales del proyecto frontend. Está pensada para que cualquier desarrollador nuevo pueda agregar funcionalidades sin romper lo existente.

---

## Tecnologías (versiones exactas)

| Paquete | Versión | Rol |
|---|---|---|
| `react` | ^19.3.1 | Librería de UI |
| `react-dom` | ^19.3.1 | Renderizador DOM |
| `react-router-dom` | ^7.18.5 | Enrutamiento |
| `tailwindcss` | ^4.3.3 | Sistema de estilos utilitario |
| `@tailwindcss/vite` | ^4.3.3 | Plugin de Tailwind para Vite |
| `vite` | ^8.3.5 | Build tool y dev server |
| `@vitejs/plugin-react` | ^6.1.2 | Plugin React para Vite (SWC) |
| `oxlint` | ^1.89.0 | Linter de JavaScript/JSX |

> [!IMPORTANT]
> No hay Redux, Zustand, React Query, ni Axios. El estado es local con React hooks y las peticiones HTTP usan `fetch` nativo a través de `apiClient.js`.

---

## Índice de documentación

| Documento | Contenido |
|---|---|
| [architecture.md](./architecture.md) | Feature-Based Architecture, reglas de importación, flujo UI→API |
| [features.md](./features.md) | Guía paso a paso para agregar una nueva funcionalidad |
| [api-layer.md](./api-layer.md) | apiClient, endpoints, manejo de errores, reglas de fetch |
| [components.md](./components.md) | Librería UI reutilizable, props, iconos |
| [state.md](./state.md) | Estado local, localStorage, cuándo extraer a hooks |
| [routing.md](./routing.md) | Router, rutas protegidas, navegación por secciones |
| [audio-player.md](./audio-player.md) | useAudioPlayer, MiniPlayer, ModalMetadatos, volumen |

---

## Estructura del proyecto

```
antara.admin/
├── src/
│   ├── app/                      ← Configuración de la app
│   │   ├── App.jsx               ← BrowserRouter + AppRouter
│   │   ├── router/               ← Definición de rutas
│   │   └── layouts/              ← MainLayout, AuthLayout
│   ├── features/                 ← Una carpeta por funcionalidad
│   │   ├── authentication/
│   │   ├── dashboard/
│   │   ├── pistas/
│   │   └── rutas-audio/
│   ├── components/ui/            ← Componentes reutilizables globales
│   ├── services/api/             ← Capa de comunicación HTTP
│   ├── hooks/                    ← Hooks globales reutilizables
│   ├── utils/                    ← Formateadores, constantes, helpers
│   └── main.jsx                  ← Punto de entrada
├── docs/                         ← Esta documentación
├── Dockerfile                    ← Build multi-stage (node → nginx)
└── nginx.conf                    ← Config nginx para SPA
```

---

## Inicio rápido de desarrollo

```bash
# En antara.admin/
npm install
npm run dev      # http://localhost:5173
npm run build    # Genera dist/
npm run lint     # Ejecuta oxlint
```

La API debe estar corriendo en `http://localhost:8080` (configurable en `src/services/api/apiConfig.js`).

