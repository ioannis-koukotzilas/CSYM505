using System;
using CSYM505.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSYM505.ORM.Configurations
{
    public class VisitorCompositionConfiguration : IEntityTypeConfiguration<VisitorComposition>
    {
        public void Configure(EntityTypeBuilder<VisitorComposition> builder)
        {
            builder.ToTable("analytics_visitor_composition");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id).HasColumnName("id");

            builder.Property(entity => entity.Dataset).HasColumnName("dataset").IsRequired();

            builder.Property(entity => entity.Frequency).HasColumnName("freq").IsRequired();

            builder.Property(entity => entity.AccommodationType).HasColumnName("nace_r2").IsRequired();

            builder.Property(entity => entity.Month).HasColumnName("month").IsRequired();

            builder.Property(entity => entity.Unit).HasColumnName("unit").IsRequired();

            builder.Property(entity => entity.Geo).HasColumnName("geo").IsRequired();

            builder.Property(entity => entity.Year).HasColumnName("year");

            builder.Property(entity => entity.DomesticNights).HasColumnName("domestic_nights");

            builder.Property(entity => entity.ForeignNights).HasColumnName("foreign_nights");

            builder.Property(entity => entity.TotalNights).HasColumnName("total_nights");

            builder.Property(entity => entity.ForeignSharePercent).HasColumnName("foreign_share_percent");

            builder.Property(entity => entity.DomesticFlags).HasColumnName("domestic_flags");

            builder.Property(entity => entity.ForeignFlags).HasColumnName("foreign_flags");

            builder.Property(entity => entity.TotalFlags).HasColumnName("total_flags");

            builder.Property(entity => entity.DomesticRawValue).HasColumnName("domestic_raw_value");

            builder.Property(entity => entity.ForeignRawValue).HasColumnName("foreign_raw_value");

            builder.Property(entity => entity.TotalRawValue).HasColumnName("total_raw_value");

            builder.HasIndex(entity => new { entity.Geo, entity.Year }).IsUnique();
        }
    }
}
