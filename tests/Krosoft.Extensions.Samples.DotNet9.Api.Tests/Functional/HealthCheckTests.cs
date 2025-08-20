using System.Net;
using HealthChecks.UI.Core;
using Krosoft.Extensions.Core.Extensions;
using Krosoft.Extensions.Samples.DotNet9.Api.Tests.Core;
using Krosoft.Extensions.WebApi.HealthChecks.Models;

namespace Krosoft.Extensions.Samples.DotNet9.Api.Tests.Functional;

[TestClass]
public class HealthCheckTests : SampleBaseApiTest<Program>
{
    [TestMethod]
    public async Task HealthCheck_Ok()
    {
        Factory = GetFactory();
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/Health/Check");
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var healthCheckStatus = await response.Content.ReadAsNewtonsoftJsonAsync<HealthCheckStatusDto>(CancellationToken.None);

        Check.That(healthCheckStatus).IsNotNull();
        Check.That(healthCheckStatus!.Status).IsEqualTo("Healthy");
        Check.That(healthCheckStatus.Duration).IsNotEmpty();
        Check.That(healthCheckStatus.Checks).IsNotNull();

        var checks = healthCheckStatus.Checks.OrderBy(c => c.Key).ToList();
        Check.That(checks).HasSize(4);
        Check.That(checks.Select(c => c.Key)).ContainsExactly("Redis", "SampleKrosoftContext", "self", "Test_Endpoint");
        Check.That(checks.Select(c => c.Status)).ContainsExactly("Healthy", "Healthy", "Healthy", "Healthy");
        Check.That(checks.Select(c => c.Description)).ContainsExactly("Ping Redis en 42s", null, null, null);
    }

    [TestMethod]
    public async Task HealthLiveness_Ok()
    {
        Factory = GetFactory();
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/Health/Liveness");
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var model = await response.Content.ReadAsNewtonsoftJsonAsync<UIHealthReport>(CancellationToken.None);

        Check.That(model).IsNotNull();
        Check.That(model!.Status).IsEqualTo(UIHealthStatus.Healthy);
        Check.That(model.TotalDuration).IsLessThan(TimeSpan.FromSeconds(1));

        Check.That(model.Entries).IsNotNull();
        var entries = model.Entries.OrderBy(c => c.Key).ToList();
        Check.That(entries).HasSize(1);
        Check.That(entries.Select(c => c.Key)).ContainsExactly("self");
        Check.That(entries.Select(c => c.Value.Status)).ContainsExactly(UIHealthStatus.Healthy);
    }

    [TestMethod]
    public async Task HealthReadiness_Ok()
    {
        Factory = GetFactory();
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/Health/Readiness");
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var model = await response.Content.ReadAsNewtonsoftJsonAsync<UIHealthReport>(CancellationToken.None);

        Check.That(model).IsNotNull();
        Check.That(model!.Status).IsEqualTo(UIHealthStatus.Healthy);
        Check.That(model.TotalDuration).IsLessThan(TimeSpan.FromSeconds(1));

        Check.That(model.Entries).IsNotNull();
        var entries = model.Entries.OrderBy(c => c.Key).ToList();
        Check.That(entries).HasSize(4);
        Check.That(entries.Select(c => c.Key)).ContainsExactly("Redis", "SampleKrosoftContext", "self", "Test_Endpoint");
        Check.That(entries.Select(c => c.Value.Status)).ContainsExactly(UIHealthStatus.Healthy, UIHealthStatus.Healthy, UIHealthStatus.Healthy, UIHealthStatus.Healthy);
    }
}