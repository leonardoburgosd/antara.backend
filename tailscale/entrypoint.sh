#!/bin/sh
# Arranca Tailscale y, cuando el nodo está autenticado, publica el servicio web
# con "tailscale serve" usando el nombre DNS real del nodo (MagicDNS). Así no
# hace falta conocer el tailnet de antemano ni depender de certificados HTTPS.
set -u

SOCKET=/tmp/tailscaled.sock
MODO="${TAILSCALE_SERVE_MODE:-http}"
DESTINO="${TAILSCALE_SERVE_URL:-http://web:80}"

containerboot &
PID_BOOT=$!

salir() {
    kill -TERM "$PID_BOOT" 2>/dev/null
    wait "$PID_BOOT" 2>/dev/null
    exit 0
}
trap salir TERM INT

# Espera a que tailscaled tenga una sesión válida (máximo ~3 minutos).
intentos=0
while [ "$intentos" -lt 90 ]; do
    if tailscale --socket="$SOCKET" status --json 2>/dev/null | grep -q '"BackendState":"Running"'; then
        break
    fi
    sleep 2
    intentos=$((intentos + 1))
done

if tailscale --socket="$SOCKET" status --json 2>/dev/null | grep -q '"BackendState":"Running"'; then
    # Limpia configuraciones previas (por ejemplo una guardada como no-https
    # antes de habilitar HTTPS) para que el nombre publicado sea el real.
    tailscale --socket="$SOCKET" serve reset >/dev/null 2>&1 || true

    case "$MODO" in
        http)
            tailscale --socket="$SOCKET" serve --bg --http=80 "$DESTINO" || true
            ;;
        https)
            tailscale --socket="$SOCKET" serve --bg --https=443 "$DESTINO" || true
            ;;
        ninguno | none | "")
            echo "TAILSCALE_SERVE_MODE=$MODO: el panel no se publica en el tailnet."
            ;;
        *)
            echo "TAILSCALE_SERVE_MODE desconocido: $MODO (usa http, https o ninguno)."
            ;;
    esac
else
    echo "El nodo no quedó autenticado; no se publica el panel. Revisa TAILSCALE_AUTHKEY o aprobá el dispositivo."
fi

wait "$PID_BOOT"
