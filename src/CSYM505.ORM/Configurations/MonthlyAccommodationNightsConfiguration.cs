namespace CSYM505.ORM.Configurations
{
    using CSYM505.Model.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MonthlyAccommodationNightsConfiguration : IEntityTypeConfiguration<MonthlyAccommodationNights>
    {
        public void Configure(EntityTypeBuilder<MonthlyAccommodationNights> builder)
        {
            builder.ToTable("tour_occ_nin2m");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .HasColumnName("id");

            builder.Property(entity => entity.Frequency)
                   .HasColumnName("freq")
                   .IsRequired();

            builder.Property(entity => entity.Residence)
                   .HasColumnName("c_resid")
                   .IsRequired();

            builder.Property(entity => entity.Unit)
                   .HasColumnName("unit")
                   .IsRequired();

            builder.Property(entity => entity.AccommodationType)
                   .HasColumnName("nace_r2")
                   .IsRequired();

            builder.Property(entity => entity.Month)
                   .HasColumnName("month")
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

            builder.HasIndex(entity => new { entity.Frequency, entity.Residence, entity.AccommodationType, entity.Month, entity.Unit, entity.Geo, entity.Year })
                   .IsUnique();
        }
    }
}
