using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using pathfinder.Services;
using pathfinder.Models;

namespace pathfinder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    /// <summary>
    /// Тут работаем с товаром
    /// </summary>
    private readonly WarehouseService _warehouseService;

    public WarehouseController(WarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    public class MoveProductRequest
    {
        public int ProductId { get; set; }
        public int TargetWarehouseId { get; set; }
    }
    

    /// <summary>
    /// Перемещаем товар на нужный нам склад
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("move-product")]
    public async Task<IActionResult> MoveProduct([FromBody] MoveProductRequest request)
    {
        var success = await _warehouseService.MoveProductToWarehouse(request.ProductId, request.TargetWarehouseId);

        if (success)
        {
            return Ok("Товар успешно перемещен.");
        }

        return BadRequest("Не удалось переместить товар. Пожалуйста, проверьте данные.");
    }
    
    /// <summary>
    /// Получаем историю перемещения товара (используя запрос выше)
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [HttpGet("product-history/{productId}")]
    public async Task<IActionResult> GetProductHistory(int productId)
    {
        var history = await _warehouseService.GetProductHistory(productId);

        if (history == null || !history.Any())
        {
            return NotFound("История перемещений товара не найдена.");
        }

        var result = history.Select(h => new 
        {
            productId = h.ProductId,
            WarehouseId = h.WarehouseId,
            MovedAt = h.MovedAt,
        }).ToList(); 

        return Ok(result);
    }

    

    /// <summary>
    /// Получаем количество шагов для достижения результата (сам товар не передвигается)
    /// </summary>
    /// <param name="productId" description="Айди продукта который мы хотим переместить"></param>
    /// <param name="targetWarehouseId" description="Айди склада на который мы хотим перенести вышеуказанный продукт"></param>
    /// <returns></returns>
    [HttpGet("path/{productId}")]
    public async Task<IActionResult> GetWarehousePath(int productId, [FromQuery] int targetWarehouseId)
    {
        // Получаем путь из сервиса
        var path = await _warehouseService.FindWarehousePath(productId, targetWarehouseId);
    
        // Логирование
        Console.WriteLine($"Path count: {path?.Count ?? 0}");
    
        // Если путь пустой или не найден, возвращаем ошибку
        if (path == null || !path.Any())
        {
            return NotFound(new { Message = "Путь не найден" });
        }
    
        // Если путь состоит из одного склада
        if (path.Count == 1)
        {
            return Ok(new
            {
                Message = "Найден единственный путь: товар уже на нужном складе.",
                Path = path.Select(w => new
                {
                    w.From.Id,
                    w.From.Street,
                    City = w.From.City ?? "Неизвестно",
                    w.Description
                }).ToList(),
                TargetWarehouseId = targetWarehouseId
            });
        }
    
        // Если путь состоит из нескольких элементов
        return Ok(new
        {
            Message = $"Количество шагов: {path.Count}",
            Path = path.Select(w => new
            {
                w.From.Id,
                w.From.Street,
                City = w.From.City ?? "Неизвестно",
                w.Description
            }).ToList(),
            TargetWarehouseId = targetWarehouseId
        });
    }



}