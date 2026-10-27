namespace Core.Abstractions;

using Core.Domain;

public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    Product? GetById(string id);
    IEnumerable<Product> Find(Func<Product, bool> predicate);
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
    
}