using System;

namespace CSYM505.Model.Entities
{
    public class MonthlyLengthOfStay
    {
        public int Id { get; set; }

        public string NightsDataset { get; set; } = string.Empty;

        public string ArrivalsDataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string AccommodationType { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public string TimePeriod { get; set; } = string.Empty;

        public decimal? Nights { get; set; }

        public decimal? Arrivals { get; set; }

        public decimal? AverageStayNights { get; set; }

        public string? NightsFlags { get; set; }

        public string? ArrivalsFlags { get; set; }

        public string? NightsRawValue { get; set; }

        public string? ArrivalsRawValue { get; set; }
    }
}
