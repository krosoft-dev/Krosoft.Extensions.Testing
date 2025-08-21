using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Samples.Library.Models.Dto;
using Krosoft.Extensions.Samples.Library.Models.Queries;
using Krosoft.Extensions.WebApi.Interfaces;
using MediatR;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class CorsPolicyQueryHandler : IRequestHandler<CorsPolicyQuery, CorsPolicyDto>
{
    private readonly ICorsPolicyAccessor _corsPolicyAccessor;
    private readonly ILogger<CorsPolicyQueryHandler> _logger;

    public CorsPolicyQueryHandler(ILogger<CorsPolicyQueryHandler> logger, ICorsPolicyAccessor corsPolicyAccessor)
    {
        _logger = logger;
        _corsPolicyAccessor = corsPolicyAccessor;
    }

    public Task<CorsPolicyDto> Handle(CorsPolicyQuery request,
                                      CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des CORS Policy...");

        var corsPolicyDto = new CorsPolicyDto
        {
            Origins = []
        };
        var corsPolicy = _corsPolicyAccessor.GetPolicy();
        if (corsPolicy != null)
        {
            corsPolicyDto.Origins.AddRange(corsPolicy.Origins);
        }

        var policy = _corsPolicyAccessor.GetPolicy("Public");
        if (policy != null)
        {
            corsPolicyDto.Origins.AddRange(policy.Origins);
        }

        return Task.FromResult(corsPolicyDto);
    }
}