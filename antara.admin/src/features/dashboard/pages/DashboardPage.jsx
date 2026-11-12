import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useDashboard } from '../hooks/useDashboard'
import { dashboardService } from '../services/dashboardService'
import { storage } from '../../../services/storage/localStorage'

import AmbientBackground from '../components/AmbientBackground'
import DashboardStats from '../components/DashboardStats'
import TopTracks from '../components/TopTracks'
import RecentTracks from '../components/RecentTracks'
import SystemStatus from '../components/SystemStatus'
import RecentUsers from '../components/RecentUsers'
import QuickActions from '../components/QuickActions'
import RecentAlbums from '../components/RecentAlbums'
import ComingSoon from '../components/ComingSoon'

import { Avatar } from '../../../components/ui/Avatar'
import { Toast } from '../../../components/ui/Toast'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import {
  IconGrid,
  IconUsers,
  IconDisc,
  IconMusic,
  IconList,
  IconFolder,
  IconSettings,
  IconLogout,
  IconSearch,
  IconBell,
  IconSparkles,
  IconUpload,
  IconPlus,
} from '../../../components/ui/Icons'

import { RutasAudioPage } from '../../rutas-audio'
import {
  ColaSubidasPanel,
  ColaSubidasProvider,
  PistasPage,
  SubirPistaModal,
  useColaSubidas,
} from '../../pistas'
import { UsuariosPage } from '../../usuarios'
import { AlbumesPage } from '../../albumes'
import { PlaylistsPage } from '../../playlists'
import { ConfiguracionPage } from '../../configuracion'

const NAV_ITEMS = [
  { id: 'dashboard', label: 'Dashboard', icon: IconGrid },
  { id: 'usuarios', label: 'Usuarios', icon: IconUsers },
  { id: 'albumes', label: 'Álbumes', icon: IconDisc },
  { id: 'pistas', label: 'Pistas', icon: IconMusic },
  { id: 'playlists', label: 'Playlists', icon: IconList },
  { id: 'rutas', label: 'Rutas de audio', icon: IconFolder },
  { id: 'configuracion', label: 'Configuración', icon: IconSettings },
]

export const DashboardPage = () => (
  <ColaSubidasProvider>
    <DashboardPageContent />
  </ColaSubidasProvider>
)

const DashboardPageContent = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()

  const { user, refreshUser, dashboard, health, loading, error, refetch } = useDashboard()
  const { encolar, ultimaCompletada } = useColaSubidas()

  const [activeNav, setActiveNav] = useState(() => {
    const seccion = searchParams.get('seccion')
    return seccion && NAV_ITEMS.some((item) => item.id === seccion) ? seccion : 'dashboard'
  })
  const [toast, setToast] = useState('')
  const [subirAbierto, setSubirAbierto] = useState(false)

  const today = useMemo(() => {
    const value = new Date().toLocaleDateString('es-PE', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    })
    return value.charAt(0).toUpperCase() + value.slice(1)
  }, [])

  const activeItem = NAV_ITEMS.find((item) => item.id === activeNav)

  const handleLogout = () => {
    storage.clearAuth()
    navigate('/login', { replace: true })
  }

  const handleNav = (id) => {
    setActiveNav(id)
    navigate(id === 'dashboard' ? '/dashboard' : `/dashboard?seccion=${id}`, { replace: true })
    const seccionesListas = ['dashboard', 'rutas', 'pistas', 'usuarios', 'albumes', 'playlists', 'configuracion']
    if (!seccionesListas.includes(id)) {
      const item = NAV_ITEMS.find((nav) => nav.id === id)
      setToast(`${item?.label ?? 'Sección'}: próximamente.`)
    }
  }

  const ultimaCompletadaId = ultimaCompletada?.id
  const ultimaCompletadaEn = ultimaCompletada?.completadoEn

  useEffect(() => {
    if (!ultimaCompletadaId) return

    const resultado = ultimaCompletada?.resultado
    setToast(
      resultado?.conMetadatos
        ? `«${resultado.nombre}» se subió y se completaron sus metadatos.`
        : `«${resultado?.nombre || ultimaCompletada?.nombre}» se subió a ${resultado?.rutaDestino || 'la biblioteca'}.`,
    )
    refetch()
  }, [ultimaCompletadaEn])

  const encolarSubidas = (trabajos) => {
    const cantidad = encolar(trabajos)
    if (!cantidad) return 0

    const primero = Array.isArray(trabajos) ? trabajos[0] : trabajos
    setSubirAbierto(false)
    setToast(
      cantidad === 1
        ? `«${primero?.nombre}» se agregó a la cola de subidas.`
        : `${cantidad} subidas se agregaron a la cola.`,
    )
    return cantidad
  }

  const handleQuickAction = async (action) => {
    if (action.id !== 'escanear') {
      setToast(action.message)
      return
    }

    try {
      await dashboardService.triggerScan()
      setToast('Escaneo de biblioteca iniciado.')
      handleNav('rutas')
    } catch (err) {
      setToast(err.message || 'Error al iniciar escaneo.')
      if (err.status === 409) handleNav('rutas')
    }
  }

  return (
    <div className="relative min-h-screen overflow-hidden bg-black text-white">
      <AmbientBackground />

      <div className="relative z-10 flex min-h-screen">
        {/* Sidebar Desktop */}
        <aside className="hidden w-72 shrink-0 flex-col border-r border-white/10 bg-black/50 px-6 py-8 backdrop-blur-xl lg:flex">
          <div className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-full border border-white/20 bg-white text-black">
              <span className="text-lg font-black">A</span>
            </div>
            <div>
              <p className="text-base font-semibold tracking-[0.35em]">ANTARA</p>
              <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Admin</p>
            </div>
          </div>

          <nav className="mt-12 space-y-1.5">
            {NAV_ITEMS.map((item) => {
              const active = activeNav === item.id
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => handleNav(item.id)}
                  className={`group flex w-full items-center gap-3 rounded-2xl px-4 py-3 text-left text-sm transition ${
                    active
                      ? 'border border-white/15 bg-white text-black'
                      : 'border border-transparent text-white/45 hover:border-white/10 hover:bg-white/[0.04] hover:text-white'
                  }`}
                >
                  <item.icon className="h-5 w-5" />
                  <span className="flex-1">{item.label}</span>
                  {active && <span className="h-1.5 w-1.5 rounded-full bg-black" />}
                </button>
              )
            })}
          </nav>

          <div className="mt-auto space-y-4 pt-8">
            <div className="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
              <div className="flex items-center gap-3">
                <Avatar name={user?.nombre} src={user?.fotoPerfil} size="md" />
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{user?.nombre || 'Administrador'}</p>
                  <p className="truncate text-xs text-white/35">{user?.email || 'admin@antara.app'}</p>
                </div>
              </div>
              <div className="mt-3 flex items-center justify-between rounded-full border border-white/10 px-3 py-1.5">
                <span className="text-[10px] uppercase tracking-[0.2em] text-white/35">Rol</span>
                <span className="text-[10px] uppercase tracking-[0.2em] text-white/60">{user?.tipo || 'admin'}</span>
              </div>
            </div>
            <button
              type="button"
              onClick={handleLogout}
              className="flex w-full items-center justify-center gap-2 rounded-full border border-white/10 px-4 py-3 text-sm text-white/50 transition hover:border-white/25 hover:text-white"
            >
              <IconLogout className="h-4 w-4" />
              Cerrar sesión
            </button>
          </div>
        </aside>

        {/* Main Content Area */}
        <main className="flex-1 overflow-y-auto">
          <div className="w-full px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
            {/* Mobile Header */}
            <div className="mb-6 flex items-center justify-between lg:hidden">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white text-black">
                  <span className="text-base font-black">A</span>
                </div>
                <div>
                  <p className="text-sm font-semibold tracking-[0.3em]">ANTARA</p>
                  <p className="text-[9px] uppercase tracking-[0.35em] text-white/35">Admin</p>
                </div>
              </div>
              <Avatar name={user?.nombre} src={user?.fotoPerfil} size="sm" />
            </div>

            {/* Mobile Nav Tabs */}
            <div className="mb-8 flex gap-2 overflow-x-auto pb-1 lg:hidden">
              {NAV_ITEMS.map((item) => {
                const active = activeNav === item.id
                return (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => handleNav(item.id)}
                    className={`flex shrink-0 items-center gap-2 rounded-full border px-4 py-2 text-xs transition ${
                      active ? 'border-white bg-white text-black' : 'border-white/10 text-white/45'
                    }`}
                  >
                    <item.icon className="h-4 w-4" />
                    {item.label}
                  </button>
                )
              })}
            </div>

            {/* Page Header */}
            <header className="flex flex-col gap-6">
              <div className="flex flex-col gap-5 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p className="text-[10px] uppercase tracking-[0.4em] text-white/35">Panel de control</p>
                  <h1 className="mt-2 text-4xl font-semibold tracking-tight sm:text-5xl">
                    {activeItem?.label || 'Dashboard'}
                  </h1>
                  <div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-white/35">
                    <span>{today}</span>
                    <span className="h-1 w-1 rounded-full bg-white/20" />
                    <span className="flex items-center gap-2">
                      <span
                        className={`h-1.5 w-1.5 rounded-full ${health?.activo ? 'bg-emerald-400' : 'bg-red-400'}`}
                      />
                      {health?.activo ? 'Sistema operativo' : 'Sin conexión con la API'}
                    </span>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <div className="hidden items-center gap-2 rounded-full border border-white/10 bg-white/[0.03] px-4 py-2.5 sm:flex">
                    <IconSearch className="h-4 w-4 text-white/30" />
                    <input
                      type="search"
                      placeholder="Buscar en Antara..."
                      className="w-44 bg-transparent text-sm text-white placeholder:text-white/25 focus:outline-none"
                    />
                    <span className="rounded-md border border-white/10 px-1.5 py-0.5 text-[9px] text-white/25">⌘K</span>
                  </div>
                  <button
                    type="button"
                    onClick={() => setToast('No tienes notificaciones nuevas.')}
                    className="relative flex h-11 w-11 items-center justify-center rounded-full border border-white/10 bg-white/[0.03] text-white/45 transition hover:border-white/25 hover:text-white"
                    aria-label="Notificaciones"
                  >
                    <IconBell className="h-5 w-5" />
                    <span className="absolute right-2.5 top-2.5 h-1.5 w-1.5 rounded-full bg-white" />
                  </button>
                </div>
              </div>
            </header>

            {error && (
              <div className="mt-6">
                <ErrorBanner message={error} onRetry={refetch} />
              </div>
            )}

            {/* Content view switch */}
            {activeNav !== 'dashboard' ? (
              <div className="mt-8">
                {activeNav === 'rutas' ? (
                  <RutasAudioPage onToast={setToast} />
                ) : activeNav === 'pistas' ? (
                  <PistasPage onToast={setToast} onEscanear={() => handleNav('rutas')} />
                ) : activeNav === 'usuarios' ? (
                  <UsuariosPage onToast={setToast} />
                ) : activeNav === 'albumes' ? (
                  <AlbumesPage onToast={setToast} />
                ) : activeNav === 'playlists' ? (
                  <PlaylistsPage onToast={setToast} />
                ) : activeNav === 'configuracion' ? (
                  <ConfiguracionPage onToast={setToast} onPerfilActualizado={refreshUser} />
                ) : (
                  <ComingSoon item={activeItem} />
                )}
              </div>
            ) : (
              <>
                {/* Hero / Welcome card */}
                <section className="relative mt-8 overflow-hidden rounded-[2rem] border border-white/10 bg-white/[0.03] p-6 sm:p-8 lg:p-10">
                  <div className="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-white/[0.05] blur-3xl" />
                  <div className="relative flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
                    <div className="max-w-2xl">
                      <div className="flex items-center gap-2 text-[10px] uppercase tracking-[0.35em] text-white/35">
                        <IconSparkles className="h-4 w-4" />
                        Resumen general
                      </div>
                      <h2 className="mt-4 text-3xl font-semibold tracking-tight sm:text-4xl">
                        Bienvenido de nuevo,{' '}
                        <span className="text-white/50">{user?.nombre?.split(' ')[0] || 'Administrador'}</span>
                      </h2>
                      <p className="mt-3 max-w-xl text-sm leading-6 text-white/40">
                        Este es el pulso de Antara: usuarios, catálogo y actividad reciente en un solo lugar.
                      </p>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        onClick={() => setSubirAbierto(true)}
                        className="flex items-center gap-2 rounded-full bg-white px-5 py-3 text-sm font-medium text-black transition hover:bg-white/85"
                      >
                        <IconUpload className="h-4 w-4" />
                        Subir pista
                      </button>
                      <button
                        type="button"
                        onClick={() => handleNav('albumes')}
                        className="flex items-center gap-2 rounded-full border border-white/15 px-5 py-3 text-sm text-white/70 transition hover:border-white/30 hover:text-white"
                      >
                        <IconPlus className="h-4 w-4" />
                        Crear álbum
                      </button>
                    </div>
                  </div>
                </section>

                {/* Dashboard Stats */}
                <DashboardStats dashboard={dashboard} loading={loading} />

                {/* Main 2-column grid */}
                <div className="mt-5 grid gap-5 xl:grid-cols-[1.55fr_1fr]">
                  <div className="space-y-5">
                    <TopTracks
                      topTracks={dashboard?.pistasMasReproducidas ?? []}
                      loading={loading}
                    />
                    <RecentTracks
                      recentTracks={dashboard?.pistasRecientes ?? []}
                      loading={loading}
                      onViewAll={() => handleNav('pistas')}
                    />
                  </div>

                  <div className="space-y-5">
                    <SystemStatus
                      health={health}
                      dashboard={dashboard}
                      loading={loading}
                      onRefresh={refetch}
                    />
                    <RecentUsers
                      recentUsers={dashboard?.usuariosRecientes ?? []}
                      total={dashboard?.usuariosTotales ?? 0}
                      loading={loading}
                    />
                    <QuickActions onAction={handleQuickAction} />
                  </div>
                </div>

                {/* Recent Albums */}
                <RecentAlbums
                  recentAlbums={dashboard?.albumesRecientes ?? []}
                  loading={loading}                      onViewAll={() => handleNav('albumes')}
                />
              </>
            )}
          </div>
        </main>
      </div>

      <SubirPistaModal
        isOpen={subirAbierto}
        onClose={() => setSubirAbierto(false)}
        onEncolar={encolarSubidas}
      />

      <ColaSubidasPanel />

      <Toast message={toast} onDismiss={() => setToast('')} />
    </div>
  )
}

export default DashboardPage
