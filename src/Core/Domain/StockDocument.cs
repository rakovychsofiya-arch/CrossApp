namespace Core.Domain;

public enum StockDocumentStatus
{
    Draft,      // Чернетка (можна редагувати)
    Confirmed,  // Підтверджено (проведено по складу)
    Cancelled   // Скасовано
}

public sealed class StockDocument
{
    public string Id { get; }
    public StockDocumentStatus Status { get; private set; }

    private StockDocument(string id, StockDocumentStatus status)
    {
        Id = id;
        Status = status;
    }

    public static StockDocument Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор документа обов'язковий.", nameof(id));

        return new StockDocument(id.Trim(), StockDocumentStatus.Draft);
    }

    /// <summary>
    /// Перехід між станами за допомогою switch expression.
    /// </summary>
    public void ChangeStatus(StockDocumentStatus newStatus)
    {
        // Перевірка допустимості переходу стану
        bool isValidTransition = (Status, newStatus) switch
        {
            // З чернетки можна перейти в підтверджений або скасований
            (StockDocumentStatus.Draft, StockDocumentStatus.Confirmed) => true,
            (StockDocumentStatus.Draft, StockDocumentStatus.Cancelled) => true,

            // Підтверджений документ можна тільки скасувати (наприклад, сторнувати)
            (StockDocumentStatus.Confirmed, StockDocumentStatus.Cancelled) => true,

            // Спроба залишити той самий стан чи неліквідний перехід
            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"Неможливо змінити стан складського документа {Id} з '{Status}' на '{newStatus}'."
            );
        }

        Status = newStatus;
    }
}