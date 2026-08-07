using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Krosoft.Extensions.Testing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RemoveService(this IServiceCollection services, Func<ServiceDescriptor, bool> filter)
    {
        var descriptor = services.SingleOrDefault(filter);
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        return services;
    }

    public static IServiceCollection RemoveServices<T>(this IServiceCollection services) => services.RemoveServices(d => d.ServiceType == typeof(T));

    public static IServiceCollection RemoveServices(this IServiceCollection services,
                                                    Func<ServiceDescriptor, bool> filter)
    {
        var servicesDescriptor = services.Where(filter).ToList();

        foreach (var serviceDescriptor in servicesDescriptor)
        {
            services.Remove(serviceDescriptor);
        }

        return services;
    }

    public static IServiceCollection RemoveTransient<TService>(this IServiceCollection services)
    {
        var serviceDescriptors = services.Where(x => x.ServiceType == typeof(TService) && x.Lifetime == ServiceLifetime.Transient).ToList();
        foreach (var serviceDescriptor in serviceDescriptors)
        {
            services.Remove(serviceDescriptor);
        }

        return services;
    }

    /// <summary>
    /// Removes all registered registrations of <see cref="TService" />, whatever their <see cref="ServiceLifetime" />, and adds a new registration which uses the <see cref="Func{IServiceProvider, TService}" />.
    /// </summary>
    /// <typeparam name="TService">The type of service interface which needs to be placed.</typeparam>
    /// <param name="services"></param>
    /// <param name="implementationFactory">The implementation factory for the specified type.</param>
    public static IServiceCollection SwapTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
    {
        services.RemoveServices<TService>();
        services.AddTransient(typeof(TService), sp => implementationFactory(sp) ?? throw new InvalidOperationException());
        return services;
    }

    /// <summary>
    /// Removes all registered registrations of <see cref="TService" />, whatever their <see cref="ServiceLifetime" />, and adds a new <see cref="ServiceLifetime.Singleton" /> registration which uses <see cref="TImplementation" />.
    /// </summary>
    /// <typeparam name="TService">The type of service interface which needs to be placed.</typeparam>
    /// <typeparam name="TImplementation">The implementation to use for the specified type.</typeparam>
    /// <param name="services"></param>
    public static IServiceCollection SwapService<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class
    {
        services.RemoveServices<TService>();
        services.AddSingleton<TService, TImplementation>();
        return services;
    }

    public static IServiceCollection MockLogger<TService>(this IServiceCollection services)
        where TService : class
    {
        return services.SwapTransient(_ => new Mock<ILogger<TService>>().Object);
    }
}