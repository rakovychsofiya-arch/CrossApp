namespace Warehouse.Core.Reports;

/// <summary>
/// DTO для підсумкових даних групи (наприклад, за одиницею виміру чи категорією).
/// </summary>
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);