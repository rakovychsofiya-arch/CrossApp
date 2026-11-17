namespace Core.Storage;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

public sealed class FileCatalogStore : ICatalogStore
{
    private readonly string _path;
    private readonly Dictionary<string, Product> _cache = new();
    private bool _loaded;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public FileCatalogStore(string fullFilePath)
    {
        _path = fullFilePath;
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        try
        {
            var json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];
            _cache.Clear();
            foreach (var dto in dtos)
            {
                var product = Product.FromDto(dto);
                _cache[product.Id] = product;
            }
            _loaded = true;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Помилка читання JSON-файлу даних ({_path}): некоректний формат.", ex);
        }
    }

    private void Flush()
    {
        var dtos = _cache.Values.Select(p => p.ToDto()).ToList();
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }
    public IEnumerable<Product> Find(Func<Product, bool> predicate)
   {
    ArgumentNullException.ThrowIfNull(predicate);
    EnsureLoaded();
    return _cache.Values.Where(predicate);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;

        Flush();
        return true;
    }

}