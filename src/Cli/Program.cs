using Core.Domain;

Console.WriteLine("=== Сценарій 1: Успішна робота з доменною моделлю ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine($"Початковий стан: {product}");

product.RegisterArrival(50);
Console.WriteLine($"Після приходу (+50): {product}");

product.Issue(30);
Console.WriteLine($"Після видачі (-30):  {product}");
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: Порушення інваріантів ===");
TryDo("Видача більша за залишок", () => product.Issue(1000));
TryDo("Порожній SKU", () => Product.Create("P-002", "   ", "Пісок", "т", 10));
TryDo("Від'ємний залишок при створенні", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

Console.WriteLine();
Console.WriteLine($"Стан об'єкта після спроб порушення (не змінився): {product}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [FAIL] {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}