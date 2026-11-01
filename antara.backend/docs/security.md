# Seguridad

## Sistema de tokens

El sistema **no usa JWT estándar**. Implementa su propio esquema de token firmado con HMAC-SHA256.

### Formato del token

```
base64url(payload_json).base64url(firma_hmac_sha256)
```

**Payload:**
```json
{
  "sub": "uuid-del-usuario",
  "email": "admin@antara.app",
  "nombre": "Administrador",
  "tipo": "admin",
  "exp": 1762000000  ← Unix timestamp de expiración
}
```

### Generación

```csharp
// TokenService.GenerarToken(usuario)
// 1. Serializa el payload a JSON
// 2. Lo codifica en Base64Url
// 3. Firma el payload con HMAC-SHA256 usando AppSettings.TokenSecret
// 4. Devuelve: base64url(payload).base64url(firma)
```

### Validación

```csharp
// TokenService.ValidarToken(token)
// 1. Separa en partes por el punto '.'
// 2. Recalcula la firma esperada del payload
// 3. Compara con tiempo constante (evita timing attacks)
// 4. Decodifica el payload y verifica expiración
// 5. Devuelve TokenValidationResult
```

> [!WARNING]
> El secreto por defecto (`antara-secreto-de-desarrollo`) es público. **Siempre** configura `AppSettings__TokenSecret` en producción con un valor largo y aleatorio (mínimo 32 caracteres).

### Configuración

```json
// appsettings.json
{
  "AppSettings": {
    "TokenSecret": "cambiar-en-produccion-minimo-32-caracteres",
    "TokenExpiracionHoras": 8
  }
}
```

---

## Hashing de contraseñas

Se usa **BCrypt** con factor de trabajo `11` (defecto de BCrypt.Net-Next).

```csharp
// EncryptText.cs
public string GeneratePasswordHash(string texto) =>
    BCrypt.Net.BCrypt.HashPassword(texto);   // Hash aleatorio con salt

public bool CompararHash(string textoPlano, string hash) =>
    BCrypt.Net.BCrypt.Verify(textoPlano, hash);
```

> [!IMPORTANT]
> Las contraseñas nunca se almacenan en texto plano. El campo `Password` en la tabla `Usuario` siempre contiene el hash BCrypt.

---

## Filtros de autorización

### `[TokenAuthorize("admin", "administrador")]`

- Extrae el token del header `Authorization: Bearer {token}`
- Valida la firma y expiración
- Verifica que `resultado.Tipo` esté en la lista de roles permitidos
- Inyecta `UsuarioId`, `UsuarioTipo` y `UsuarioEmail` en `HttpContext.Items`

```csharp
// Acceder al usuario autenticado dentro de un controlador:
var usuarioId = HttpContext.Items["UsuarioId"] is Guid valor ? valor : Guid.Empty;
var tipo = HttpContext.Items["UsuarioTipo"] as string;
```

### `[RoleAuthorize("admin", "administrador")]`

Variante que valida roles directamente desde el header `X-Usuario-Tipo` (para uso interno o en encadenamiento de filtros). **No valida el token**.

> [!NOTE]
> En prácticamente todos los endpoints usa `[TokenAuthorize]`, no `[RoleAuthorize]`. `RoleAuthorize` es un filtro auxiliar legado.

---

## AdminBootstrap

Cuando el sistema arranca sin ningún administrador registrado, el filtro `AdminBootstrapFilter` **bloquea todos los endpoints** excepto:

- `GET /api/sistema/health`
- `GET /api/sistema/crear-primer-admin`
- `POST /api/sistema/crea-primero-admin`

Esto garantiza que el primer usuario creado sea siempre un administrador.

```csharp
// AdminBootstrapState es un Singleton que guarda el estado:
// - AdminExiste: bool (actualizado al iniciar y después de crear el primer admin)
```

El estado se verifica al arrancar la app (`Program.cs`) consultando la BD, y se actualiza cuando se crea el primer admin.

---

## Reglas de seguridad del proyecto

1. **No uses `[Authorize]` de ASP.NET Core.** El sistema usa sus propios filtros.
2. **No expongas `UsuarioId` en la URL.** Extráelo de `HttpContext.Items` desde el token.
3. **No almacenes el token en la BD.** Son stateless; se invalidan automáticamente al expirar.
4. **No uses el secreto por defecto en producción.** Configura `AppSettings__TokenSecret` vía variable de entorno.
5. **La política CORS es `AllowAll`.** Es intencional para el entorno actual (intranet). En producción con dominio fijo, restringe los orígenes.

---

## ❌ Errores comunes de seguridad

- **Olvidar `[TokenAuthorize]` en un nuevo endpoint.** Queda abierto sin autenticación.
- **Comparar contraseñas con `==` en vez de `BCrypt.Verify`.** El hash es diferente cada vez aunque el texto sea el mismo.
- **Parsear el token manualmente con `Split('.')` y asumir que es válido.** Siempre usa `TokenService.ValidarToken()`.
- **Devolver el hash BCrypt de la contraseña en una respuesta.** Los DTOs de usuario nunca incluyen el campo `Password`.

