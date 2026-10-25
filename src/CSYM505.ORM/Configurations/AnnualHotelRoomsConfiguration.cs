using System;
using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class AnnualHotelRoomsConfiguration : IEntityTypeConfiguration<AnnualHotelRooms>
    {
        public void Configure(EntityTypeBuilder<AnnualHotelRooms> builder)
        {
            builder.ToTable("analytics_annual_hotel_rooms");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Dataset).HasColumnName("dataset").IsRequired();

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.AccommodationType).HasColumnName("nace_r2").IsRequired();

            builder.Property(entity => entity.AccommodationUnit).HasColumnName("accomunit").IsRequired();

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Year).HasColumnName("year");

            builder.Property(entity => entity.Rooms).HasColumnName("rooms");

            builder.Property(entity => entity.PreviousYearRooms).HasColumnName("previous_year_rooms");

            builder.Property(entity => entity.AnnualChangePercent).HasColumnName("annual_change_percent");

            builder.Property(entity => entity.Flags).HasColumnName("flags").IsRequired();

            builder.Property(entity => entity.PreviousYearFlags).HasColumnName("previous_year_flags");

            builder.Property(entity => entity.RawValue).HasColumnName("raw_value").IsRequired();

            builder.HasIndex(entity => new { entity.Geo, entity.Year }).IsUnique();
        }
    }
}
