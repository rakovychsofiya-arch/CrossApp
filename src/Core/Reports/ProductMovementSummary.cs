namespace Core.Reports;

public record ProductMovementSummary(
    string Sku,
    string Name,
    int CurrentStock,
    int TotalIn,
    int TotalOut
);