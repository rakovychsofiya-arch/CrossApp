using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Product> Items() => new()
    {
        new Product("1", "SKU-BIG", "Деталь A", "шт", 100),
        new Product("2", "SKU-MID", "Деталь B", "шт", 50),
        new Product("3", "SKU-LOW", "Деталь C", "шт", 2)
    };
}