using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class MonthlyForecastConfiguration : IEntityTypeConfiguration<MonthlyForecast>
    {
        public void Configure(EntityTypeBuilder<MonthlyForecast> builder)
        {
            builder.ToTable("analytics_monthly_forecast");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Series).HasColumnName("series").IsRequired();

            builder.Property(entity => entity.Dataset).HasColumnName("dataset").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.TrainingStartMonth).HasColumnName("training_start_month");

            builder.Property(entity => entity.TrainingEndMonth).HasColumnName("training_end_month");

            builder.Property(entity => entity.HorizonMonths).HasColumnName("horizon_months");

            builder.Property(entity => entity.Month).HasColumnName("month");

            builder.Property(entity => entity.Method).HasColumnName("method").IsRequired();

            builder.Property(entity => entity.PredictedValue).HasColumnName("predicted_value").HasColumnType("REAL");

            builder
                .HasIndex(entity => new
                {
                    entity.Series,
                    entity.Geo,
                    entity.Month,
                })
                .IsUnique();
        }
    }
}
