using SimpleProject.Data;
using SimpleProject.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleProject.Services;
public class StartUp
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
        {
            var settings = configuration.Get<AppSettings>();
            if (settings != null)
            {
                AppSettings.Current = settings;
            }
        }

        services.AddDbContext<DbContext, AppDbContext>(opt =>
        {
            opt.UseSqlServer(AppSettings.Current.ConnectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped((provider) =>
        {
            var opt = new DbContextOptionsBuilder<LogDbContext>();
            opt.UseSqlServer(AppSettings.Current.LogConnectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new LogDbContext(opt.Options);
        });

        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        var allService = GetAllServices();
        foreach (var interfaceType in allService)
        {
            foreach (var serviceType in interfaceType.Value.Scopeds)
            {
                if (serviceType == interfaceType.Key)
                {
                    services.AddScoped(serviceType);
                }
                else
                {
                    services.AddScoped(interfaceType.Key, serviceType);
                }
            }

            foreach (var serviceType in interfaceType.Value.Singletons)
            {
                if (serviceType == interfaceType.Key)
                {
                    services.AddSingleton(serviceType);
                }
                else
                {
                    services.AddSingleton(interfaceType.Key, serviceType);
                }
            }
        }
    }

    private static Dictionary<Type, (List<Type> Scopeds, List<Type> Singletons)> GetAllServices()
    {
        var list = new Dictionary<Type, (List<Type> Scopeds, List<Type> Singletons)>();
        var singletonService = typeof(ISingletonService);
        var scopedService = typeof(IScopedService);

        var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(a => !a.IsInterface && !a.IsAbstract && (scopedService.IsAssignableFrom(a) || singletonService.IsAssignableFrom(a)))).ToList();
        foreach (var type in allTypes)
        {
            var scopedInterface = type.GetInterfaces().FirstOrDefault(a => a != scopedService && scopedService!.IsAssignableFrom(a));
            var singletonInterface = type.GetInterfaces().FirstOrDefault(a => a != singletonService && singletonService!.IsAssignableFrom(a));
            if (scopedInterface != null)
            {
                if (list.TryGetValue(scopedInterface, out var value))
                {
                    value.Scopeds.Add(type);
                }
                else
                {
                    list[scopedInterface] = ([type], []);
                }
            }
            else if (singletonInterface != null)
            {
                if (list.TryGetValue(singletonInterface, out var value))
                {
                    value.Singletons.Add(type);
                }
                else
                {
                    list[singletonInterface] = ([], [type]);
                }
            }
            else
            {
                if (scopedService.IsAssignableFrom(type))
                {
                    list[type] = ([type], []);
                }
                else
                {
                    list[type] = ([], [type]);
                }
            }
        }

        return list;
    }
}
