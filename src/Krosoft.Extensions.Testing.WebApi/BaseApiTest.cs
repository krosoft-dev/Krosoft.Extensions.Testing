using System.Net;
using Krosoft.Extensions.Core.Models;
using Krosoft.Extensions.Testing.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Krosoft.Extensions.Testing.WebApi;

public abstract class BaseApiTest<TStartup, TKrosoftContext> : BaseTest
    where TStartup : class
{
    private static readonly object SharedFactoryLock = new();
    private static CustomWebApplicationFactory<TStartup, TKrosoftContext>? _sharedFactory;
    private static Type? _sharedFactoryOwner;

    protected CustomWebApplicationFactory<TStartup, TKrosoftContext> Factory = null!;
    protected virtual bool UseFakeAuth => true;

    /// <summary>
    /// Partage la factory (et donc l'hôte web) entre les tests d'une même classe au lieu d'en démarrer une par test.
    /// <see cref="ResetState" /> est appelé avant chaque test qui réutilise l'hôte.
    /// ⚠️ <see cref="ConfigureServices" /> n'est joué qu'une fois par classe, sur la première instance : il ne doit pas
    /// capturer d'état d'instance (mocks en champs d'instance notamment). Suppose des classes exécutées l'une après l'autre
    /// (comportement par défaut de MSTest).
    /// </summary>
    protected virtual bool ShareFactory => false;

    /// <summary>
    /// Remet l'application dans son état initial (base de données, caches...) avant un test qui réutilise l'hôte partagé.
    /// N'est pas appelé pour le premier test d'une classe : l'hôte vient d'être créé.
    /// </summary>
    protected virtual void ResetState(IServiceProvider services)
    {
    }

    protected virtual void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    protected virtual void ConfigureClaims(KrosoftToken krosoftToken)
    {
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected CustomWebApplicationFactory<TStartup, TKrosoftContext> GetFactory()
    {
        void Action(IServiceCollection services)
        {
            ConfigureServices(services);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                  .ReturnsAsync(new HttpResponseMessage
                                  {
                                      StatusCode = HttpStatusCode.OK
                                  });

            var client = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
            services.SwapTransient(_ => mockHttpClientFactory.Object);
        }

        return new CustomWebApplicationFactory<TStartup, TKrosoftContext>(Action, ConfigureAppConfiguration, null, UseFakeAuth);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (!ShareFactory)
        {
            Factory.Dispose();
        }
    }

    [TestInitialize]
    public void TestInitialize()
    {
        if (!ShareFactory)
        {
            Factory = CreateFactory();
            return;
        }

        lock (SharedFactoryLock)
        {
            if (_sharedFactory != null && _sharedFactoryOwner == GetType())
            {
                ResetState(_sharedFactory.Services);
            }
            else
            {
                // Nouvelle classe de tests : l'hôte de la classe précédente n'est plus utile.
                _sharedFactory?.Dispose();
                _sharedFactory = CreateFactory();
                _sharedFactoryOwner = GetType();
            }

            Factory = _sharedFactory;
        }
    }

    /// <summary>
    /// Libère l'hôte partagé encore actif. À appeler depuis l'<c>[AssemblyCleanup]</c> du projet de tests
    /// (MSTest ne le découvre que sur une classe <c>[TestClass]</c> du projet, pas dans ce package).
    /// </summary>
    public static void DisposeSharedFactory()
    {
        lock (SharedFactoryLock)
        {
            _sharedFactory?.Dispose();
            _sharedFactory = null;
            _sharedFactoryOwner = null;
        }
    }

    private CustomWebApplicationFactory<TStartup, TKrosoftContext> CreateFactory() =>
        new(ConfigureServices, ConfigureAppConfiguration, ConfigureClaims, UseFakeAuth);
}
