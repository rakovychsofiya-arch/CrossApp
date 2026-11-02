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
        Product.Create("6",  "SKU-006", "Коннектор RJ-45", "пачка", 85),
        Product.Create("7",  "SKU-007", "Кабель FTP cat6", "м", 240),
        Product.Create("8",  "SKU-008", "Патч-панель 24p", "шт", 6),       // дефіцит (<10)
        Product.Create("9",  "SKU-009", "Шафа серверна 12U", "шт", 3),    // дефіцит (<10)
        Product.Create("10", "SKU-010", "Оптичний патч-корд", "шт", 45),
        Product.Create("11", "SKU-011", "SFP модуль 1G", "шт", 22),
        Product.Create("12", "SKU-012", "Патч-корд 3м", "шт", 60),
        Product.Create("13", "SKU-013", "Стяжки кабельні 200мм", "пачка", 120),
        Product.Create("14", "SKU-014", "Кабельний канал 20х10", "м", 180),
        Product.Create("15", "SKU-015", "Блок живлення 12V 5A", "шт", 8),  // дефіцит (<10)
        Product.Create("16", "SKU-016", "Джерело ДБЖ 850VA", "шт", 4),     // дефіцит (<10)
        Product.Create("17", "SKU-017", "Набір інструментів КСП", "комплект", 12),
        Product.Create("18", "SKU-018", "Тестер мережевий RJ45", "шт", 7)
    ];
}