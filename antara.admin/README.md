# Antara Admin

Panel de administración web para la biblioteca musical Antara, desarrollado con **React 19**, **Vite 8**, **Tailwind CSS 4** y arquitectura **Feature-Based**.

> 📚 **Documentación técnica completa disponible en [`docs/`](./docs/README.md)**

---

## 🚀 Tecnologías

- **React 19.3.1**
- **Vite 8.3.5**
- **Tailwind CSS 4.3.3**
- **React Router DOM 7.18.5**
- **Oxlint 1.89.0**

---

## 🏛️ Arquitectura Modular (Feature-Based)

El frontend está estructurado por funcionalidades independientes:

- `src/features/authentication`: Flujo de inicio de sesión, verificación y creación del primer administrador.
- `src/features/dashboard`: Shell principal, barra de navegación, resúmenes estadísticos y métricas.
- `src/features/pistas`: Catálogo de pistas, reproductor flotante con control de volumen, modal de detalles y búsqueda/actualización de metadatos (Deezer y TheAudioDB).
- `src/features/rutas-audio`: Administración de rutas locales del host y escaneo en segundo plano.
- `src/components/ui`: Componentes atómicos e interactivos reutilizables (Modales, Botones, Inputs, Sliders, Badges, etc.).
- `src/services/api`: Cliente HTTP centralizado (`apiClient`), manejo automático de 401 y catálogo de `endpoints`.

---

## 📖 Documentación para Desarrolladores

Para conocer las pautas de implementación, convenciones de código y limitaciones:

1. [Índice de Documentación](docs/README.md)
2. [Arquitectura Feature-Based y Reglas de Importación](docs/architecture.md)
3. [Guía para Construir Nuevos Features](docs/features.md)
4. [Capa de Comunicación con la API (apiClient y endpoints)](docs/api-layer.md)
5. [Catálogo de Componentes UI Reutilizables](docs/components.md)
6. [Gestión de Estado (Local y Persistencia)](docs/state.md)
7. [Sistema de Enrutamiento y Navegación](docs/routing.md)
8. [Reproductor de Audio y Metadatos](docs/audio-player.md)

---

## 💻 Desarrollo Local

```bash
npm install
npm run dev
```
