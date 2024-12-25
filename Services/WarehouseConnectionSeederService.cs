using Microsoft.EntityFrameworkCore;

namespace pathfinder.Services;

using pathfinder.Models;
using pathfinder.Data;

// Используем 1 раз при первой инициализации БД

public class WarehouseConnectionSeederService
{
    private readonly ApplicationDbContext _context;

        public WarehouseConnectionSeederService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedWarehouseConnectionsAsync()
        {
            // Получаем все склады, группируем по городам
            var warehousesByCity = await _context.Warehouses.Include(w => w.City)
                .GroupBy(w => w.CityId) 
                .ToListAsync();

            var connections = new List<WarehouseConnection>();

            // Создаем соединения между складами одного города
            foreach (var cityGroup in warehousesByCity)
            {
                var cityId = cityGroup.Key;
                var warehousesInCity = cityGroup.ToList();

                for (int i = 0; i < warehousesInCity.Count - 1; i++)
                {
                    var fromWarehouse = warehousesInCity[i];
                    var toWarehouse = warehousesInCity[i + 1];

                    var distance = CalculateDistance(fromWarehouse, toWarehouse);  // Вычисляем расстояние между складами

                    if (distance > 0)
                    {
                        // Добавляем связь от склада A к складу B
                        connections.Add(new WarehouseConnection
                        {
                            FromWarehouseId = fromWarehouse.Id,
                            ToWarehouseId = toWarehouse.Id,
                            Distance = distance,
                            CityId = cityId 
                        });

                        // Добавляем обратную связь от склада B к складу A
                        connections.Add(new WarehouseConnection
                        {
                            FromWarehouseId = toWarehouse.Id,
                            ToWarehouseId = fromWarehouse.Id,
                            Distance = distance,
                            CityId = cityId 
                        });
                    }
                }
            }

            // Создаем связи между складами разных городов
            var allWarehouses = await _context.Warehouses.Include(w => w.City).ToListAsync();
            for (int i = 0; i < allWarehouses.Count - 1; i++)
            {
                var fromWarehouse = allWarehouses[i];
                var toWarehouse = allWarehouses[i + 1];

                // Проверяем, находятся ли склады в разных городах
                if (fromWarehouse.CityId != toWarehouse.CityId)
                {
                    var distance = CalculateDistance(fromWarehouse, toWarehouse);

                    if (distance > 0)
                    {
                        // Создаем связь от склада A к складу B
                        connections.Add(new WarehouseConnection
                        {
                            FromWarehouseId = fromWarehouse.Id,
                            ToWarehouseId = toWarehouse.Id,
                            Distance = distance,
                            CityId = fromWarehouse.CityId  // Связываем с городом первого склада
                        });

                        // Создаем обратную связь от склада B к складу A
                        connections.Add(new WarehouseConnection
                        {
                            FromWarehouseId = toWarehouse.Id,
                            ToWarehouseId = fromWarehouse.Id,
                            Distance = distance,
                            CityId = toWarehouse.CityId  // Связываем с городом второго склада
                        });
                    }
                }
            }

            // Добавляем все соединения в базу данных
            if (connections.Any())
            {
                await _context.WarehouseConnections.AddRangeAsync(connections);
                await _context.SaveChangesAsync();
            }
        }

        private int CalculateDistance(Warehouses fromWarehouse, Warehouses toWarehouse)
        {
            return new Random().Next(10, 100); // Примерное расстояние от 10 до 100 км
        }
}