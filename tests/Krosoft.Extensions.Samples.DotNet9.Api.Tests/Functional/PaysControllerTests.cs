using System.Net;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Samples.DotNet9.Api.Tests.Core;
using Krosoft.Extensions.Samples.Library.Models.Dto;

namespace Krosoft.Extensions.Samples.DotNet9.Api.Tests.Functional;

[TestClass]
public class PaysControllerTests : SampleBaseApiTest<Program>
{
    [TestMethod]
    public async Task Get_Ok()
    {
        var httpClient = Factory.CreateClient();
        var response = await httpClient.GetAsync("/Pays");

        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var pays = await response.Content.ReadAsNewtonsoftJsonAsync<IEnumerable<PaysDto>>(CancellationToken.None).ToList();
        Check.That(pays).IsNotNull();
        Check.That(pays).HasSize(5);
        Check.That(pays.Select(x => x.Code)).ContainsExactly("fr", "de", "it", "es", "gb");
    }
}