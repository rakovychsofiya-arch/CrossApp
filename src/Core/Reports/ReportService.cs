using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Domain;
using Core.Dto;

namespace Core.Reports;
using Warehouse.Core.Reports;
public static class ReportService
{
    // 1) Фільтр + сортування + проєкція (Дефіцитні товари)
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ-N за залишком зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування за одиницею виміру (Unit) з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // Додаткове завдання 1: GroupJoin товарів і рухів
    public static IReadOnlyList<ProductMovementSummary> ProductMovements(
        IEnumerable<Product> products,
        IEnumerable<MovementDto> movements)
    {
        return products.GroupJoin(
            movements,
            p => p.Sku,
            m => m.Sku,
            (product, productMovements) => new ProductMovementSummary(
                product.Sku,
                product.Name,
                product.Quantity,
                productMovements.Where(m => m.Type == "IN").Sum(m => m.Quantity),
                productMovements.Where(m => m.Type == "OUT").Sum(m => m.Quantity)
            )
        ).ToList();
    }

    // Додаткове завдання 2: Звіт 1 — Топ товарів за залишком через власне розширення TopBy
    public static IReadOnlyList<StockLine> TopProductsWithExtension(IEnumerable<Product> source, int count) =>
        source.TopBy(p => p.Quantity, count)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // Додаткове завдання 2: Звіт 2 — Топ груп за сумарною кількістю через власне розширення TopBy
    public static IReadOnlyList<GroupSummary> TopGroupsWithExtension(IEnumerable<Product> source, int count) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .TopBy(g => g.TotalQuantity, count)
              .ToList();

    // Додаткове завдання 3: Групування через Query Syntax (from ... group ... into)
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = from product in source
                    group product by product.Unit into g
                    let total = g.Sum(p => p.Quantity)
                    orderby total descending
                    select new GroupSummary(g.Key, g.Count(), total);

        return query.ToList();
    }

    // Додаткове завдання 4: Вимірювання продуктивності Stopwatch (100k записів)
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