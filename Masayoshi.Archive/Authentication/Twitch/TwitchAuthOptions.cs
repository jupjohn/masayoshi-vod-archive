using System.ComponentModel.DataAnnotations;

namespace Masayoshi.Archive.Authentication.Twitch;

public class TwitchAuthOptions : IValidatableObject
{
    public const string SectionKey = "Authentication:Twitch";

    [Required(AllowEmptyStrings = false)]
    public required Uri HelixBaseUrl { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^[a-z0-9]+$")]
    public required string ClientId { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^[a-z0-9]+$")]
    public required string ClientSecret { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"^(/[^\s?/]+)+$")]
    public required string CallbackPath { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string[] AdministratorUserIds { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"^\d+$")]
    public required string BroadcasterUserId { get; init; }

    public required string[] Scopes { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var userId in AdministratorUserIds)
        {
            if (!userId.All(char.IsAsciiDigit))
            {
                yield return new ValidationResult("User IDs must be numeric.", [nameof(AdministratorUserIds)]);
            }
        }
    }
}
