namespace CSYM505.Model.Entities
{
    public class VisitorComposition
    {
        public int Id { get; set; }

        public string Dataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string AccommodationType { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? DomesticNights { get; set; }

        public decimal? ForeignNights { get; set; }

        public decimal? TotalNights { get; set; }

        public decimal? ForeignSharePercent { get; set; }

        public string? DomesticFlags { get; set; }

        public string? ForeignFlags { get; set; }

        public string? TotalFlags { get; set; }

        public string? DomesticRawValue { get; set; }

        public string? ForeignRawValue { get; set; }

        public string? TotalRawValue { get; set; }
    }
}
