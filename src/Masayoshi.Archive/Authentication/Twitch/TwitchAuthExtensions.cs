using System.Net.Http.Headers;
using System.Security.Claims;
using Masayoshi.Archive.Generic;

namespace Masayoshi.Archive.Authentication.Twitch;

public static class TwitchAuthExtensions
{
    public static IServiceCollection AddTwitchAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TwitchAuthOptions>()
            .BindConfiguration(TwitchAuthOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOpenIddict()
            .AddClient(options => options
                .UseWebProviders()
                .AddTwitch(twitchOptions =>
                {
                    var authOptions = configuration
                        .GetRequiredSection(TwitchAuthOptions.SectionKey)
                        .Get<TwitchAuthOptions>();
                    if (authOptions is null)
                    {
                        // TODO(jupjohn): implement me!
                        throw new NotImplementedException("TODO(jupjohn): implement me!");
                    }

                    twitchOptions.SetProviderName(TwitchAuthConstants.AuthenticationScheme);

                    twitchOptions.SetRedirectUri(authOptions.CallbackPath);
                    twitchOptions.SetClientId(authOptions.ClientId);
                    twitchOptions.SetClientSecret(authOptions.ClientSecret);
                    twitchOptions.AddScopes(authOptions.Scopes);
                })
            );

        services.AddHttpClient(
                TwitchAuthConstants.BackChannelHttpClientKey,
                (serviceProvider, client) =>
                {
                    var authOptions = serviceProvider.GetRequiredOptions<TwitchAuthOptions>().Value;
                    var userAgent = serviceProvider.GetRequiredOptions<HttpClientDefaultOptions>().Value.UserAgent;

                    client.BaseAddress = new Uri(authOptions.HelixBaseUrl, "users");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Client-Id", authOptions.ClientId);
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                })
            // TODO(jupjohn): polly
            .AddAsKeyed();

        return services;
    }
}

public static class TwitchClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public TwitchUser? Twitch => principal.Identity?.IsAuthenticated is true
            ? TwitchUser.FromPrincipal(principal)
            : null;
    }
}

public readonly record struct TwitchUser(string Id, string Login, string DisplayName, TwitchUserRole[] Role)
{
    internal static TwitchUser? FromPrincipal(ClaimsPrincipal principal)
    {
        var id = principal.FindFirst(TwitchAuthConstants.Claims.Id)?.Value;
        var login = principal.FindFirst(TwitchAuthConstants.Claims.Login)?.Value;
        var displayName = principal.FindFirst(TwitchAuthConstants.Claims.DisplayName)?.Value;

        if (id is null || login is null || displayName is null)
        {
            return null;
        }

        var roles = TwitchUserRole.List()
            .Select(role => (IsInRole: principal.IsInRole(role.Value), Role: role))
            .Where(tuple => tuple.IsInRole)
            .Select(tuple => tuple.Role)
            .ToArray();

        return new TwitchUser(id, login, displayName, roles);
    }
}
