using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Timesheet.Infrastructure.MongoDb.Documents;

namespace Timesheet.Infrastructure.MongoDb.Mappings;

public static class BsonClassMapConfigurator
{
    private static bool _isConfigured;
    private static readonly object Lock = new();

    public static void Configure()
    {
        if (_isConfigured)
        {
            return;
        }

        lock (Lock)
        {
            if (_isConfigured)
            {
                return;
            }

            ConfigureConventions();
            ConfigureTimeEntryMap();
            ConfigureEmployeeMap();
            ConfigureProjectMap();
            ConfigurePeriodClosureMap();

            _isConfigured = true;
        }
    }

    private static void ConfigureConventions()
    {
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register("CamelCaseConvention", conventionPack, _ => true);
    }

    private static void ConfigureTimeEntryMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(TimeEntryDocument)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<TimeEntryDocument>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(e => e.Id);
            cm.MapProperty(e => e.Date).SetSerializer(new DateOnlySerializer());
        });
    }

    private static void ConfigureEmployeeMap()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(EmployeeDocument)))
        {
            BsonClassMap.RegisterClassMap<EmployeeDocument>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(e => e.Id);
                cm.MapProperty(e => e.RateHistory);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(RateHistoryEntryDocument)))
        {
            BsonClassMap.RegisterClassMap<RateHistoryEntryDocument>(cm =>
            {
                cm.AutoMap();
                cm.MapProperty(e => e.From).SetSerializer(new DateOnlySerializer());
            });
        }
    }

    private static void ConfigureProjectMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ProjectDocument)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<ProjectDocument>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(p => p.Id);
            cm.MapProperty(p => p.StartDate).SetSerializer(new NullableSerializer<DateOnly>(new DateOnlySerializer()));
            cm.MapProperty(p => p.EndDate).SetSerializer(new NullableSerializer<DateOnly>(new DateOnlySerializer()));
        });
    }

    private static void ConfigurePeriodClosureMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(PeriodClosureDocument)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<PeriodClosureDocument>(cm =>
        {
            cm.AutoMap();
        });
    }
}
