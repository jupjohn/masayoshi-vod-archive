using FastEndpoints;
using Masayoshi.Archive;
using Masayoshi.Archive.Authentication;
using Masayoshi.Archive.Generic;
using Masayoshi.Archive.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.AddApplicationAuthentication();
builder.Services.AddFastEndpoints(options =>
{
    options.DisableAutoDiscovery = true;
    options.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
});

builder
    .AddGenericFeatures()
    .AddHostingFeatures();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    var options = app.Services.GetRequiredOptions<HostingOptions>().Value;
    policyBuilder
        .WithOrigins(options.PublicSiteUri.ToString())
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(config =>
{
    config.Binding.ReflectionCache.AddFromMasayoshiArchive();
});

app.UseDefaultFiles();
app.UseStaticFiles();

await app.InitializeAuthenticationAsync(app.Lifetime.ApplicationStopping);

app.Run();

// TODO:
//   - admin page
//   - disk metadata & src loading
//   - VOD listing page (at root, and with "sign in with Twitch" button when not authed)
//   - video serving (byte range + auth etc.)
//   - VOD player page (videojs.org)
//   - nice to haves:
//      - remembering playback
