using System.ComponentModel.DataAnnotations;

namespace Masayoshi.Archive.Hosting;

public class HostingOptions
{
    public const string SectionKey = "Hosting";

    [Required(AllowEmptyStrings = false)]
    public required Uri PublicSiteUri { get; init; }
}
