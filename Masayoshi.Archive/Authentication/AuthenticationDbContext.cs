using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Masayoshi.Archive.Authentication;

/// <summary>
/// DB context used (and model managed) by OpenIddict's EF integration.
/// </summary>
public class AuthenticationDbContext(
    DbContextOptions<AuthenticationDbContext> options
) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; init; }
}
