using System;
using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class MonthlyLengthOfStayConfiguration : IEntityTypeConfiguration<MonthlyLengthOfStay>
    {
        public void Configure(EntityTypeBuilder<MonthlyLengthOfStay> builder)
        {
            builder.ToTable("analytics_monthly_length_of_stay");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.NightsDataset).HasColumnName("nights_dataset").IsRequired();

            builder.Property(entity => entity.ArrivalsDataset).HasColumnName("arrivals_dataset").IsRequired();

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.Residence).HasColumnName("c_resid").IsRequired();

            builder.Property(entity => entity.AccommodationType).HasColumnName("nace_r2").IsRequired();

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.TimePeriod).HasColumnName("time_period").IsRequired();

            builder.Property(entity => entity.Nights).HasColumnName("nights");

            builder.Property(entity => entity.Arrivals).HasColumnName("arrivals");

            builder.Property(entity => entity.AverageStayNights).HasColumnName("average_stay_nights");

            builder.Property(entity => entity.NightsFlags).HasColumnName("nights_flags");

            builder.Property(entity => entity.ArrivalsFlags).HasColumnName("arrivals_flags");

            builder.Property(entity => entity.NightsRawValue).HasColumnName("nights_raw_value");

            builder.Property(entity => entity.ArrivalsRawValue).HasColumnName("arrivals_raw_value");

            builder.HasIndex(entity => new { entity.Geo, entity.TimePeriod }).IsUnique();
        }
    }
}
