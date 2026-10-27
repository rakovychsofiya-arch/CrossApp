using Core.Abstractions;
using Core.Storage;

namespace Core.Factories;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        bool useCache = args.Contains("--cache", StringComparer.OrdinalIgnoreCase);

        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        // 1. Визначаємо базове сховище
        ICatalogStore baseStore = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // 2. Огортаємо декоратором за потреби
        if (useCache)
        {
            return new CachingCatalogStore(baseStore);
        }

        return baseStore;
    }
}