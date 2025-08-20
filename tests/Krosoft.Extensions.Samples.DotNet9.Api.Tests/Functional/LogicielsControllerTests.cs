using System.Net;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Core.Helpers;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Samples.DotNet9.Api.Tests.Core;
using Krosoft.Extensions.Samples.Library.Models.Dto;

namespace Krosoft.Extensions.Samples.DotNet9.Api.Tests.Functional;

[TestClass]
public class LogicielsControllerTests : SampleBaseApiTest<Program>
{
    private static async Task CheckExportFile(HttpResponseMessage response, string fileNameExpected)
    {
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Console.WriteLine(content);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var fileName = response.Content.Headers.ContentDisposition?.FileName;

        Check.That(fileName).IsEqualTo(fileNameExpected);

        var stream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        Check.That(stream).IsNotNull();
        Check.That(stream.CanRead).IsTrue();

        await FileHelper.WriteAsync(fileName!, stream, CancellationToken.None);
        Check.That(File.Exists(fileName)).IsTrue();
    }

    [TestMethod]
    public async Task Csv_Ok()
    {
        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync("/Logiciels/Export/Csv");

        await CheckExportFile(response, "Logiciels.csv");
    }

    [TestMethod]
    public async Task Logiciels_Ok()
    {
        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync("/Logiciels");

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadAsNewtonsoftJsonAsync<PaginationResult<LogicielDto>>(CancellationToken.None);
        Check.That(result).IsNotNull();
        Check.That(result?.Items).HasSize(5);
        Check.That(result?.Items.Select(x => x.Id.ToString()))
             .IsOnlyMadeOf("1f39c60d-4f92-4f17-aa45-99e8f86a3b3a",
                           "3e8f94fd-03ea-47b0-b8c5-20b14c361fce",
                           "6c8a012b-cc8a-4da0-85a5-c9a87bb1a20a",
                           "9b5ccfd1-8c3e-4a0c-b4af-543a6f87042d",
                           "c40e3ff1-4f3a-4a3a-8b0c-c52a45e6e7b6");
    }

    [TestMethod]
    public async Task Logiciels_Query_Ok()
    {
        const string nom = "Excel";

        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync($"/Logiciels?Nom={nom}");

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadAsNewtonsoftJsonAsync<PaginationResult<LogicielDto>>(CancellationToken.None);
        Check.That(result).IsNotNull();
        Check.That(result?.Items).HasSize(5);
        Check.That(result?.Items.Select(x => x.Id.ToString()))
             .IsOnlyMadeOf("1f39c60d-4f92-4f17-aa45-99e8f86a3b3a",
                           "3e8f94fd-03ea-47b0-b8c5-20b14c361fce",
                           "6c8a012b-cc8a-4da0-85a5-c9a87bb1a20a",
                           "9b5ccfd1-8c3e-4a0c-b4af-543a6f87042d",
                           "c40e3ff1-4f3a-4a3a-8b0c-c52a45e6e7b6");
    }

    [TestMethod]
    public async Task Pdf_Ok()
    {
        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync("/Logiciels/Export/Pdf");

        await CheckExportFile(response, "Logiciels.pdf");
    }

    [TestMethod]
    public async Task Zip_Ok()
    {
        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync("/Logiciels/Export/Zip");

        await CheckExportFile(response, "Logiciels.zip");
    }
}