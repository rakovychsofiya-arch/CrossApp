using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core;
using Core.Domain;
using Core.Dto;
using Core.Reports;
using Warehouse.Core.Reports;
namespace Warehouse.Cli;

internal class Program
{
    private static void Main(string[] args)
    {
        var items = SampleData.Products();
        var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

        switch (command)
        {
            case "report-top":
                Console.WriteLine("=== Топ-5 товарів за залишком ===");
                Print(ReportService.TopByQuantity(items, 5));
                break;

            case "report-low":
                Console.WriteLine("=== Дефіцитні товари (< 10) ===");
                Print(ReportService.LowStock(items, 10));
                break;

            case "report-groups":
                Console.WriteLine("=== Групування за одиницями (Method Syntax) ===");
                PrintGroups(ReportService.ByUnit(items));
                break;

            case "report-sum":
                var t = ReportService.Totals(items);
                Console.WriteLine($"позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
                break;

            case "demo-deferred":
                Console.WriteLine("=== Демонстрація відкладеного виконання ===");
                RunDeferredExecutionDemo(items);
                break;

            // Додаткове завдання 1: GroupJoin
            case "demo-join":
                Console.WriteLine("=== Зведення товарів та їх рухів (GroupJoin) ===");
                var sampleMovements = new List<MovementDto>
                {
                    new("SKU-001", 50, "IN"),
                    new("SKU-001", 20, "OUT"),
                    new("SKU-002", 30, "IN"),
                    new("SKU-004", 5, "OUT")
                };
                var movements = ReportService.ProductMovements(items, sampleMovements);
                foreach (var m in movements)
                {
                    Console.WriteLine($"  {m.Sku,-10} {m.Name,-22} Залишок: {m.CurrentStock,4} | Прихід: {m.TotalIn,4} | Расхід: {m.TotalOut,4}");
                }
                break;

            // Додаткове завдання 2: Власне розширення TopBy
            case "demo-extension":
                Console.WriteLine("=== Використання власного методу-розширення TopBy ===");
                Console.WriteLine("-- Топ-3 товари --");
                Print(ReportService.TopProductsWithExtension(items, 3));
                Console.WriteLine("\n-- Топ-2 групи --");
                PrintGroups(ReportService.TopGroupsWithExtension(items, 2));
                break;

            // Додаткове завдання 3: Query Syntax (from ... group ... into ...)
            case "demo-query":
                Console.WriteLine("=== Групування через Query Syntax (from ... group ... into) ===");
                PrintGroups(ReportService.ByUnitQuerySyntax(items));
                break;

            // Додаткове завдання 4: Вимірювання продуктивності Stopwatch (100k)
            case "demo-benchmark":
                Console.WriteLine("=== Порівняння продуктивності (100 000 елементів) ===");
                PerformanceTest.RunBenchmark();
                break;

            default:
                Console.WriteLine("Команди: report-top | report-low | report-groups | report-sum | demo-deferred | demo-join | demo-extension | demo-query | demo-benchmark");
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

    private static void PrintGroups(IReadOnlyList<GroupSummary> groups)
    {
        foreach (var g in groups)
        {
            Console.WriteLine($"  {g.Key,-10} позицій: {g.Count,3}   сумарно: {g.TotalQuantity,6}");
        }
    }

    private static void RunDeferredExecutionDemo(List<Product> sourceList)
    {
        var source = new List<Product>(sourceList);
        var query = source.Where(p => p.Quantity < 10);

        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        source.Add(Product.Create("231", "SKU-999", "Тестова скоба", "шт", 3));

        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

        var snapshot = query.ToList();
        source.Clear();

        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
    }
}

public static class PerformanceTest
{
    public static void RunBenchmark()
    {
        var rand = new Random(42);
        var largeSource = Enumerable.Range(1, 100_000)
            .Select(i => Product.Create(i.ToString(), $"SKU-{i:D6}", $"Товар {i}", i % 2 == 0 ? "шт" : "м", rand.Next(1, 500)))
            .ToList();

        var sw = Stopwatch.StartNew();
        var result1 = largeSource
            .Where(p => p.Quantity < 100)
            .OrderBy(p => p.Quantity)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
        sw.Stop();
        long timeWithoutIntermediateToList = sw.ElapsedMilliseconds;

        sw.Restart();
        var result2 = largeSource
            .Where(p => p.Quantity < 100)
            .ToList() // Проміжна матеріалізація
            .OrderBy(p => p.Quantity)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
        sw.Stop();
        long timeWithIntermediateToList = sw.ElapsedMilliseconds;

        Console.WriteLine($"[100k records] Без проміжного ToList(): {timeWithoutIntermediateToList} ms");
        Console.WriteLine($"[100k records] З проміжним ToList(): {timeWithIntermediateToList} ms");
    }
}