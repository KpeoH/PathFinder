namespace pathfinder.Models;

public class Products
{
    public int Id { get; set; }
    public int NomenclatureId { get; set; }
    public int WarehouseId { get; set; }
    public Warehouses Warehouse { get; set; }
    
    public List<ProductWarehouseHistory> WarehouseHistory { get; set; }
}