using AntaraSoft.Api;
using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Application.Services;
using AntaraSoft.Infrastructure.Metadata;
using AntaraSoft.Infrastructure.Persistence;
using AntaraSoft.Infrastructure.Security;
using AntaraSoft.Infrastructure.Services;
using AntaraSoft.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

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

builder.Services.AddSingleton<IMetadataProvider, DeezerMetadataProvider>();
builder.Services.AddSingleton<IMetadataProvider, TheAudioDbMetadataProvider>();
builder.Services.AddSingleton<ILetrasProvider, LyricsOvhProvider>();
builder.Services.AddSingleton<EnriquecedorMetadatos>();
builder.Services.AddSingleton<IEscaneoAudioService, EscaneoAudioService>();

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
