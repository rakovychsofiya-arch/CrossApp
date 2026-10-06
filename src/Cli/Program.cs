using Core.Domain;
using Core.Dto;
using Core.Import;

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
Console.WriteLine("\n=== Сценарій 3: Додаткове завдання 1 (Мапінг DTO -> Сутності + Помилки) ===");

// 1. Готуємо тестовий список DTO (валідні та з порушенням інваріантів)
List<ProductDto> dtos = new()
{
    new ProductDto("P-010", "SKU-010", "Лопата", "шт", 15),          // Валідний
    new ProductDto("P-011", "", "Пісок", "т", 5),                    // Помилка: порожній SKU
    new ProductDto("P-012", "SKU-012", "Грейфер", "шт", -10),        // Помилка: від'ємний залишок
    new ProductDto("P-013", "SKU-013", "Цвяхи 100мм", "кг", 200)   // Валідний
};

// 2. Викликаємо конвертер
var importResult = ProductImportConverter.ToDomainEntities(dtos);

// 3. Виводим успішно створені сутності
Console.WriteLine($"Успішно створено сутностей: {importResult.Entities.Count}");
foreach (var entity in importResult.Entities)
{
    Console.WriteLine($"  [Успіх] {entity}");
}

// 4. Виводимо помилки інваріантів
Console.WriteLine($"\nПомилок інваріантів при імпорті: {importResult.InvariantErrors.Count}");
foreach (var error in importResult.InvariantErrors)
{
    Console.WriteLine($"  [Помилка] {error}");
}
Console.WriteLine("\n=== Сценарій 4: Додаткове завдання 3 (Стани накладної та переходи) ===");

// 1. Успішний процес: створення чернетки -> підтвердження -> скасування
StockDocument doc = StockDocument.Create("DOC-2026-001");
Console.WriteLine($"Створено документ: {doc.Id}, поточний стан: {doc.Status}");

doc.ChangeStatus(StockDocumentStatus.Confirmed);
Console.WriteLine($"Після проведення: {doc.Id}, поточний стан: {doc.Status}");

doc.ChangeStatus(StockDocumentStatus.Cancelled);
Console.WriteLine($"Після скасування: {doc.Id}, поточний стан: {doc.Status}");

Console.WriteLine();
Console.WriteLine("--- Спроби недопустимих переходів між станами ---");

// 2. Спроба повторно провести вже скасований документ (недопустимий перехід)
TryDo("Спроба підтвердити скасований документ", () => doc.ChangeStatus(StockDocumentStatus.Confirmed));

// 3. Спроба створити документ з порожнім номером
TryDo("Спроба створити документ без номера", () => StockDocument.Create("   "));