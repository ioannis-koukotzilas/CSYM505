using System;

namespace CSYM505.Model.Entities
{
    public class AnnualRoomOccupancy
    {
        public int Id { get; set; }

        public string Dataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string AccommodationUnit { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? OccupancyPercent { get; set; }

        public decimal? PreviousYearOccupancyPercent { get; set; }

        public decimal? AnnualChangePercentagePoints { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string? PreviousYearFlags { get; set; }

        public string RawValue { get; set; } = string.Empty;
    }
}
