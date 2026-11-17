using Core.Domain;

namespace Core.Services;

public static class ReportService
{
    // Метод для розрахунку товарів із низьким запасом (менше або дорівнює threshold)
    public static IReadOnlyList<Product> LowStock(IEnumerable<Product> items, int threshold)
    {
        return items
            .Where(p => p.Quantity <= threshold)
            .ToList();
    }

    // Детермінована вибірка топ-N товарів за кількістю (із сортуванням по SKU при однакових значеннях)
    public static IReadOnlyList<Product> TopByQuantity(IEnumerable<Product> items, int count)
    {
        return items
            .OrderByDescending(p => p.Quantity)
            .ThenBy(p => p.Sku)
            .Take(count)
            .ToList();
    }
}