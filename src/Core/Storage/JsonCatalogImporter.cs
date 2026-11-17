using System.Text.Json;
using System.Runtime.CompilerServices; // <-- Додає підтримку [EnumeratorCancellation]
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    // 1. Метод для завантаження всього списку в пам'ять (Крок 3)
    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        List<ProductDto>? dtos;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, options, cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Некоректний формат JSON-файлу {Path.GetFileName(path)}.", ex);
        }

        if (dtos is null)
        {
            throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");
        }

        var result = new List<Product>(dtos.Count);

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                result.Add(Product.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    // 2. Окремий метод для потокового імпорту (Додаткове завдання 1)
    public async IAsyncEnumerable<Product> StreamAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var dtosStream = JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(stream, options, cancellationToken: ct);

        int index = 0;
        await foreach (var dto in dtosStream.WithCancellation(ct))
        {
            index++;
            if (dto is null) continue;

            Product product;
            try
            {
                product = Product.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }

            yield return product;
        }
    }
}