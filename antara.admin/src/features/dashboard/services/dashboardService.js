import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'

export const dashboardService = {
  async getDashboardData() {
    const [dashboardResponse, healthResponse] = await Promise.all([
      apiClient.get(ENDPOINTS.DASHBOARD.GET),
      apiClient.get(ENDPOINTS.AUTH.HEALTH),
    ])

    if (!dashboardResponse.ok) {
      if (dashboardResponse.status === 503) {
        throw new ApiError('El sistema aún no tiene un administrador registrado.', 503)
      }
      const msg = await parseErrorMessage(dashboardResponse, 'No se pudo cargar la información del dashboard.')
      throw new ApiError(msg, dashboardResponse.status)
    }

    const dashboard = await dashboardResponse.json()
    const health = healthResponse.ok ? await healthResponse.json().catch(() => null) : null

    return { dashboard, health }
  },

  async triggerScan() {
    const response = await apiClient.post(ENDPOINTS.ESCANEO_AUDIO.START)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo iniciar el escaneo.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },
}

