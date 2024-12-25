using Microsoft.AspNetCore.Mvc;
using pathfinder.Data;
using pathfinder.Models;


namespace pathfinder.Controllers;
[ApiController]
[Route("[controller]")]
public class PathFinderController(ApplicationDbContext context) : ControllerBase
{

    private readonly ApplicationDbContext _context = context;

    
    /// <summary>
    /// 501 город 
    /// </summary>
    /// <returns></returns>
    [HttpGet("Cities")]
    public IActionResult GetCities()
    {
        var cities = _context.Cities.Take(501).ToList();
        
        return Ok(cities);
    }
    
    /// <summary>
    /// 1000 номенклатур
    /// </summary>
    /// <returns></returns>
    [HttpGet("Nomenclatures")]
    public IActionResult GetNomenclatures()
    {
        var nomenclatures = _context.Nomenclatures.Take(1000).ToList();
        
        return Ok(nomenclatures);
    }
    
    /// <summary>
    /// 1000 продуктов, с их айди по номенклатуре и айди склада
    /// </summary>
    /// <returns></returns>
    [HttpGet("Products")]
    public IActionResult GetProducts()
    {
        var products = _context.Products.Take(1000).ToList();
        
        return Ok(products);
    }
    /// <summary>
    /// 1000 складов
    /// </summary>
    /// <returns></returns>
    [HttpGet("Warehouses")]
    public IActionResult GetWarehouses()
    {
        var warehouses = _context.Warehouses.Take(1000).ToList();
        
        return Ok(warehouses);
    }
    
    /// <summary>
    /// Находим на каком складе продукт по его айди
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("ProductById")]
    public IActionResult GetProductById(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound($"Продукт с Id '{id}' не найден");
        
        
        return Ok(product.WarehouseId);
    }
}