using Krosoft.Extensions.Samples.Library.Models.Queries;
using MediatR;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class HelloDotNet6QueryHandler : IRequestHandler<HelloDotNet6Query, string>
{
    private readonly ILogger<HelloDotNet6QueryHandler> _logger;

    public HelloDotNet6QueryHandler(ILogger<HelloDotNet6QueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<string> Handle(HelloDotNet6Query request,
                               CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hello DotNet6...");

        return Task.FromResult("Hello DotNet6");
    }
}