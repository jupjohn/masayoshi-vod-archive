using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Microsoft.Extensions.Primitives;
using Void = FastEndpoints.Void;

namespace Masayoshi.Archive.Generic;

public static class EndpointExtensions
{
    extension(HttpContext context)
    {
        public bool IsHtmxRequest() => context.Request.Headers["HX-Request"] != StringValues.Empty;
    }

    extension<T1, T2>(ResponseSender<T1, T2> sender) where T1 : notnull
    {
        /// <summary>
        /// Send an HTMX fragment response
        /// </summary>
        /// <param name="html">The HTML to return</param>
        /// <param name="cancellation">A cancellation token to stop sending</param>
        public Task<Void> HtmxFragAsync(
            [StringSyntax("html")] string html,
            CancellationToken cancellation = default
        ) => sender.StringAsync(html, StatusCodes.Status200OK, "text/html; charset=utf-8", cancellation);

        public async Task<Void> HtmxFragOrDocumentAsync(
            [StringSyntax("html")] string html,
            CancellationToken cancellation = default
        )
        {
            if (sender.HttpContext.IsHtmxRequest())
            {
                return await sender.HtmxFragAsync(html, cancellation);
            }

            var environment = sender.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var indexFile = environment.WebRootFileProvider.GetFileInfo("index.html");
            if (!indexFile.Exists) throw new NotImplementedException("TODO(jupjohn): implement me!");

            using var reader = new StreamReader(indexFile.CreateReadStream(), leaveOpen: false);
            var indexContent = await reader.ReadToEndAsync(cancellation);

            const string startTag = "<body>";
            var startTagIndex = indexContent.IndexOf(startTag, StringComparison.InvariantCulture);
            if (startTagIndex == -1) throw new NotImplementedException("TODO(jupjohn): implement me!");

            var startSection = indexContent[..(startTagIndex + startTag.Length)];

            const string endTag = "</body>";
            var endTagIndex = indexContent.LastIndexOf(endTag, StringComparison.InvariantCulture);
            if (endTagIndex == -1) throw new NotImplementedException("TODO(jupjohn): implement me!");

            var endSection = indexContent[endTagIndex..];
            var wrappedHtml = $"{startSection}{html}{endSection}";
            return await sender.StringAsync(wrappedHtml, StatusCodes.Status200OK, "text/html; charset=utf-8", cancellation);
        }
    }
}
