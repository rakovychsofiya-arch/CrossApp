namespace Core.Services;

public class CatalogImporter
{
    // Метод приймає дані та делегат Action<string> для сповіщення про пропущені рядки
    public int ImportFromCsv(IEnumerable<string> lines, Action<string> onError)
    {
        int importedCount = 0;

        foreach (var line in lines)
        {
            try
            {
                // Приклад розбору рядка (формат: SKU;Name;Unit;Quantity)
                var parts = line.Split(';');
                if (parts.Length < 4)
                {
                    throw new FormatException("Невірний формат рядка (очікується 4 поля).");
                }

                string sku = parts[0];
                string name = parts[1];
                string unit = parts[2];
                
                if (!int.TryParse(parts[3], out int quantity))
                {
                    throw new FormatException($"Некоректне значення кількості: '{parts[3]}'.");
                }

                // Логіка додавання через сервіс (або безпосередньо у сховище)
                // ...
                importedCount++;
            }
            catch (Exception ex)
            {
                // Викликаємо Action, щоб передати текст помилки у зовнішній код (Cli)
                onError?.Invoke($"Пропущено рядок '{line}': {ex.Message}");
            }
        }

        return importedCount;
    }
}