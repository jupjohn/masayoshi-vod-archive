using System.ComponentModel.DataAnnotations;

namespace Masayoshi.Archive.Generic;

public class HttpClientDefaultOptions
{
    public const string SectionKey = "Client:Defaults";

    [Required(AllowEmptyStrings = false)]
    public required string UserAgent { get; init; }
}
