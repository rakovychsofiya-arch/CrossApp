using Core;
using Core.Domain;

using Warehouse.Core.Reports;
using Core.Reports;
namespace Warehouse.Cli;

internal class Program
{
    private static void Main(string[] args)
    {
        // Симуляція отримання даних із сервісу/сховища
        var items = SampleData.Products();

        // Ігноруємо прапорці на кшталт --file з попередніх тижнів
        var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

        switch (command)
        {
            case "report-top":
                Print(ReportService.TopByQuantity(items, 5));
                break;

            case "report-low":
                Print(ReportService.LowStock(items, 10));
                break;

            case "report-groups":
                foreach (var g in ReportService.ByUnit(items))
                {
                    Console.WriteLine($"  {g.Key,-10} позицій: {g.Count,3}   сумарно: {g.TotalQuantity,6}");
                }
                break;

            case "report-sum":
                var t = ReportService.Totals(items);
                Console.WriteLine($"позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
                break;

            case "demo-deferred":
                RunDeferredExecutionDemo(items);
                break;

            default:
                Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum | demo-deferred");
                break;
        }
    }

    private static void Print(IReadOnlyList<StockLine> lines)
    {
        foreach (var l in lines)
        {
            Console.WriteLine($"  {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
        }
    }

    private static void RunDeferredExecutionDemo(List<Product> sourceList)
    {
        var source = new List<Product>(sourceList); // Створюємо локальну копию для демонстрації
        var query = source.Where(p => p.Quantity < 10); // Запит оголошено, але ще не виконано

        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        // Додаємо новий елемент, що відповідає умові (Quantity < 10)
        source.Add(Product.Create("231","SKU-999", "Тестова скоба", "шт", 3));

        // Викликається повторно — запит переобчислюється по оновленій колекції
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

        // Фіксуємо результат у пам'яті за допомогою матеріалізації
        var snapshot = query.ToList();
        source.Clear(); // Очищаємо джерело

        // snapshot містить знімок даних на момент виклику ToList()
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
    }
}