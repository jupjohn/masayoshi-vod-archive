using FastEndpoints;
using Masayoshi.Archive;
using Masayoshi.Archive.Generic;
using Masayoshi.Archive.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddFastEndpoints(options =>
{
    options.DisableAutoDiscovery = true;
    options.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
});

builder.AddHostingFeatures();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    var options = app.Services.GetRequiredOptions<HostingOptions>().Value;
    policyBuilder
        .WithOrigins(options.PublicSiteUri.ToString())
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseFastEndpoints(config =>
{
    config.Binding.ReflectionCache.AddFromMasayoshiArchive();
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

// TODO:
//   - efcore sqlite
//   - twitch auth
//   - admin page
//   - disk metadata & src loading
//   - VOD listing page
//   - video serving (byte range + auth etc.)
//   - VOD player page (videojs.org)
//   - nice to haves:
//      - remembering playback
