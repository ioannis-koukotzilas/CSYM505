namespace CSYM505.Model.Entities
{
    public class AnnualDemand
    {
        public int Id { get; set; }

        public string Dataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string AccommodationType { get; set; } = string.Empty;

        public string? Month { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? Nights { get; set; }

        public decimal? PreviousYearNights { get; set; }

        public decimal? AnnualChangePercent { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string? PreviousYearFlags { get; set; }

        public string RawValue { get; set; } = string.Empty;
    }
}
