using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants.Tokens;

namespace Masayoshi.Archive.Authentication.Twitch;

public class TwitchAuthCallbackEndpoint(
    IOptions<TwitchAuthOptions> twitchAuthOptions
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get(twitchAuthOptions.Value.CallbackPath);
        AllowAnonymous(Http.GET);
    }

    public override async Task HandleAsync(CancellationToken cancellation)
    {
        // TODO(jupjohn): handle cancel flow
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        if (result is not { Principal.Identity.IsAuthenticated: true, Properties: not null })
        {
            await Send.ForbiddenAsync(cancellation);
            return;
        }

        var (backChannelAccessToken, backChannelIdToken, refreshToken) =
        (
            result.Properties.GetTokens().FirstOrDefault(t => t.Name == BackchannelAccessToken),
            result.Properties.GetTokens().FirstOrDefault(t => t.Name == BackchannelIdentityToken),
            result.Properties.GetTokens().FirstOrDefault(t => t.Name == RefreshToken)
        );

        if (backChannelAccessToken is null || backChannelIdToken is null || refreshToken is null)
        {
            // TODO(jupjohn): figure out why we'd get here...
            await Send.ForbiddenAsync(cancellation);
            return;
        }

        // TODO(jupjohn): Result<T, TError>
        var (userId, login, displayName) = await new AuthingTwitchUserRequest(backChannelAccessToken.Value)
            .ExecuteAsync(cancellation);

        TwitchUserRole[] userRoles = [];

        var globalAdminIds = twitchAuthOptions.Value.AdministratorUserIds;
        var broadcasterUserId = twitchAuthOptions.Value.BroadcasterUserId;

        // TODO(jupjohn): also check user is following/subscribed
        if (Enumerable.Contains([..globalAdminIds, broadcasterUserId], userId))
        {
            userRoles = [TwitchUserRole.Administrator];
        }

        var redirectUri = result.Properties?.RedirectUri ?? "/";
        await CookieAuth.SignInAsync(
            ConfigurePrivileges,
            ConfigureProperties
        );
        await Send.RedirectAsync(redirectUri);

        return;

        void ConfigurePrivileges(UserPrivileges privileges)
        {
            privileges.Roles.AddRange(userRoles.Select(r => r.Value));

            privileges.Claims.AddRange(
                new Claim(TwitchAuthConstants.Claims.Id, userId),
                new Claim(TwitchAuthConstants.Claims.Login, login),
                new Claim(TwitchAuthConstants.Claims.DisplayName, displayName)
            );

            const string registrationClaim = OpenIddictConstants.Claims.Private.RegistrationId;
            var registrationId = result.Principal?.GetClaim(registrationClaim);
            if (registrationId is not null)
            {
                privileges.Claims.Add(new Claim(registrationClaim, registrationId));
            }
        }

        void ConfigureProperties(AuthenticationProperties authenticationProperties)
        {
            authenticationProperties.RedirectUri = redirectUri;
            foreach (var item in result.Properties?.Items!)
            {
                authenticationProperties.Parameters.Add(item.Key, item.Value);
            }

            authenticationProperties.StoreTokens([backChannelAccessToken, backChannelIdToken, refreshToken]);
        }
    }
}

public class TwitchUserHandler(
    [FromKeyedServices(TwitchAuthConstants.BackChannelHttpClientKey)]
    HttpClient backchannel
) : ICommandHandler<AuthingTwitchUserRequest, AuthingTwitchUserResponse>
{
    public async Task<AuthingTwitchUserResponse> ExecuteAsync(AuthingTwitchUserRequest command, CancellationToken cancellation)
    {
        backchannel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", command.AccessToken);
        var userResponse = await backchannel.GetAsync(
            string.Empty,
            HttpCompletionOption.ResponseHeadersRead,
            cancellation
        );

        if (!userResponse.IsSuccessStatusCode)
        {
            // TODO(jupjohn): implement me!
            throw new NotImplementedException("TODO(jupjohn): implement me!");
        }

        await using var responseStream = await userResponse.Content.ReadAsStreamAsync(cancellation);
        var deserializedResponse = await JsonSerializer.DeserializeAsync(
            responseStream,
            TwitchAuthJsonContext.Default.HelixUserResponse,
            cancellation
        );

        if (deserializedResponse is not { Users.Length: 1 })
        {
            // TODO(jupjohn): implement me!
            throw new NotImplementedException("TODO(jupjohn): implement me!");
        }

        var (id, login, displayName) = deserializedResponse.Users[0];
        return new AuthingTwitchUserResponse(id, login, displayName);
    }
}

public record AuthingTwitchUserRequest(
    string AccessToken
) : ICommand<AuthingTwitchUserResponse>;

public record AuthingTwitchUserResponse(
    string UserId,
    string Login,
    string DisplayName
);

public record HelixUserResponse(
    [property: JsonPropertyName("data")] HelixUserResponse.User[] Users
)
{
    public record User(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("display_name")] string DisplayName
    );
}

[JsonSerializable(typeof(HelixUserResponse))]
public partial class TwitchAuthJsonContext : JsonSerializerContext;
