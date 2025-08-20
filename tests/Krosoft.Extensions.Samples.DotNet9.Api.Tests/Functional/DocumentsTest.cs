using System.Net;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Core.Models.Dto;
using Krosoft.Extensions.Samples.DotNet9.Api.Tests.Core;
using Krosoft.Extensions.Samples.Library.Models.Dto;

namespace Krosoft.Extensions.Samples.DotNet9.Api.Tests.Functional;

[TestClass]
public class DocumentsTest : SampleBaseApiTest<Program>
{
    [TestMethod]
    public async Task DeposerFichier_Empty_FichierId()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("1000"), "TenantId");
        var response = await Factory.CreateClient().PostAsync("/Documents/Deposer/Fichier", content);
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        var error = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>();
        Check.That(error).IsNotNull();
        Check.That(error!.Code).IsEqualTo(500);
        Check.That(error.Errors).ContainsExactly("Required parameter \"long FichierId\" was not provided from form.");
    }

    [TestMethod]
    public async Task DeposerFichier_Empty_File()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("2000"), "FichierId");
        var response = await Factory.CreateClient().PostAsync("/Documents/Deposer/Fichier", content);
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        var error = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>();
        Check.That(error).IsNotNull();
        Check.That(error!.Code).IsEqualTo(500);
        Check.That(error.Errors)
             .ContainsExactly("Required parameter \"IFormFile File\" was not provided from form file.");
    }

    [TestMethod]
    public async Task DeposerFichier_Ok()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("42"), "FichierId");
        await using var fileStream = File.OpenRead("./Files/Hello.txt");
        content.Add(new StreamContent(fileStream), "File", "Hello.txt");
        var response = await Factory.CreateClient().PostAsync("/Documents/Deposer/Fichier", content);
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var error = await response.Content.ReadAsNewtonsoftJsonAsync<DepotDto>();
        Check.That(error).IsNotNull();
        Check.That(error!.Numero).IsEqualTo(null);
        Check.That(error.Message).StartsWith("Fichier créé sur temp");
    }

    [TestMethod]
    public async Task DeposerFichier_Type()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("ABC"), "FichierId");
        var response = await Factory.CreateClient().PostAsync("/Documents/Deposer/Fichier", content);
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        var error = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>();
        Check.That(error).IsNotNull();
        Check.That(error!.Code).IsEqualTo(500);
        Check.That(error.Errors).ContainsExactly("Failed to bind parameter \"long FichierId\" from \"ABC\".");
    }
}