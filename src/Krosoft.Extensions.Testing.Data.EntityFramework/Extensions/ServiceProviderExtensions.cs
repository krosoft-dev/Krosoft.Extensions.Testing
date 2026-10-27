using Krosoft.Extensions.Data.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Testing.Data.EntityFramework.Extensions;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Recrée la base (supprimée puis recréée) et rejoue le seed enregistré via <c>AddSeedService</c>.
    /// Destiné à remettre une base InMemory à zéro entre deux tests partageant le même hôte.
    /// </summary>
    public static void ResetDatabase<TDbContext>(this IServiceProvider services) where TDbContext : DbContext
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Le seed service est scoped : l'instance du scope n'a jamais été initialisée.
        var seedService = scope.ServiceProvider.GetRequiredService<ISeedService<TDbContext>>();
        seedService.InitializeDbForTests(db);
    }
}
