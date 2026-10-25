namespace CSYM505.ORM.Context
{
    using CSYM505.Model.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        // Eurostat
        public DbSet<AnnualAccommodationNights> AnnualAccommodationNights { get; set; } = null!;

        public DbSet<MonthlyAccommodationNights> MonthlyAccommodationNights { get; set; } = null!;

        public DbSet<NationalMonthlyAccommodationNights> NationalMonthlyAccommodationNights { get; set; } = null!;

        public DbSet<NationalMonthlyAccommodationArrivals> NationalMonthlyAccommodationArrivals { get; set; } = null!;

        public DbSet<AnnualAccommodationArrivals> AnnualAccommodationArrivals { get; set; } = null!;

        public DbSet<MonthlyPlatformActivity> MonthlyPlatformActivity { get; set; } = null!;

        public DbSet<NationalMonthlyPlatformActivity> NationalMonthlyPlatformActivity { get; set; } = null!;

        public DbSet<AnnualPlatformGuestNights> AnnualPlatformGuestNights { get; set; } = null!;

        public DbSet<AnnualAccommodationCapacity> AnnualAccommodationCapacity { get; set; } = null!;

        public DbSet<AnnualHotelOccupancy> AnnualHotelOccupancy { get; set; } = null!;

        // Analytics
        public DbSet<AnnualDemand> AnnualDemand { get; set; } = null!;

        public DbSet<MonthlyDemand> MonthlyDemand { get; set; } = null!;

        public DbSet<AnnualSeasonality> AnnualSeasonality { get; set; } = null!;

        public DbSet<VisitorComposition> VisitorComposition { get; set; } = null!;

        public DbSet<MonthlyLengthOfStay> MonthlyLengthOfStay { get; set; } = null!;

        public DbSet<AnnualHotelRooms> AnnualHotelRooms { get; set; } = null!;

        public DbSet<AnnualRoomOccupancy> AnnualRoomOccupancy { get; set; } = null!;

        public DbSet<AnnualPlatformDemand> AnnualPlatformDemand { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
