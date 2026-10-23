namespace CSYM505.ORM.Configurations
{
    using CSYM505.Model.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MonthlyPlatformActivityConfiguration : IEntityTypeConfiguration<MonthlyPlatformActivity>
    {
        public void Configure(EntityTypeBuilder<MonthlyPlatformActivity> builder)
        {
            builder.ToTable("tour_ce_omn12");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .HasColumnName("id");

            builder.Property(entity => entity.Frequency)
                   .HasColumnName("freq")
                   .IsRequired();

            builder.Property(entity => entity.Indicator)
                   .HasColumnName("indic_to")
                   .IsRequired();

            builder.Property(entity => entity.Residence)
                   .HasColumnName("c_resid")
                   .IsRequired();

            builder.Property(entity => entity.Month)
                   .HasColumnName("month")
                   .IsRequired();

            builder.Property(entity => entity.Unit)
                   .HasColumnName("unit")
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

            builder.HasIndex(entity => new { entity.Frequency, entity.Indicator, entity.Residence, entity.Month, entity.Unit, entity.Geo, entity.Year })
                   .IsUnique();
        }
    }
}
