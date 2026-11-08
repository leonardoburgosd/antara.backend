export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/usuario/login',
    CREATE_FIRST_ADMIN: '/api/sistema/crea-primero-admin',
    HEALTH: '/api/sistema/health',
  },
  SISTEMA: {
    UNIDADES_HOST: '/api/sistema/unidades-host',
  },
  DASHBOARD: {
    GET: '/api/dashboard',
  },
  USUARIOS: {
    LIST: '/api/usuario/todos',
    CREATE: '/api/usuario',
    BY_ID: (id) => `/api/usuario/${id}`,
    UPDATE: (id) => `/api/usuario/${id}`,
    DELETE: (id) => `/api/usuario/${id}`,
    PERFIL: '/api/usuario/perfil',
    PERFIL_PASSWORD: '/api/usuario/perfil/password',
  },
  ALBUMES: {
    LIST: '/api/album/todos',
    CREATE: '/api/album',
    BY_ID: (id) => `/api/album/${id}`,
    UPDATE: (id) => `/api/album/${id}`,
    DELETE: (id) => `/api/album/${id}`,
    PUBLICAR: (id) => `/api/album/${id}/publicar`,
  },
  PLAYLISTS: {
    LIST: '/api/playlist/todos',
    CREATE: '/api/playlist',
    BY_ID: (id) => `/api/playlist/${id}`,
    PISTAS: (id) => `/api/playlist/${id}/pistas`,
    AGREGAR_PISTA: (id) => `/api/playlist/${id}/pistas`,
    QUITAR_PISTA: (id, pistaId) => `/api/playlist/${id}/pistas/${pistaId}`,
  },
  PISTAS: {
    LIST: '/api/pista/todos',
    SUBIR_RUTAS: '/api/pista/subir/rutas',
    SUBIR: '/api/pista/subir',
    SUBIR_URL: '/api/pista/subir/url',
    BY_ID: (id) => `/api/pista/${id}`,
    STREAM: (id) => `/api/pista/${id}`,
    METADATA_PROVIDERS: '/api/pista/metadatos/proveedores',
    SEARCH_METADATA: '/api/pista/metadatos/buscar',
    UPDATE_METADATA: (id) => `/api/pista/${id}/metadatos`,
    PLAY: (id) => `/reproducir/${id}`,
  },
  RUTAS_AUDIO: {
    LIST: '/api/rutas-audio',
    CREATE: '/api/rutas-audio',
    BY_ID: (id) => `/api/rutas-audio/${id}`,
    TEST: (id) => `/api/rutas-audio/${id}/probar`,
  },
  ESCANEO_AUDIO: {
    START: '/api/escaneo-audio',
    CURRENT: '/api/escaneo-audio/actual',
    BY_ID: (id) => `/api/escaneo-audio/${id}`,
    CANCEL: (id) => `/api/escaneo-audio/${id}/cancelar`,
  },
}

