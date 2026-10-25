using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class AnnualRegionalTravelReceiptsConfiguration : IEntityTypeConfiguration<AnnualRegionalTravelReceipts>
    {
        public void Configure(EntityTypeBuilder<AnnualRegionalTravelReceipts> builder)
        {
            builder.ToTable("bog_regional_travel_receipts");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Region).HasColumnName("region").IsRequired();

            builder.Property(entity => entity.Country).HasColumnName("country").IsRequired();

            builder.Property(entity => entity.Year).HasColumnName("year");

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Value).HasColumnName("value");

            builder.Property(entity => entity.Flags).HasColumnName("flags").IsRequired();

            builder.Property(entity => entity.RawValue).HasColumnName("raw_value").IsRequired();

            builder
                .HasIndex(entity => new
                {
                    entity.Frequency,
                    entity.Geo,
                    entity.Country,
                    entity.Year,
                    entity.Unit,
                })
                .IsUnique();
        }
    }
}
