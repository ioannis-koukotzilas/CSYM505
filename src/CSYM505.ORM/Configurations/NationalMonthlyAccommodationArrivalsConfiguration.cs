namespace CSYM505.ORM.Configurations
{
    using CSYM505.Model.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NationalMonthlyAccommodationArrivalsConfiguration : IEntityTypeConfiguration<NationalMonthlyAccommodationArrivals>
    {
        public void Configure(EntityTypeBuilder<NationalMonthlyAccommodationArrivals> builder)
        {
            builder.ToTable("tour_occ_arm");

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

            builder.Property(entity => entity.Geo)
                   .HasColumnName("geo")
                   .IsRequired();

            builder.Property(entity => entity.TimePeriod)
                   .HasColumnName("time_period")
                   .IsRequired();

            builder.Property(entity => entity.Value)
                   .HasColumnName("value");

            builder.Property(entity => entity.Flags)
                   .HasColumnName("flags")
                   .IsRequired();

            builder.Property(entity => entity.RawValue)
                   .HasColumnName("raw_value")
                   .IsRequired();

            builder.HasIndex(entity => new { entity.Frequency, entity.Residence, entity.Unit, entity.AccommodationType, entity.Geo, entity.TimePeriod })
                   .IsUnique();
        }
    }
}
