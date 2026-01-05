using Masayoshi.Archive.Generic;

namespace Masayoshi.Archive.Hosting;

public static class HostingBuilderExtensions
{
    public static WebApplicationBuilder AddHostingFeatures(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatableOptions<HostingOptions>(HostingOptions.SectionKey);
        return builder;
    }
}
