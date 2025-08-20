using System.Net;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Core.Models.Dto;
using Krosoft.Extensions.Samples.DotNet9.Api.Tests.Core;
using Krosoft.Extensions.WebApi.Identity.Middlewares;

namespace Krosoft.Extensions.Samples.DotNet9.Api.Tests.Functional;

[TestClass]
public class HelloControllerTests : SampleBaseApiTest<Program>
{
    [TestMethod]
    public async Task Hello_Ok()
    {
        var url = "/Hello";

        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync(url);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Check.That(result).IsEqualTo("Hello DotNet9");
    }

    [TestMethod]
    public async Task HelloName_Ok()
    {
        var url = "/Hello";
        var obj = new { Name = "World of Tests" };

        var httpClient = Factory.CreateClient();
        var response = await httpClient.PostAsNewtonsoftJsonAsync(url, obj);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Check.That(result).IsEqualTo("Hello World of Tests !");
    }

    [TestMethod]
    public async Task HelloName_Null()
    {
        var url = "/Hello";
        var obj = new { Name = (string?)null };

        var httpClient = Factory.CreateClient();
        var response = await httpClient.PostAsNewtonsoftJsonAsync(url, obj);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>();
        Check.That(error).IsNotNull();
        Check.That(error!.Code).IsEqualTo(400);
        Check.That(error.Errors)
             .ContainsExactly("'Name' ne doit pas être vide.",
                              "'Name' ne doit pas avoir la valeur null.");
    }

    [TestMethod]
    public async Task HelloApiKey_Ok()
    {
        var url = "/Hello/ApiKey";

        var httpClient = Factory.CreateClient();
        httpClient.DefaultRequestHeaders.Add(ApiKeyMiddleware.ApiKeyHeaderName, "AZERTY!123");
        var response = await httpClient.GetAsync(url);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Check.That(result).IsEqualTo("Hello DotNet9");
    }

    [TestMethod]
    public async Task HelloApiKey_Unauthorized_Invalid()
    {
        var url = "/Hello/ApiKey";

        var httpClient = Factory.CreateClient();
        httpClient.DefaultRequestHeaders.Add(ApiKeyMiddleware.ApiKeyHeaderName, "AZERTY");
        var response = await httpClient.GetAsync(url);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        var errorDto = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>(CancellationToken.None);
        Check.That(errorDto).IsNotNull();
        Check.That(errorDto!.Code).IsEqualTo(401);
        Check.That(errorDto.Message).IsEqualTo("Unauthorized");
        Check.That(errorDto.Errors.First()).IsEqualTo("Invalid Api Key provided.");
    }

    [TestMethod]
    public async Task HelloApiKey_Unauthorized_NotProvided()
    {
        var url = "/Hello/ApiKey";

        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync(url);

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        var errorDto = await response.Content.ReadAsNewtonsoftJsonAsync<ErrorDto>(CancellationToken.None);
        Check.That(errorDto).IsNotNull();
        Check.That(errorDto!.Code).IsEqualTo(401);
        Check.That(errorDto.Message).IsEqualTo("Unauthorized");
        Check.That(errorDto.Errors.First()).IsEqualTo("Api Key was not provided.");
    }
}