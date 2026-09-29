namespace Core.Dto;

public record WarehouseDto(
    string Id,
    string Code,
    string Name,
    string City,
    int Capacity): InventoryRecordDto;