using Krosoft.Extensions.Core.Helpers;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Samples.Library.Factories;
using Krosoft.Extensions.Samples.Library.Models.Queries;
using Krosoft.Extensions.Zip.Interfaces;
using MediatR;
using Newtonsoft.Json;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class LogicielsExportZipQueryHandler : IRequestHandler<LogicielsExportZipQuery, IFileStream>
{
    private readonly ILogger<LogicielsExportZipQueryHandler> _logger;
    private readonly IZipService _zipService;

    public LogicielsExportZipQueryHandler(ILogger<LogicielsExportZipQueryHandler> logger, IZipService zipService)
    {
        _logger = logger;
        _zipService = zipService;
    }

    public async Task<IFileStream> Handle(LogicielsExportZipQuery request,
                                          CancellationToken cancellationToken)
    {
        _logger.LogInformation("Export des logiciels en ZIP...");

        await Task.Delay(2000, cancellationToken);

        var logiciels = LogicielFactory.GetRandom(10, null)
                                       .AsQueryable()
                                       .ToList();

        var dictionary = new Dictionary<string, Stream>();
        foreach (var logiciel in logiciels)
        {
            var stream = StringHelper.GenerateStreamFromString(JsonConvert.SerializeObject(logiciel));
            dictionary.Add($"{logiciel.Nom}.txt", stream);
        }

        var zipFileStream = await _zipService.ZipAsync(dictionary, "Logiciels.zip", cancellationToken);
        return zipFileStream;
    }
}