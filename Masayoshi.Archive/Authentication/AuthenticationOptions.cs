using System.ComponentModel.DataAnnotations;

namespace Masayoshi.Archive.Authentication;

public class AuthenticationOptions : IValidatableObject
{
    public const string SectionKey = "Authentication";

    /// <summary>
    /// How long should cookies last after their last use
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required TimeSpan CookieExpirationTime { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string TokenDatabasePath { get; init; }

    public PathString FullTokenDatabasePath => Path.IsPathRooted(TokenDatabasePath)
        ? TokenDatabasePath
        : Path.GetFullPath(TokenDatabasePath, Environment.CurrentDirectory);

    private static readonly TimeSpan MaximumCookieAge = TimeSpan.FromDays(7);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CookieExpirationTime <= TimeSpan.Zero || CookieExpirationTime >= MaximumCookieAge)
        {
            yield return new ValidationResult(
                $"Expiry time must be a positive value, less than {(int)MaximumCookieAge.TotalDays} day(s).",
                [nameof(MaximumCookieAge)]
            );
        }
    }
}
