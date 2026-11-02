namespace Warehouse.Core.Reports;

/// <summary>
/// DTO для відображення рядка звіту по товарах.
/// </summary>
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);