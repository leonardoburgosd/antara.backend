export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/usuario/login',
    CREATE_FIRST_ADMIN: '/api/sistema/crea-primero-admin',
    HEALTH: '/api/sistema/health',
  },
  DASHBOARD: {
    GET: '/api/dashboard',
  },
  PISTAS: {
    LIST: '/api/pista/todos',
    BY_ID: (id) => `/api/pista/${id}`,
    STREAM: (id) => `/api/pista/${id}`,
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

