using FastEndpoints;
using Masayoshi.Archive;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints(options =>
{
    options.DisableAutoDiscovery = true;
    options.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
});

var app = builder.Build();

app.UseFastEndpoints(config =>
{
    config.Binding.ReflectionCache.AddFromMasayoshiArchive();
});

app.Run();

// TODO:
//   - static site files + htmx
//   - efcore sqlite
//   - twitch auth
//   - admin page
//   - disk metadata & src loading
//   - VOD listing page
//   - video serving (byte range + auth etc.)
//   - VOD player page (videojs.org)
//   - nice to haves:
//      - remembering playback
