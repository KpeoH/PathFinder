namespace pathfinder.Models;

public class ProductWarehouseHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime MovedAt { get; set; }

    // Навигационные свойства
    public Products Product { get; set; }
    public Warehouses Warehouse { get; set; }
}