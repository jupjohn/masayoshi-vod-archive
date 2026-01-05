namespace Masayoshi.Archive.Generic;

public static class GenericServiceRegistration
{
    public static WebApplicationBuilder AddGenericFeatures(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatableOptions<HttpClientDefaultOptions>(HttpClientDefaultOptions.SectionKey);
        return builder;
    }
}
