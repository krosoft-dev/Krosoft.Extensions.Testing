using AutoMapper;
using AutoMapper.QueryableExtensions;
using Krosoft.Extensions.Data.Abstractions.Interfaces;
using Krosoft.Extensions.Samples.Library.Models.Dto;
using Krosoft.Extensions.Samples.Library.Models.Entities;
using Krosoft.Extensions.Samples.Library.Models.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class PaysQueryHandler : IRequestHandler<PaysQuery, IEnumerable<PaysDto>>
{
    private readonly ILogger<PaysQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IReadRepository<Pays> _repository;

    public PaysQueryHandler(ILogger<PaysQueryHandler> logger, IReadRepository<Pays> repository, IMapper mapper)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaysDto>> Handle(PaysQuery request,
                                                   CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des pays...");

        var pays = await _repository.Query().ProjectTo<PaysDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        return pays;
    }
}