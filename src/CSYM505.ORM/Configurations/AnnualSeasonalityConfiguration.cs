using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class AnnualSeasonalityConfiguration : IEntityTypeConfiguration<AnnualSeasonality>
    {
        public void Configure(EntityTypeBuilder<AnnualSeasonality> builder)
        {
            builder.ToTable("analytics_annual_seasonality");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Dataset).HasColumnName("dataset").IsRequired();

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.Residence).HasColumnName("c_resid").IsRequired();

            builder.Property(entity => entity.AccommodationType).HasColumnName("nace_r2").IsRequired();

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Year).HasColumnName("year");

            builder.Property(entity => entity.SummerNights).HasColumnName("summer_nights");

            builder.Property(entity => entity.AnnualNights).HasColumnName("annual_nights");

            builder.Property(entity => entity.SummerSharePercent).HasColumnName("summer_share_percent");

            builder.Property(entity => entity.SummerFlags).HasColumnName("summer_flags");

            builder.Property(entity => entity.AnnualFlags).HasColumnName("annual_flags");

            builder.Property(entity => entity.AnnualRawValue).HasColumnName("annual_raw_value");

            builder.Property(entity => entity.MeanMonthlyNights).HasColumnName("mean_monthly_nights");

            builder.Property(entity => entity.PopulationStandardDeviation).HasColumnName("population_standard_deviation");

            builder.Property(entity => entity.CoefficientOfVariationPercent).HasColumnName("coefficient_of_variation_percent");

            builder.HasIndex(entity => new { entity.Geo, entity.Year }).IsUnique();
        }
    }
}
