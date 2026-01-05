using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.HighPerformance;
using Masayoshi.Archive.Authentication.Twitch;
using Masayoshi.Archive.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Masayoshi.Archive.Authentication;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddApplicationAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<AuthenticationOptions>()
            .BindConfiguration(AuthenticationOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddTwitchAuth(builder.Configuration);

        builder.Services.AddDbContextPool<AuthenticationDbContext>((services, options) =>
        {
            var authOptions = services.GetRequiredOptions<AuthenticationOptions>().Value;
            options.UseSqlite($"Filename={authOptions.FullTokenDatabasePath}");
            options.UseOpenIddict();
        }, 256);

        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<AuthenticationDbContext>();

        builder.Services.AddOpenIddict()
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.UseSystemNetHttp();

                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    var (signingCert, encryptionCert) = GetAuthCerts(builder.Configuration);
                    options.AddSigningCertificate(signingCert.AsStream(), null)
                        .AddEncryptionCertificate(encryptionCert.AsStream(), null);
                }

                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();
            })
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthenticationDbContext>();
            });

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = TwitchAuthConstants.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // Validated elsewhere, assume it's valid
                var authOptions = builder.Configuration
                    .GetRequiredSection(AuthenticationOptions.SectionKey)
                    .Get<AuthenticationOptions>();
                if (authOptions is null)
                {
                    throw new InvalidOperationException("AuthenticationOptions couldn't be bound from config");
                }

                options.ExpireTimeSpan = authOptions.CookieExpirationTime;
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/";
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static async Task InitializeAuthenticationAsync(this WebApplication app, CancellationToken cancellation)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        if (app.Environment.IsDevelopment())
        {
            await context.Database.EnsureDeletedAsync(cancellation);
            await context.Database.EnsureCreatedAsync(cancellation);
            return;
        }

        await context.Database.MigrateAsync(cancellation);
    }

    private static (Memory<byte> Signing, Memory<byte> Encryption) GetAuthCerts(IConfiguration configuration)
    {
        // TODO(jupjohn): I'd like to pull in the validation on these objects, even if we can't get an IOptions<> before DI build
        var signingCertificateEncoded = configuration.GetValue<string>("Authentication:EncodedSigningCertificate");
        if (string.IsNullOrWhiteSpace(signingCertificateEncoded))
        {
            throw new InvalidOperationException("Authentication:EncodedSigningCertificate is unset");
        }

        var encryptionCertificateEncoded = configuration.GetValue<string>("Authentication:EncodedEncryptionCertificate");
        if (string.IsNullOrWhiteSpace(encryptionCertificateEncoded))
        {
            throw new InvalidOperationException("Authentication:EncodedEncryptionCertificate is unset");
        }

        if (!(TryDecodeCertificate(signingCertificateEncoded, out var signingCert) && IsValid(signingCert)))
        {
            throw new InvalidOperationException("Signing certificate is invalid");
        }

        if (!(TryDecodeCertificate(encryptionCertificateEncoded, out var encryptionCert) && IsValid(encryptionCert)))
        {
            throw new InvalidOperationException("Encryption certificate is invalid");
        }

        return
        (
            Signing: signingCert,
            Encryption: encryptionCert
        );

        static bool TryDecodeCertificate(string base64EncodedCertificate, [NotNullWhen(true)] out byte[]? certificate)
        {
            certificate = null;

            Span<byte> decodedCertData = stackalloc byte[4 * 1024];
            if (!Convert.TryFromBase64String(base64EncodedCertificate, decodedCertData, out var bytesWritten))
            {
                return false;
            }

            certificate = decodedCertData[..bytesWritten].ToArray();
            return true;
        }

        static bool IsValid(byte[] certificate)
        {
            X509Certificate2 cert;
            try
            {
                cert = X509CertificateLoader.LoadPkcs12(certificate, null);
            }
            catch
            {
                return false;
            }

            return cert.NotAfter >= DateTime.UtcNow.AddDays(15);
        }
    }
}
