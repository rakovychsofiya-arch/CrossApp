using Core;
using Core.Services;
using Core.Storage;

// 1. Створення джерела токенів скасування
using var cts = new CancellationTokenSource();

// Для тестування таймауту можна розкоментувати рядок нижче:
//cts.CancelAfter(TimeSpan.FromMilliseconds(50));

// Підписка на Ctrl+C у консолі
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // запобігає негайному аварійному завершенню процесу
    cts.Cancel();
};

// Парсинг аргументів командного рядка
string dataPath = "data/sample.json";
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--file" && i + 1 < args.Length)
    {
        dataPath = args[i + 1];
        break;
    }
    else if (!args[i].StartsWith("--"))
    {
        dataPath = args[i];
    }
}

try
{
    var store = new InMemoryCatalogStore();
    var service = new CatalogService(store, Array.Empty<INotificationSink>());

    // Підписник на progress малює прогрес обробки в консолі
    var progress = new Progress<int>(count =>
    {
        Console.Write($"\rОбробка записів: {count} шт...");
    });

    var importer = new JsonCatalogImporter();

    // Для використання потокового імпорту з прогресом (Додаткове завдання 3):
    var added = await service.ImportStreamAsync(importer, dataPath, progress, cts.Token);
    
    // Якщо ви використовуєте базовий ImportAsync (Основне завдання):
    //var added = await service.ImportAsync(importer, dataPath, cts.Token);

    Console.WriteLine($"\nІмпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    // Обов'язково першим catch!
    Console.WriteLine("\nІмпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"\nДані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"\nФайл не знайдено: {dataPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"\nПомилка: {ex.Message}");
}