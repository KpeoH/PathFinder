namespace pathfinder.Models;

public class Warehouses
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string Street { get; set; }
    public Cities City { get; set; }
}