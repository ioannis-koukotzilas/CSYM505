using System;
using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class AnnualPlatformDemandConfiguration : IEntityTypeConfiguration<AnnualPlatformDemand>
    {
        public void Configure(EntityTypeBuilder<AnnualPlatformDemand> builder)
        {
            builder.ToTable("analytics_annual_platform_demand");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Dataset).HasColumnName("dataset").IsRequired();

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.Indicator).HasColumnName("indic_to").IsRequired();

            builder.Property(entity => entity.Residence).HasColumnName("c_resid").IsRequired();

            builder.Property(entity => entity.Month).HasColumnName("month");

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Year).HasColumnName("year");

            builder.Property(entity => entity.GuestNights).HasColumnName("guest_nights");

            builder.Property(entity => entity.PreviousYearGuestNights).HasColumnName("previous_year_guest_nights");

            builder.Property(entity => entity.AnnualChangePercent).HasColumnName("annual_change_percent");

            builder.Property(entity => entity.Flags).HasColumnName("flags").IsRequired();

            builder.Property(entity => entity.PreviousYearFlags).HasColumnName("previous_year_flags");

            builder.Property(entity => entity.RawValue).HasColumnName("raw_value").IsRequired();

            builder.HasIndex(entity => new { entity.Geo, entity.Year }).IsUnique();
        }
    }
}
