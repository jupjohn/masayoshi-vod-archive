using FastEndpoints;

namespace Masayoshi.Archive.StubEndpoint;

public class StubEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/stub");
    }

    public override Task HandleAsync(CancellationToken _)
    {
        throw new NotImplementedException("Stub endpoint");
    }
}
