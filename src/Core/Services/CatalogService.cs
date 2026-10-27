namespace Core.Services;

using Core.Abstractions;
using Core.Domain;
using Core.Events;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;
    private readonly IEnumerable<INotificationSink> _sinks;

    public CatalogService(ICatalogStore store, IEnumerable<INotificationSink> sinks)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sinks = sinks ?? throw new ArgumentNullException(nameof(sinks));
    }

    private void NotifySinks(string message)
    {
        foreach (var sink in _sinks)
        {
            sink.Notify(message);
        }
    }

    // Подія зміни каталогу
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        
        // Викликаємо подію ПІСЛЯ успішного додавання[cite: 3]
        OnChanged(product, CatalogChangeKind.Added, quantity); 
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id) 
                    ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        
        // Якщо інваріант у сутності кине виняток, рядок нижче зупинить виконання, 
        // і подія OnChanged НЕ спрацює!
        product.RegisterArrival(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    // Використання Func замість події, коли потрібно отримати результат (фільтрація)[cite: 4, 7]
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();
}