#!/usr/bin/env bash
# Genera docker-compose.unidades.yml con un bind por unidad del host
# (X:/ -> /host/x) y levanta el stack con ese archivo adicional. Así el
# docker-compose.yml no registra letras de unidad en crudo: los montajes salen
# de lo que exista realmente en la máquina.
#
# Los montajes son de escritura porque el panel sube canciones a esas carpetas;
# usa UNIDADES_SOLO_LECTURA=1 si preferís montarlas de solo lectura.
#
# Uso:
#   ./scripts/montar-unidades.sh              # detecta y levanta el stack
#   ./scripts/montar-unidades.sh --build      # argumentos extra para docker compose up
#   UNIDADES="c,f,h" ./scripts/montar-unidades.sh   # fuerza la lista de unidades
#   RUTAS_EXTRA="/mnt/musica" ./scripts/montar-unidades.sh  # carpetas extra (Linux/macOS)
#   UNIDADES_SOLO_LECTURA=1 ./scripts/montar-unidades.sh    # sin escritura
#
# Volver a ejecutarlo es la forma de aplicar una unidad nueva: reescribe el
# archivo y recrea el servicio api.

set -euo pipefail

RAIZ_PROYECTO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SALIDA="$RAIZ_PROYECTO/docker-compose.unidades.yml"
BASE="$RAIZ_PROYECTO/docker-compose.yml"

detectar_unidades() {
  if [ -n "${UNIDADES:-}" ]; then
    printf '%s\n' "$UNIDADES" | tr ',' '\n' | tr -d ' ' | tr '[:upper:]' '[:lower:]' | sed '/^$/d'
    return
  fi

  case "$(uname -s)" in
    MINGW* | MSYS* | CYGWIN*)
      # Git Bash expone cada unidad como /c, /d, ...
      for letra in {a..z}; do
        if [ -d "/$letra" ]; then
          printf '%s\n' "$letra"
        fi
      done
      ;;
    *)
      # En Linux/macOS no hay letras de unidad; se usan RUTAS_EXTRA.
      ;;
  esac
}

unidades="$(detectar_unidades | sort -u)"
rutas_extra="${RUTAS_EXTRA:-}"

if [ -z "$unidades" ] && [ -z "$rutas_extra" ]; then
  echo "Aviso: no se detectó ninguna unidad del host ni RUTAS_EXTRA." >&2
  echo "       El contenedor no verá carpetas del host; solo los montajes ./music." >&2
fi

solo_lectura=""
if [ "${UNIDADES_SOLO_LECTURA:-0}" = "1" ]; then
  solo_lectura="        read_only: true
"
fi

montajes=""
for unidad in $unidades; do
  case "$unidad" in
    [a-z]) ;;
    *)
      echo "Aviso: '$unidad' no es una letra de unidad válida; se omite." >&2
      continue
      ;;
  esac
  montajes+="      - type: bind
        source: \"$(echo "$unidad" | tr '[:lower:]' '[:upper:]'):/\"
        target: /host/$unidad
$solo_lectura"
done
for ruta in $rutas_extra; do
  nombre="$(basename "$ruta" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9_-' '-')"
  montajes+="      - type: bind
        source: \"$ruta\"
        target: /host/$nombre
$solo_lectura"
done

if [ -z "$montajes" ]; then
  montajes="      []
"
else
  montajes="    volumes:
$montajes"
fi

{
  echo "# Generado por scripts/montar-unidades.sh: no editar a mano."
  if [ -n "$solo_lectura" ]; then
    echo "# Unidades del host montadas en solo lectura para la API."
  else
    echo "# Unidades del host montadas con escritura para la API (subidas)."
  fi
  echo "services:"
  echo "  api:"
  printf '%s' "$montajes"
} > "$SALIDA"

echo "Montajes generados en $(basename "$SALIDA"): /host/{$(echo $unidades | tr ' ' ','),}${rutas_extra:+ rutas extra}"

cd "$RAIZ_PROYECTO"
docker compose -f "$BASE" -f "$SALIDA" up -d "$@"
