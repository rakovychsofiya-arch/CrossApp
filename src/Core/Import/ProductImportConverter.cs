using Core.Domain;
using Core.Dto; // Зверни увагу: перевір точну назву класу ImportResult з 3-го тижня

namespace Core.Import;

public record DomainImportResult<T>(
    IReadOnlyList<T> Entities,
    IReadOnlyList<string> InvariantErrors
);

public static class ProductImportConverter
{
    public static DomainImportResult<Product> ToDomainEntities(IEnumerable<ProductDto> dtos)
    {
        var entities = new List<Product>();
        var errors = new List<string>();

        foreach (var dto in dtos)
        {
            try
            {
                // Спроба відновити сутність із DTO із проходженням усіх інваріантів
                var product = Product.FromDto(dto);
                entities.Add(product);
            }
            catch (Exception ex)
            {
                // Фіксуємо конкретний рядок/об'єкт, що не пройшов перевірку інваріантів
                errors.Add($"Помилка імпорту для SKU '{dto.Sku}': {ex.Message}");
            }
        }

        return new DomainImportResult<Product>(entities.AsReadOnly(), errors.AsReadOnly());
    }
}