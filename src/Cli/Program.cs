using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// 1. Вибір імпортера за назвою або розширенням файлу через switch expression
ImportResult<InventoryRecordDto> result = Path.GetFileName(path).ToLowerInvariant() switch
{
    "mixed.csv" => MixedCsvImporter.Load(path),
    _ => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => AdaptResult(ProductCsvImporter.Load(path)),
        ".json" => AdaptResult(ProductJsonImporter.Load(path)),
        var ext => throw new NotSupportedException($"Розширення '{ext}' не підтримується")
    }
};

// 2. Виведення даних із розпізнаванням конкретного типу запису
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (InventoryRecordDto item in result.Items.Take(5))
{
    string display = item switch
    {
        ProductDto p => $"  [Товар] {p.Id,-6} {p.Sku,-10} {p.Name,-24} {p.Quantity,5} {p.Unit}",
        WarehouseDto w => $"  [Склад] {w.Id,-6} {w.Name,-30}",
        _ => $"  [Невідомо] {item}"
    };
    Console.WriteLine(display);
}

// 3. Повідомлення про помилки
if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// 4. Статистика імпорту одним рядком
int total = result.Items.Count + result.Errors.Count;
double errorPercent = total > 0 ? (double)result.Errors.Count / total * 100 : 0.0;

Console.WriteLine("----------------------------------------------------------------------");
Console.WriteLine($"Статистика: Усього: {total} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилок: {errorPercent:F1}%");

return 0;

// Допоміжний метод приведення типізованого результату ProductDto до загального InventoryRecordDto
static ImportResult<InventoryRecordDto> AdaptResult(ImportResult<ProductDto> res) =>
    new(res.Items.Cast<InventoryRecordDto>().ToList(), res.Errors);