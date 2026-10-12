using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Composition Root: ручне впровадження залежностей (Manual DI)
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

try
{
    // 1. Демонстрація стандартного сценарію
    Console.WriteLine("--- Додавання товару ---");
    var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    Console.WriteLine($"Створено: {created.Id} | {created.Name} ({created.Quantity} {created.Unit})");
    var created2 = service.Add("SKU-102", "Кабель новий", "м", 75);
    Console.WriteLine($"Створено: {created2.Id} | {created2.Name} ({created2.Quantity} {created2.Unit})");

    Console.WriteLine("\n--- Прихід товару (+25) ---");
    service.Receive(created.Id, 25);
    service.Receive(created2.Id, 25);

    Console.WriteLine("\n--- Поточний список товарів ---");
    foreach (var p in service.All())
    {
        Console.WriteLine($" {p.Id,-8} {p.Sku,-10} {p.Name,-25} {p.Quantity,5} {p.Unit}");
    }

    // 2. Демонстрація сценарію відмови
    Console.WriteLine("\n--- Тест сценарію відмови (неіснуючий ID) ---");
    service.Receive("non-existing-id", 10);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ПОМИЛКА]: {ex.Message}");
    Console.ResetColor();
}