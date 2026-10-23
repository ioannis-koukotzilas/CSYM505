namespace CSYM505.ORM.Configurations
{
    using CSYM505.Model.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AnnualAccommodationCapacityConfiguration : IEntityTypeConfiguration<AnnualAccommodationCapacity>
    {
        public void Configure(EntityTypeBuilder<AnnualAccommodationCapacity> builder)
        {
            builder.ToTable("tour_cap_nuts2");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .HasColumnName("id");

            builder.Property(entity => entity.Frequency)
                   .HasColumnName("freq")
                   .IsRequired();

            builder.Property(entity => entity.AccommodationUnit)
                   .HasColumnName("accomunit")
                   .IsRequired();

            builder.Property(entity => entity.Unit)
                   .HasColumnName("unit")
                   .IsRequired();

            builder.Property(entity => entity.AccommodationType)
                   .HasColumnName("nace_r2")
                   .IsRequired();

            builder.Property(entity => entity.Geo)
                   .HasColumnName("geo")
                   .IsRequired();

            builder.Property(entity => entity.Year)
                   .HasColumnName("year");

            builder.Property(entity => entity.Value)
                   .HasColumnName("value");

            builder.Property(entity => entity.Flags)
                   .HasColumnName("flags")
                   .IsRequired();

            builder.Property(entity => entity.RawValue)
                   .HasColumnName("raw_value")
                   .IsRequired();

            builder.HasIndex(entity => new { entity.Frequency, entity.AccommodationUnit, entity.Unit, entity.AccommodationType, entity.Geo, entity.Year })
                   .IsUnique();
        }
    }
}
