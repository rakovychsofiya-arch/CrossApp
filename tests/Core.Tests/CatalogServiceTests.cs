using Core.Services;
using Core.Storage;
using Core.Events;

namespace Core.Tests;

public class CatalogServiceTests
{
    // 1) Інваріант: відмова при негативній кількості та збереження попереднього стану
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store, Array.Empty<INotificationSink>());
        var p = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    // 2) Перевірка підняття події точно один раз
    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<INotificationSink>());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    // 3) Перевірка звіту з низьким запасом для кількох наборів вхідних даних
    [Theory]
    [InlineData(10, 1)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var count = ReportService.LowStock(TestData.Items(), threshold).Count;
        Assert.Equal(expected, count);
    }

    // 4) Детермінованість топ-N позицій за кількістю
    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var result = ReportService.TopByQuantity(TestData.Items(), 2).Select(i => i.Sku);
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, result);
    }

    // 5) Перевірка асинхронного скасування
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<INotificationSink>());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    // 6) Перевірка створення DomainException при зіпсованих даних у тимчасовому файлі
[Fact]
public async Task ImportAsync_WithInvalidData_ThrowsDomainException()
{
    var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    // Передаємо "id" як рядок "1", але з невалідними даними для Product.FromDto
    await File.WriteAllTextAsync(tempPath, "[{\"id\": \"1\", \"sku\": \"\", \"name\": \"Test\", \"unit\": \"шт\", \"quantity\": -10}]");

    try
    {
        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<INotificationSink>());
        await Assert.ThrowsAsync<DomainException>(
            () => service.ImportAsync(new JsonCatalogImporter(), tempPath));
    }
    finally
    {
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }
}
[Fact]
public async Task ImportAsync_WhenTimeoutFires_CancelsByTimeoutToken()
{
    // Arrange: створюємо токен користувача (який не скасовується) і токен таймауту (на 50 мс)
    using var userCts = new CancellationTokenSource();
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

    // Створюємо зв'язаний токен через CreateLinkedTokenSource
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

    var tempPath = Path.GetTempFileName();
    // Створюємо великий тимчасовий JSON для імітації тривалого читання
    var itemsJson = System.Text.Json.JsonSerializer.Serialize(
        Enumerable.Range(1, 1000).Select(i => new { id = i.ToString(), sku = $"SKU-{i}", name = $"Товар {i}", unit = "шт", quantity = 10 }));
    await File.WriteAllTextAsync(tempPath, itemsJson);

    try
    {
        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<INotificationSink>());

        // Чекаємо спрацювання 50 мс таймауту перед викликом або під час нього
        await Task.Delay(100);

        // Act & Assert: перевіряємо, що спрацювало саме скасування
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), tempPath, linkedCts.Token));

        // Доводимо, що скасування відбулося саме від timeoutCts, а не userCts
        Assert.True(timeoutCts.IsCancellationRequested, "Таймаут-токен повинен бути скасований.");
        Assert.False(userCts.IsCancellationRequested, "Токен користувача НЕ повинен бути скасований.");
    }
    finally
    {
        if (File.Exists(tempPath)) File.Delete(tempPath);
    }
}
}