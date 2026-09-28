using Core.Dto;
 using Core.Import;
 string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");
 if (!File.Exists(path))
 {
 Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
 return 1;
 }
 ImportResult<ProductDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    var ext => throw new NotSupportedException($"Формат файлу '{ext}' не підтримується")
};
 Console.WriteLine($"Завантажено записів: {result.Items.Count}");
 foreach (ProductDto p in result.Items.Take(5))
 Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
 if (result.Errors.Count > 0)
 {
 Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
 foreach (string e in result.Errors)
 Console.WriteLine($" ! {e}");
 }
 int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorRate = total > 0 ? (double)skipped / total * 100 : 0;

Console.WriteLine();
Console.WriteLine($"Статистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorRate:F1}%");
 return 0;