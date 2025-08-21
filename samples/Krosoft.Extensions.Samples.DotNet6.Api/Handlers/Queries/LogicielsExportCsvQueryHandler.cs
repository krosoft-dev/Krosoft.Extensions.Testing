using System.Globalization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Reporting.Csv.Extensions;
using Krosoft.Extensions.Reporting.Csv.Models;
using Krosoft.Extensions.Samples.Library.Factories;
using Krosoft.Extensions.Samples.Library.Models.Dto;
using Krosoft.Extensions.Samples.Library.Models.Queries;
using MediatR;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class LogicielsExportCsvQueryHandler : IRequestHandler<LogicielsExportCsvQuery, IFileStream>
{
    private readonly ILogger<LogicielsExportCsvQueryHandler> _logger;
    private readonly IMapper _mapper;

    public LogicielsExportCsvQueryHandler(ILogger<LogicielsExportCsvQueryHandler> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IFileStream> Handle(LogicielsExportCsvQuery request,
                                          CancellationToken cancellationToken)
    {
        _logger.LogInformation("Export des logiciels en CSV...");

        await Task.Delay(2000, cancellationToken);

        var logiciels = LogicielFactory.GetRandom(10, null)
                                       .AsQueryable()
                                       .ProjectTo<LogicielCsvDto>(_mapper.ConfigurationProvider)
                                       .ToList();

        var csvFileStream = new CsvFileData<LogicielCsvDto>(logiciels, "Logiciels.csv", CultureInfo.InvariantCulture)
            .ToCsvStreamResult();

        return csvFileStream;
    }
}