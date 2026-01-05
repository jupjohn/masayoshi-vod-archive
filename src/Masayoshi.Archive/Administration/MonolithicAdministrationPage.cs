using FastEndpoints;
using Masayoshi.Archive.Authentication.Twitch;
using Masayoshi.Archive.Generic;

namespace Masayoshi.Archive.Administration;

public class MonolithicAdministrationPage : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/admin");
        // FIXME(jupjohn): this was looping the login flow if the user doesn't have the role, so we're doing it in-handler
        // Roles(TwitchUserRole.Administrator);
    }

    public override async Task HandleAsync(CancellationToken cancellation)
    {
        if (HttpContext.User.Twitch is not { } currentUser)
        {
            await Send.UnauthorizedAsync(cancellation);
            return;
        }

        if (!currentUser.Role.Contains(TwitchUserRole.Administrator))
        {
            await Send.ForbiddenAsync(cancellation);
            return;
        }

        await Send.HtmxFragOrDocumentAsync($"<span>Administrating as {currentUser.DisplayName}...</span>", cancellation);
    }
}
