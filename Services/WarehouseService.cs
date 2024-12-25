namespace pathfinder.Services;
using pathfinder.Models;
using pathfinder.Data;
using Microsoft.EntityFrameworkCore;

public class WarehouseService
{
    private readonly ApplicationDbContext _context;

    public WarehouseService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public class PathStep
    {
        public WarehouseInfo From { get; set; }
        public WarehouseInfo To { get; set; }
        public string Description { get; set; }
    }

    public class WarehouseInfo
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }


        public WarehouseInfo(Warehouses warehouse)
        {
            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse), "Warehouse не может быть null.");
            }

            Id = warehouse.Id;
            Street = warehouse.Street;
            City = warehouse.City?.Name;   
        }
    }


    // Ищем путь до нужного склада
    public async Task<List<PathStep>> FindWarehousePath(int productId, int targetWarehouseId)
    {
        var product = await _context.Products
            .Include(p => p.Warehouse)
                .ThenInclude(w => w.City)
            .FirstOrDefaultAsync(p => p.Id == productId);
     
        if (product == null)
        {
            Console.WriteLine($"Продукт с ID {productId} не найден.");
            return null;
        }

        var startWarehouse = product.Warehouse;

        if (startWarehouse == null)
        {
            Console.WriteLine($"Склад для продукта с ID {productId} не найден.");
            return null;
        }

        if (startWarehouse.City == null)
        {
            Console.WriteLine($"Город для склада {startWarehouse.Id} не найден.");
            return null;
        }

        if (startWarehouse.Id == targetWarehouseId)
        {
            return new List<PathStep>
            {
                new PathStep
                {
                    From = new WarehouseInfo(startWarehouse),
                    To = new WarehouseInfo(startWarehouse),
                    Description = $"Товар уже находится на складе {startWarehouse.Id} в городе {startWarehouse.City.Name}"
                }
            };
        }

        var visited = new HashSet<int>();
        var queue = new Queue<List<Warehouses>>();
        queue.Enqueue(new List<Warehouses> { startWarehouse });
        visited.Add(startWarehouse.Id);

        var pathStepsResult = new List<PathStep>();

        while (queue.Any())
        {
            var path = queue.Dequeue();
            var currentWarehouse = path.Last();

            Console.WriteLine($"Обрабатываем склад {currentWarehouse.Id}. Статус очереди: {queue.Count}");

            if (currentWarehouse.Id == targetWarehouseId)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    var fromWarehouse = path[i - 1];
                    var toWarehouse = path[i];

                    var fromCity = fromWarehouse.City?.Name ?? "Неизвестно";
                    var toCity = toWarehouse.City?.Name ?? "Неизвестно";

                    pathStepsResult.Add(new PathStep
                    {
                        From = new WarehouseInfo(fromWarehouse),
                        To = new WarehouseInfo(toWarehouse),
                        Description = $"Переезд товара со склада {fromWarehouse.Id} в город {fromCity} на склад {toWarehouse.Id} в город {toCity}"
                    });
                }

                return pathStepsResult;
            }

            // Ищем соседей, учитывая связи между складами
            var neighbors = await _context.WarehouseConnections
                .Where(wc => wc.FromWarehouseId == currentWarehouse.Id && !visited.Contains(wc.ToWarehouseId))
                .Include(wc => wc.ToWarehouse)
                    .ThenInclude(w => w.City)
                .ToListAsync();

            if (neighbors.Count == 0)
            {
                Console.WriteLine($"Нет соседей для склада {currentWarehouse.Id}");
            }

            foreach (var connection in neighbors)
            {
                var nextWarehouse = connection.ToWarehouse;
                if (!visited.Contains(nextWarehouse.Id))
                {
                    visited.Add(nextWarehouse.Id);
                    var newPath = new List<Warehouses>(path) { nextWarehouse };
                    queue.Enqueue(newPath);
                }
            }
        }

        Console.WriteLine("Путь не найден.");
        return null;
    }







    public async Task<bool> MoveProductToWarehouse(int productId, int targetWarehouseId)
    {
        // Получаем товар и склад, куда нужно переместить
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
        var targetWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == targetWarehouseId);

        if (product == null || targetWarehouse == null)
        {
            return false; 
        }


        var productHistory = new ProductWarehouseHistory
        {
            ProductId = productId,
            WarehouseId = targetWarehouseId,
            MovedAt = DateTime.UtcNow
        };

        await _context.ProductWarehouseHistories.AddAsync(productHistory);


        product.WarehouseId = targetWarehouseId;
        _context.Products.Update(product);

        await _context.SaveChangesAsync();

        return true; 
    }

    // Получение истории перемещений товара
    public async Task<List<ProductWarehouseHistory>> GetProductHistory(int productId)
    {
        return await _context.ProductWarehouseHistories
            .Where(h => h.ProductId == productId)
            .Include(h => h.Warehouse)  
            .OrderBy(h => h.MovedAt)    
            .ToListAsync();
    }

}