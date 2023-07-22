using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AutoGnome;

public static class ServiceCollectionExtensions
{
    public static void AddAllImplementations<T>(this IServiceCollection services, Assembly[] assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)) || x.IsSubclassOf(typeof(T))));
        foreach (var type in typesFromAssemblies)
        {
            services.TryAddEnumerable(new ServiceDescriptor(typeof(T), type, lifetime));
        }
    }
}