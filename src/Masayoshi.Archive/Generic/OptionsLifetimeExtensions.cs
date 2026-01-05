using Microsoft.Extensions.Options;

namespace Masayoshi.Archive.Generic;

public static class OptionsLifetimeExtensions
{
    public static OptionsBuilder<TOptions> AddValidatableOptions<TOptions>(
        this IServiceCollection services,
        string configurationSectionKey
    ) where TOptions : class
    {
        return services.AddOptions<TOptions>()
            .BindConfiguration(configurationSectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static IOptions<TOptions> GetRequiredOptions<TOptions>(
        this IServiceProvider services
    ) where TOptions : class => services.GetRequiredService<IOptions<TOptions>>();
}
