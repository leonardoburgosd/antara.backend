using AntaraSoft.Api;
using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Application.Services;
using AntaraSoft.Infrastructure.Media;
using AntaraSoft.Infrastructure.Metadata;
using AntaraSoft.Infrastructure.Persistence;
using AntaraSoft.Infrastructure.Security;
using AntaraSoft.Infrastructure.Services;
using AntaraSoft.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Bloquea todos los endpoints mientras no exista un administrador registrado
    options.Filters.Add<AdminBootstrapFilter>();
});

builder.Services.AddSingleton<AdminBootstrapState>();
builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddTransient<DatabaseInitializer>();
builder.Services.AddTransient<IDapper, DapperWrapper>();
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IPistaRepository, PistaRepository>();
builder.Services.AddTransient<IAlbumRepository, AlbumRepository>();
builder.Services.AddTransient<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();
builder.Services.AddTransient<IRutaAudioRepository, RutaAudioRepository>();
builder.Services.AddTransient<IBibliotecaRepository, BibliotecaRepository>();
builder.Services.AddTransient<IRegistrarUsuarioService, RegistrarUsuarioService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IGestionarPistaService, GestionarPistaService>();
builder.Services.AddTransient<IMetadatosPistaService, MetadatosPistaService>();
builder.Services.AddTransient<IGestionarAlbumService, GestionarAlbumService>();
builder.Services.AddTransient<IGestionarPlaylistService, GestionarPlaylistService>();
builder.Services.AddTransient<IEncryptText, EncryptText>();
builder.Services.AddSingleton<RutaAudioCache>();
builder.Services.AddSingleton<HostPathResolver>();
builder.Services.AddTransient<IRutaAudioService, RutaAudioService>();

builder.Services.AddHttpClient("metadatos", cliente =>
{
    cliente.DefaultRequestHeaders.UserAgent.ParseAdd("Antara/1.0 (+https://antara.local)");
    cliente.Timeout = TimeSpan.FromSeconds(60);
});

// Proveedores de metadatos y letras: se construyen desde la configuración
// (AppSettings:MetadatosProveedores / LetrasProveedor). Agregar, deshabilitar o
// reordenar proveedores no requiere tocar código ni recompilar adaptadores.
var appSettingsInicial = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

// METADATOS_PROVEEDORES_JSON permite redefinir la lista completa desde una sola
// variable de entorno (por ejemplo en .env), sin editar docker-compose.yml ni
// recompilar. Tiene prioridad sobre AppSettings:MetadatosProveedores.
List<ProveedorMetadatosConfig> proveedoresConfigurados = null;
var proveedoresJson = builder.Configuration["METADATOS_PROVEEDORES_JSON"];
if (!string.IsNullOrWhiteSpace(proveedoresJson))
{
    try
    {
        proveedoresConfigurados = JsonSerializer.Deserialize<List<ProveedorMetadatosConfig>>(
            proveedoresJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch (JsonException err)
    {
        Console.Error.WriteLine(
            $"METADATOS_PROVEEDORES_JSON invalido ({err.Message}). Se usa AppSettings:MetadatosProveedores.");
    }
}

proveedoresConfigurados ??= appSettingsInicial.MetadatosProveedores ?? new List<ProveedorMetadatosConfig>();

var proveedoresMetadatos = (proveedoresConfigurados.Count > 0
        ? proveedoresConfigurados
        : AppSettings.Predeterminados())
    .Where(proveedor => proveedor != null && proveedor.Habilitado)
    .OrderBy(proveedor => proveedor.Prioridad)
    .ToList();

foreach (var proveedor in proveedoresMetadatos)
{
    var configurado = proveedor;

    switch ((configurado.Tipo ?? string.Empty).Trim().ToLowerInvariant())
    {
        case "deezer":
            builder.Services.AddSingleton<IMetadataProvider>(sp => new DeezerMetadataProvider(
                sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, configurado));
            break;

        case "theaudiodb":
            builder.Services.AddSingleton<IMetadataProvider>(sp => new TheAudioDbMetadataProvider(
                sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, configurado));
            break;

        case "musicbrainz":
            builder.Services.AddSingleton<IMetadataProvider>(sp => new MusicBrainzMetadataProvider(
                sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, configurado));
            break;

        case "itunes":
            builder.Services.AddSingleton<IMetadataProvider>(sp => new ItunesMetadataProvider(
                sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, configurado));
            break;

        case "lrclib":
            builder.Services.AddSingleton<IMetadataProvider>(sp => new LrcLibMetadataProvider(
                sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, configurado));
            break;

        default:
            Console.Error.WriteLine(
                $"Proveedor de metadatos '{configurado.Nombre}' omitido: tipo '{configurado.Tipo}' no soportado.");
            break;
    }
}

if (appSettingsInicial.LetrasProveedor == null || appSettingsInicial.LetrasProveedor.Habilitado)
{
    var letrasConfig = appSettingsInicial.LetrasProveedor ?? new ProveedorLetrasConfig();
    builder.Services.AddSingleton<ILetrasProvider>(sp => new LyricsOvhProvider(
        sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial, letrasConfig));
}
// LRCLIB no requiere clave y se usa para la letra sincronizada del reproductor.
builder.Services.AddSingleton<ILetrasSincronizadasProvider>(sp => new LrcLibProvider(
    sp.GetRequiredService<IHttpClientFactory>(), appSettingsInicial));
builder.Services.AddSingleton<EnriquecedorMetadatos>();
builder.Services.AddSingleton<IEscaneoAudioService, EscaneoAudioService>();
builder.Services.AddTransient<ISubidaPistaService, SubidaPistaService>();
builder.Services.AddSingleton<IDescargadorYouTube, YouTubeAudioDownloader>();

// Las canciones pueden pesar cientos de MB: el límite por defecto de Kestrel
// (30 MB) rompería la subida. Se ajusta con AppSettings__SubidaAudioMaxMb.
var subidaMaxBytes = Math.Max(1, builder.Configuration.GetValue<int?>
    ("AppSettings:SubidaAudioMaxMb") ?? 2048) * 1024L * 1024L;
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opciones =>
{
    opciones.MultipartBodyLengthLimit = subidaMaxBytes;
    opciones.ValueLengthLimit = int.MaxValue;
});
builder.WebHost.ConfigureKestrel(opciones => opciones.Limits.MaxRequestBodySize = subidaMaxBytes);

builder.Services.AddTransient<ITokenService>(sp =>
{
    var options = sp.GetService<IOptions<AppSettings>>();
    return new TokenService(options);
});

builder.Services.AddTransient<IAudioStorage>(sp =>
{
    var options = sp.GetService<IOptions<AppSettings>>();
    var cache = sp.GetService<RutaAudioCache>();
    var resolver = sp.GetService<HostPathResolver>();
    return new AudioStorageService(options?.Value ?? new AppSettings(), cache, resolver);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();

    var rutaAudioRepository = scope.ServiceProvider.GetRequiredService<IRutaAudioRepository>();
    var rutaAudioCache = scope.ServiceProvider.GetRequiredService<RutaAudioCache>();
    var rutas = await rutaAudioRepository.ObtenerTodas();
    rutaAudioCache.Actualizar(rutas.Where(ruta => ruta.EstaActiva));
}

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
