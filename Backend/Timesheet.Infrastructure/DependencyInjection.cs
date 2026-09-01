using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Timesheet.Application.Common.Interfaces;
using Timesheet.Infrastructure.Maintenance;
using Timesheet.Infrastructure.MongoDb;
using Timesheet.Infrastructure.MongoDb.Indexes;
using Timesheet.Infrastructure.MongoDb.Mappings;
using Timesheet.Infrastructure.MongoDb.Repositories;

namespace Timesheet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        BsonClassMapConfigurator.Configure();

        var settings = configuration
            .GetSection("MongoDb")
            .Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("MongoDb settings are missing");

        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(settings.ConnectionString));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IMongoClient>()
              .GetDatabase(settings.DatabaseName));

        services.AddScoped<ITimeEntryRepository, MongoTimeEntryRepository>();
        services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
        services.AddScoped<IProjectRepository, MongoProjectRepository>();
        services.AddScoped<IPeriodClosureRepository, MongoPeriodClosureRepository>();
        services.AddScoped<IndexCreator>();

        services.AddScoped<SeedData>();
        services.AddScoped<RateChangeService>();

        return services;
    }
}
