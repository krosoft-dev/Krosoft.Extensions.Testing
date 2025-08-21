using Krosoft.Extensions.Core.Helpers;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Pdf.Interfaces;
using Krosoft.Extensions.Samples.Library.Factories;
using Krosoft.Extensions.Samples.Library.Models.Queries;
using MediatR;

namespace Krosoft.Extensions.Samples.DotNet6.Api.Handlers.Queries;

public class LogicielsExportPdfQueryHandler : IRequestHandler<LogicielsExportPdfQuery, IFileStream>
{
    private readonly ILogger<LogicielsExportPdfQueryHandler> _logger;

    private readonly IPdfService _pdfService;

    public LogicielsExportPdfQueryHandler(ILogger<LogicielsExportPdfQueryHandler> logger, IPdfService pdfService)
    {
        _logger = logger;
        _pdfService = pdfService;
    }

    public async Task<IFileStream> Handle(LogicielsExportPdfQuery request,
                                          CancellationToken cancellationToken)
    {
        _logger.LogInformation("Export des logiciels en PDF...");

        await Task.Delay(2000, cancellationToken);

        var assembly = typeof(AddresseFactory).Assembly;

        var pdf1 = AssemblyHelper.Read(assembly, "sample1.pdf");
        var pdf2 = AssemblyHelper.Read(assembly, "sample1.pdf");

        var data = _pdfService.Merge(pdf1, pdf2);

        return new PdfFileStream(data, "Logiciels.pdf");
    }
}