using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WowClassicEraItems.Repository.Items;

public class ItemRecordConfiguration : IEntityTypeConfiguration<ItemRecord>
{
    public void Configure(EntityTypeBuilder<ItemRecord> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(x => x.Id);

        // Database-generated surrogate key.
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // The Blizzard item id is an external reference, unique but not the PK.
        // Combined with Updated so re-importing an item is always a plain insert
        // (a new snapshot in time) rather than requiring an upsert.
        builder.Property(x => x.BlizzardItemId)
            .IsRequired();

        builder.HasIndex(x => new { x.BlizzardItemId, x.Updated })
            .IsUnique();

        builder.Property(x => x.RawJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("nvarchar(400)")
            .HasComputedColumnSql("JSON_VALUE([RawJson], '$.name.en_US')", stored: true);

        // Resolved lazily (on first read) and persisted; not derivable from RawJson.
        builder.Property(x => x.Icon)
            .HasColumnType("nvarchar(500)");
    }
}
