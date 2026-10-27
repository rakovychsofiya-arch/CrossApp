using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Events;

// Додайте простір імен вашого сховища, наприклад:
// using Core.Infrastructure; 

// 1. Налаштування лог-файлу (створення каталогу за потреби)
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// 2. Ініціалізація сховища та сервісу (ВИРІШУЄ ПОМИЛКУ CS0103)
// (Замініть InMemoryCatalogStore на ваш реальний клас сховища з тижня 5)
 ICatalogStore store = new InMemoryCatalogStore(); 
 // Створюємо приймальники для додаткового завдання (або використовуємо їх як альтернативу подіям)
var sinks = new INotificationSink[] 
{
    new ConsoleSink(),
    new FileSink(logPath)
};

var service = new CatalogService(store, sinks);

// --- Крок 3. Підписка локальним методом ---
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// --- Крок 4. Другий підписник (лямбда із замиканням) ---
int changes = 0;
service.Changed += (_, _) => changes++;

// --- Крок 5. Основний сценарій ---
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);

Console.WriteLine($"Подій отримано: {changes}"); // Очікується 2

// Відписка першого обробника
service.Changed -= OnCatalogChanged;

// Тиха операція (працює лише лічильник лямбди)
service.Receive(created.Id, 5); 
Console.WriteLine($"Подій отримано: {changes}"); // Очікується 3

// --- Крок 6. Сценарій відмови (інваріант) у try/catch ---
try
{
    // Спроба отримати неіснуючий ID, що викличе виняток
    service.Receive("non-existent", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"[ПОМИЛКА ПЕРЕХОПЛЕНА]: {ex.Message}");
    // У консолі є текст помилки, але рядок події НЕ додається, бо подія не піднімалася[cite: 4]
}

// --- Крок 7. Використання делегата Func (значення у відповідь) ---
var lowStockProducts = service.FindAll(p => p.Quantity < 10);
Console.WriteLine($"Знайдено товарів з критичним залишком: {lowStockProducts.Count}");
// Приклад вхідних даних для імпорту (останній рядок навмисно з помилкою)
var csvLines = new[]
{
    "SKU-201;Миша бездротова;шт;15",
    "SKU-202;Клавіатура;шт;помилка_кількості", 
    "SKU-203;Монітор;шт;10"
};

var importer = new CatalogImporter();

// Викликаємо метод, передаючи лямбду для обробки помилок (Action<string>)
int successCount = importer.ImportFromCsv(csvLines, errorMessage => 
{
    // Цей код виконається щоразу, коли всередині імпорту станеться виняток
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[ІМПОРТ ПОПЕРЕДЖЕННЯ]: {errorMessage}");
    Console.ResetColor();
    
    // За бажанням можна також дублювати у файл логів
    File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} [IMPORT_WARN] {errorMessage}{Environment.NewLine}");
});

Console.WriteLine($"Успішно імпортовано товарів: {successCount}");