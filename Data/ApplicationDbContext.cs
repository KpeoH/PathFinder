using Microsoft.EntityFrameworkCore;
using pathfinder.Models;

namespace pathfinder.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cities> Cities { get; set; }
    public DbSet<Nomenclatures> Nomenclatures { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Warehouses> Warehouses { get; set; }
    public DbSet<ProductWarehouseHistory> ProductWarehouseHistories { get; set; }
    public DbSet<WarehouseConnection> WarehouseConnections { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WarehouseConnection>()
            .HasKey(wc => new { wc.FromWarehouseId, wc.ToWarehouseId });
        
        base.OnModelCreating(modelBuilder);
    }
    
}