using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return new ImportResult<ProductDto>(items, ["Корінь JSON має бути масивом [...]"]);
            }

            int index = 0;
            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                index++;
                try
                {
                    var product = element.Deserialize<ProductDto>(Options);
                    if (product is not null)
                    {
                        items.Add(product);
                    }
                }
                catch (JsonException ex)
                {
                    errors.Add($"елемент {index}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Критична помилка файлу: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}