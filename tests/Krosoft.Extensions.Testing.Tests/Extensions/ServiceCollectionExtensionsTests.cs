using Krosoft.Extensions.Samples.Library.Models;
using Krosoft.Extensions.Testing.Extensions;
using Krosoft.Extensions.Testing.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Testing.Tests.Extensions;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void SwapTransient_EnregistrementTransient_EstRemplace()
    {
        var services = new ServiceCollection();
        services.AddTransient<IFoo, FooService>();

        var mock = new Mock<IFoo>();
        services.SwapTransient(_ => mock.Object);

        Check.That(services.Where(d => d.ServiceType == typeof(IFoo))).HasSize(1);

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetRequiredService<IFoo>()).IsSameReferenceAs(mock.Object);
    }

    [TestMethod]
    public void SwapTransient_EnregistrementScoped_EstRemplace()
    {
        var services = new ServiceCollection();
        services.AddScoped<IFoo, FooService>();

        var mock = new Mock<IFoo>();
        services.SwapTransient(_ => mock.Object);

        Check.That(services.Where(d => d.ServiceType == typeof(IFoo))).HasSize(1);

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetRequiredService<IFoo>()).IsSameReferenceAs(mock.Object);
    }

    [TestMethod]
    public void SwapTransient_EnregistrementSingleton_EstRemplace()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFoo, FooService>();

        var mock = new Mock<IFoo>();
        services.SwapTransient(_ => mock.Object);

        Check.That(services.Where(d => d.ServiceType == typeof(IFoo))).HasSize(1);

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetRequiredService<IFoo>()).IsSameReferenceAs(mock.Object);
    }

    [TestMethod]
    public void SwapTransient_PlusieursEnregistrements_TousSupprimes()
    {
        var services = new ServiceCollection();
        services.AddTransient<IFoo, FooService>();
        services.AddScoped<IFoo, FooService>();
        services.AddSingleton<IFoo, FooService>();

        var mock = new Mock<IFoo>();
        services.SwapTransient(_ => mock.Object);

        var descriptors = services.Where(d => d.ServiceType == typeof(IFoo)).ToList();
        Check.That(descriptors).HasSize(1);
        Check.That(descriptors[0].Lifetime).IsEqualTo(ServiceLifetime.Transient);

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetServices<IFoo>()).HasSize(1);
    }

    [TestMethod]
    public void SwapTransient_AucunEnregistrementExistant_AjouteLeService()
    {
        var services = new ServiceCollection();

        var mock = new Mock<IFoo>();
        services.SwapTransient(_ => mock.Object);

        Check.That(services.Where(d => d.ServiceType == typeof(IFoo))).HasSize(1);

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetRequiredService<IFoo>()).IsSameReferenceAs(mock.Object);
    }

    [TestMethod]
    public void SwapService_EnregistrementScoped_EstRemplace()
    {
        var services = new ServiceCollection();
        var mock = new Mock<IFoo>();
        services.AddScoped(_ => mock.Object);

        services.SwapService<IFoo, FooService>();

        var descriptors = services.Where(d => d.ServiceType == typeof(IFoo)).ToList();
        Check.That(descriptors).HasSize(1);
        Check.That(descriptors[0].Lifetime).IsEqualTo(ServiceLifetime.Singleton);
        Check.That(descriptors[0].ImplementationType).IsEqualTo(typeof(FooService));

        using var serviceProvider = services.BuildServiceProvider();
        Check.That(serviceProvider.GetRequiredService<IFoo>()).IsInstanceOf<FooService>();
    }

    [TestMethod]
    public void RemoveTransient_NeSupprimeQueLesTransients()
    {
        var services = new ServiceCollection();
        services.AddTransient<IFoo, FooService>();
        services.AddScoped<IFoo, FooService>();
        services.AddSingleton<IFoo, FooService>();

        services.RemoveTransient<IFoo>();

        var descriptors = services.Where(d => d.ServiceType == typeof(IFoo)).ToList();
        Check.That(descriptors).HasSize(2);
        Check.That(descriptors.Select(d => d.Lifetime)).ContainsExactly(ServiceLifetime.Scoped, ServiceLifetime.Singleton);
    }
}
