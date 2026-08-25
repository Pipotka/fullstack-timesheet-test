using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timesheet.Application;
using Timesheet.Infrastructure;
using Timesheet.Infrastructure.Maintenance;
using Timesheet.Infrastructure.MongoDb.Indexes;

namespace Timesheet.Maintenance;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            return command switch
            {
                "seed" => await RunSeedAsync(cts.Token),
                "create-indexes" or "indexes" => await RunCreateIndexesAsync(cts.Token),
                "change-rate" => await RunChangeRateAsync(args, cts.Token),
                _ => PrintUnknownCommand(command)
            };
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Операция отменена.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunSeedAsync(CancellationToken ct)
    {
        var services = BuildServiceProvider();
        var seedData = services.GetRequiredService<SeedData>();
        await seedData.SeedAsync(ct);
        Console.WriteLine("Данные успешно загружены.");
        return 0;
    }

    private static async Task<int> RunCreateIndexesAsync(CancellationToken ct)
    {
        var services = BuildServiceProvider();
        var indexCreator = services.GetRequiredService<IndexCreator>();
        await indexCreator.CreateIndexesAsync(ct);
        Console.WriteLine("Индексы успешно созданы.");
        return 0;
    }

    private static async Task<int> RunChangeRateAsync(string[] args, CancellationToken ct)
    {
        string? employeeId = null;
        string? fromDateStr = null;
        string? rateStr = null;

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--employee":
                    if (i + 1 < args.Length) employeeId = args[++i];
                    break;
                case "--from":
                    if (i + 1 < args.Length) fromDateStr = args[++i];
                    break;
                case "--rate":
                    if (i + 1 < args.Length) rateStr = args[++i];
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(employeeId))
        {
            Console.Error.WriteLine("Ошибка: не указан параметр --employee <id>.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(fromDateStr))
        {
            Console.Error.WriteLine("Ошибка: не указан параметр --from <yyyy-MM-dd>.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(rateStr))
        {
            Console.Error.WriteLine("Ошибка: не указан параметр --rate <decimal>.");
            return 1;
        }

        if (!DateOnly.TryParseExact(fromDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
        {
            Console.Error.WriteLine($"Ошибка: некорректный формат даты '{fromDateStr}'. Ожидается yyyy-MM-dd.");
            return 1;
        }

        if (!decimal.TryParse(rateStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
        {
            Console.Error.WriteLine($"Ошибка: некорректный формат ставки '{rateStr}'. Ожидается десятичное число.");
            return 1;
        }

        var services = BuildServiceProvider();
        var rateChangeService = services.GetRequiredService<RateChangeService>();
        var revision = await rateChangeService.ChangeRateAndRecalculateAsync(employeeId, fromDate, rate, ct);
        Console.WriteLine($"Ставка успешно изменена. Новая ревизия: {revision}.");
        return 0;
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    internal static IConfiguration BuildConfiguration()
    {
        // Load config from AppContext.BaseDirectory (where appsettings.json is copied to output)
        // This ensures the config is found regardless of current working directory
        var basePath = AppContext.BaseDirectory;

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Maintenance.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    // Exposed for testing
    public static IConfiguration BuildConfigurationForTest() => BuildConfiguration();

    private static int PrintUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Ошибка: неизвестная команда '{command}'.");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Использование: Timesheet.Maintenance <команда> [параметры]");
        Console.WriteLine();
        Console.WriteLine("Команды:");
        Console.WriteLine("  seed                                        Загрузить начальные данные");
        Console.WriteLine("  create-indexes, indexes                     Создать индексы MongoDB");
        Console.WriteLine("  change-rate --employee <id> --from <yyyy-MM-dd> --rate <decimal>");
        Console.WriteLine("                                              Изменить ставку сотрудника и пересчитать стоимости");
    }
}
