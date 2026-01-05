using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Masayoshi.Archive.Authentication;

public class LogoutEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/logout");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await Send.RedirectAsync("/");
    }
}
