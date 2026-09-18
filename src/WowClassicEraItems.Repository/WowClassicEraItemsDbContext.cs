using Microsoft.EntityFrameworkCore;
using WowClassicEraItems.Repository.Items;

namespace WowClassicEraItems.Repository;

public class WowClassicEraItemsDbContext(DbContextOptions<WowClassicEraItemsDbContext> options) : DbContext(options)
{
    public DbSet<ItemRecord> Items => Set<ItemRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WowClassicEraItemsDbContext).Assembly);
    }
}
