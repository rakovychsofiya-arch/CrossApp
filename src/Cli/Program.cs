using System.Reflection;
using Core;
using Core.Configuration;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

// 1. Збирання конфігурації
var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// Підтримка простої змінної CROSSAPP_DATA_PATH
var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storage = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacyDataPath) ? options.Storage.DataPath : legacyDataPath,
    FileName = options.Storage.FileName
};

var fullDataPath = storage.ResolveFullPath(AppContext.BaseDirectory);
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Файл даних: {fullDataPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");

// 2. Токен скасування
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// 3. Парсинг команди (export / import) та опційного --file
string command = "export";
string? importFilePath = null;

for (int i = 0; i < args.Length; i++)
{
    if (args[i].Equals("export", StringComparison.OrdinalIgnoreCase) ||
        args[i].Equals("import", StringComparison.OrdinalIgnoreCase))
    {
        command = args[i].ToLowerInvariant();
    }
    else if (args[i] == "--file" && i + 1 < args.Length)
    {
        importFilePath = args[i + 1];
        i++;
    }
}

try
{
    var store = new FileCatalogStore(fullDataPath);
    var service = new CatalogService(store, Array.Empty<INotificationSink>());

    if (command == "export")
    {
        // Якщо сховище ще порожнє (наприклад, перший запуск), додаємо тестові товари складу
        if (store.List().Count == 0)
        {
            var defaultUnit = options.Warehouse.DefaultUnit;
            store.Add(new Product("a1b2c3d4", "A-100", "Цемент М400", "мішок", 40));
            store.Add(new Product("b2c3d4e5", "B-205", "Цегла рядова", defaultUnit, 500));
            store.Add(new Product("c3d4e5f6", "C-310", "Арматура 12мм", "м", 120));
        }

        var count = store.List().Count;
        Console.WriteLine($"Експортовано {count} записів.");
    }
    else if (command == "import")
    {
        var sourcePath = string.IsNullOrWhiteSpace(importFilePath)
            ? Path.Combine(AppContext.BaseDirectory, "data", "sample.json")
            : Path.GetFullPath(importFilePath);

        var progress = new Progress<int>(count =>
        {
            Console.Write($"\rОбробка записів: {count} шт...");
        });

        var importer = new JsonCatalogImporter();
        var added = await service.ImportStreamAsync(importer, sourcePath, progress, cts.Token);

        Console.WriteLine($"\nІмпортовано записів: {added}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nОперацію скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"\nДані відхилено: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"\nФайл не знайдено: {ex.FileName}");
}
catch (Exception ex)
{
    Console.WriteLine($"\nПомилка: {ex.Message}");
}