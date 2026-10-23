using System;

namespace CSYM505.Model.Entities
{
    public class AnnualSeasonality
    {
        public int Id { get; set; }

        public string Dataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string AccommodationType { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? SummerNights { get; set; }

        public decimal? AnnualNights { get; set; }

        public decimal? SummerSharePercent { get; set; }

        public string? SummerFlags { get; set; }

        public string? AnnualFlags { get; set; }

        public string? AnnualRawValue { get; set; }

        public double? MeanMonthlyNights { get; set; }

        public double? PopulationStandardDeviation { get; set; }

        public double? CoefficientOfVariationPercent { get; set; }
    }
}
