using Krosoft.Extensions.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Testing.WebApi;

/// <summary>
/// Factory pour les controleurs de base.
/// </summary>
/// <typeparam name="TStartup">Type du startup du projet à test.</typeparam>
/// <typeparam name="TKrosoftContext">DbContext du projet.</typeparam>
public class CustomWebApplicationFactory<TStartup, TKrosoftContext> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly Action<KrosoftToken>? _actionConfigureClaims;
    private readonly Action<IConfigurationBuilder>? _actionConfigureAppConfiguration;
    private readonly Action<IServiceCollection>? _actionConfigureServices;

    public CustomWebApplicationFactory(Action<IServiceCollection> actionConfigureServices,
                                       Action<IConfigurationBuilder>? actionConfigureAppConfiguration,
                                       Action<KrosoftToken>? actionConfigureClaims,
                                       bool useFakeAuth)
    {
        _actionConfigureServices = actionConfigureServices;
        _actionConfigureAppConfiguration = actionConfigureAppConfiguration;
        _actionConfigureClaims = actionConfigureClaims;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_actionConfigureAppConfiguration != null)
        {
            builder.ConfigureAppConfiguration((_, config) => _actionConfigureAppConfiguration(config));
        }

        builder.ConfigureServices(services =>
        {
            if (_actionConfigureServices != null)
            {
                _actionConfigureServices(services);
            }
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var claimInfo = KrosoftTokenHelper.Defaut;
        if (_actionConfigureClaims != null)
        {
            _actionConfigureClaims(claimInfo);
        }

        return CreateAuthenticatedClient(claimInfo);
    }

    public HttpClient CreateAuthenticatedClient(KrosoftToken positiveToken)
    {
        var client = CreateClient();
        return client;
    }

    public void WithService<T>(Action<T> action) where T : notnull
    {
        var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<T>();
            action(service);
        }
    }

    //public void RemoveRange<T>() where T : class
    //{
    //    WithService<TKrosoftContext>(db =>
    //    {
    //        db.RemoveRange(db.Set<T>());
    //        db.SaveChanges();
    //    });
    //}
}
