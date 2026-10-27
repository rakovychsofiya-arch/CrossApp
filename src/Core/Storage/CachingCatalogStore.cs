using System.Collections.Concurrent;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _innerStore;
    private readonly ConcurrentDictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isFullyLoaded;

    public CachingCatalogStore(ICatalogStore innerStore)
    {
        _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    }

    public IReadOnlyList<Product> List()
    {
        EnsureCacheLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        if (_cache.TryGetValue(id, out var cachedItem))
        {
            return cachedItem;
        }

        var item = _innerStore.GetById(id);
        if (item != null)
        {
            _cache[item.Id] = item;
        }

        return item;
    }

    public IEnumerable<Product> Find(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        // Гарантуємо, що весь список завантажено з innerStore у кеш перед пошуком
        EnsureCacheLoaded();

        return _cache.Values.Where(predicate);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Спочатку записуємо в основне сховище (щоб перевірити унікальність/інваріанти)[cite: 7, 8]
        _innerStore.Add(item);
        _cache[item.Id] = item;
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _innerStore.Update(item);
        _cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        var removed = _innerStore.Remove(id);
        if (removed)
        {
            _cache.TryRemove(id, out _);
        }

        return removed;
    }

    private void EnsureCacheLoaded()
    {
        if (_isFullyLoaded) return;

        var items = _innerStore.List();
        _cache.Clear();
        foreach (var item in items)
        {
            _cache[item.Id] = item;
        }

        _isFullyLoaded = true;
    }
}