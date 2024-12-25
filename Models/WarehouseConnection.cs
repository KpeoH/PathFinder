namespace pathfinder.Models;

public class WarehouseConnection
{
    public int FromWarehouseId { get; set; }    // ID исходного склада
    public int ToWarehouseId { get; set; }      // ID целевого склада
    public int Distance { get; set; }           // Расстояние между складами (или стоимость, если нужно)
    public int CityId { get; set; }             // ID города, через который проходит путь

    // Навигационные свойства
    public Warehouses FromWarehouse { get; set; }
    public Warehouses ToWarehouse { get; set; }
    public Cities City { get; set; }
}