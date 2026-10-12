namespace Core;

using Core.Domain;

public static class SampleData
{
    public static List<Product> Products() =>
    [
        Product.Create("1", "SKU-001", "Кабель UTP cat5e", "м", 100),
        Product.Create("2", "SKU-002", "Патч-корд 1м", "шт", 50),
        Product.Create("3", "SKU-003", "Розетка RJ-45", "шт", 30),
        Product.Create("4", "SKU-004", "Комутатор 8-port", "шт", 10),
        Product.Create("5", "SKU-005", "Маршрутизатор Wi-Fi", "шт", 15),
        // ... додати 15-20 записів за вашим доменом
    ];
}