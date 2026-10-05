using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<InventoryRecordDto> Load(string path)
    {
        var items = new List<InventoryRecordDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск заголовка таблиці
            if (number == 1 && (line.StartsWith("type", StringComparison.OrdinalIgnoreCase) || line.StartsWith("id", StringComparison.OrdinalIgnoreCase)))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<InventoryRecordDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Мінімальна кількість колонок
            { Length: < 6 }
                => new ParseFailed($"очікую щонайменше 6 колонок, отримав {parts.Length}"),

            // Валідація обов'язкових рядкових полів
            ["P" or "p", _, "", _, _, _, ..] or ["P" or "p", _, _, "", _, _, ..]
                => new ParseFailed("Товар: SKU або назва порожні"),

            ["W" or "w", _, "", _, _, _, ..] or ["W" or "w", _, _, "", _, _, ..]
                => new ParseFailed("Склад: код або назва порожні"),

            // Валідація числових полів (when з TryParse)
            ["P" or "p", _, _, _, _, var qty, ..] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"Товар: кількість '{qty}' не є невід'ємним числом"),

            ["W" or "w", _, _, _, _, var cap, ..] when !int.TryParse(cap, NumberStyles.Integer, CultureInfo.InvariantCulture, out int c) || c < 0
                => new ParseFailed($"Склад: місткість '{cap}' не є невід'ємним числом"),

            // Успішний розбір товару (ProductDto)
            // Формат: P;id;sku;name;unit;quantity;
            ["P" or "p", var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture))),

            ["P" or "p", var id, var sku, var name, var unit, var qty, var note, ..]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(note) ? null : note)),

            // Успішний розбір складу (WarehouseDto)
            // Формат: W;id;code;name;location;capacity
            ["W" or "w", var id, var code, var name, var loc, var cap, ..]
                => new ParseOk(new WarehouseDto(id, code, name, loc, int.Parse(cap, CultureInfo.InvariantCulture))),

            // Невідомий тип запису
            [var prefix, ..] when prefix != "P" && prefix != "p" && prefix != "W" && prefix != "w"
                => new ParseFailed($"невідомий префікс запису: '{prefix}'"),

            _ => new ParseFailed($"некоректна структура рядка: {parts.Length} колонок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(InventoryRecordDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}